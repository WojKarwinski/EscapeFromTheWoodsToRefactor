using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EscapeFromTheWoods
{
    public class Wood
    {
        private const int drawingFactor = 8;
        private string path;
        private DBwriter db;
        private Random r = new Random(1);
        public int woodID { get; private set; }
        private Map map;
        private Dictionary<int, Tree> trees;
        private Dictionary<int, Monkey> monkeys;

        public Wood(int woodID, Dictionary<int, Tree> trees, Map map, string path, DBwriter db)
        {
            this.woodID = woodID;
            this.trees = trees;
            this.monkeys = new Dictionary<int, Monkey>();
            this.map = map;
            this.path = path;
            this.db = db;
        }

        // Place monkey asynchronously
        public Task PlaceMonkeyAsync(string monkeyName, int monkeyID)
        {
            Tree selectedTree = null;
            int treeCount = 0;

            foreach(var tree in trees.Values)
            {
                if(!tree.hasMonkey)
                {
                    if(r.Next(++treeCount) == 0)
                    {
                        selectedTree = tree;
                    }
                }
            }

            if(selectedTree == null)
            {
                throw new InvalidOperationException("No available trees to place a monkey.");
            }

            Monkey m = new Monkey(monkeyID, monkeyName, selectedTree);
            lock(monkeys) // one thread at a time
            {
                monkeys.Add(monkeyID, m);
            }
            selectedTree.hasMonkey = true;

            return Task.CompletedTask;
        }

        // Monkey escape async
        public async Task EscapeAsync()
        {
            var escapeTasks = monkeys.Values.Select(EscapeMonkey).ToList();
            var routes = await Task.WhenAll(escapeTasks);
            WriteEscaperoutesToBitmap(routes.ToList());
        }

        // Write route to database
        private async Task WriteRouteToDBAsync(Monkey monkey, List<Tree> route)
        {
            List<DBMonkeyRecord> records = new List<DBMonkeyRecord>();
            for(int j = 0; j < route.Count; j++)
            {
                records.Add(new DBMonkeyRecord(monkey.monkeyID, monkey.name, woodID, j, route[j].treeID, route[j].x, route[j].y));
            }
            await db.WriteMonkeyRecords(records);
        }

        // Write escape routes to bitmap
        public void WriteEscaperoutesToBitmap(List<List<Tree>> routes)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} start");

            try
            {
                using(Bitmap bm = InitializeBitmap())
                using(Graphics g = Graphics.FromImage(bm))
                {
                    DrawTrees(g);
                    DrawRoutes(routes, g);
                    SaveBitmap(bm);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error occurred while writing escape routes: {ex.Message}");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} end");
        }

        private Bitmap InitializeBitmap()
        {
            return new Bitmap((map.xmax - map.xmin) * drawingFactor, (map.ymax - map.ymin) * drawingFactor);
        }

        private void DrawTrees(Graphics g)
        {
            using(Pen p = new Pen(Color.Green, 1))
            {
                foreach(Tree t in trees.Values)
                {
                    int x = t.x * drawingFactor;
                    int y = t.y * drawingFactor;
                    g.DrawEllipse(p, x, y, drawingFactor, drawingFactor);
                }
            }
        }

        private void DrawRoutes(List<List<Tree>> routes, Graphics g)
        {
            Color[] cvalues = new Color[] { Color.Red, Color.Yellow, Color.Blue, Color.Cyan, Color.GreenYellow };
            int colorN = 0;

            foreach(List<Tree> route in routes)
            {
                DrawSingleRoute(g, route, cvalues[colorN % cvalues.Length]);
                colorN++;
            }
        }

        private void DrawSingleRoute(Graphics g, List<Tree> route, Color color)
        {
            int delta = drawingFactor / 2;
            using(Pen pen = new Pen(color, 1))
            {
                int p1x = route[0].x * drawingFactor + delta;
                int p1y = route[0].y * drawingFactor + delta;
                g.DrawEllipse(pen, p1x - delta, p1y - delta, drawingFactor, drawingFactor);
                g.FillEllipse(new SolidBrush(color), p1x - delta, p1y - delta, drawingFactor, drawingFactor);

                for(int i = 1; i < route.Count; i++)
                {
                    int nextX = route[i].x * drawingFactor + delta;
                    int nextY = route[i].y * drawingFactor + delta;
                    g.DrawLine(pen, p1x, p1y, nextX, nextY);
                    p1x = nextX;
                    p1y = nextY;
                }
            }
        }

        private void SaveBitmap(Bitmap bm)
        {
            bm.Save(Path.Combine(path, woodID.ToString() + "_escapeRoutes.jpg"), ImageFormat.Jpeg);
        }

        // Single monkey escape
        public async Task<List<Tree>> EscapeMonkey(Monkey monkey)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{woodID}:start {woodID},{monkey.name}");

            HashSet<int> visited = new HashSet<int> { monkey.tree.treeID };
            List<Tree> route = new List<Tree>() { monkey.tree };
            var candidateTrees = new HashSet<Tree>(trees.Values.Where(t => !t.hasMonkey));

            while(true)
            {
                double distanceToBorder = DistanceToBorder(monkey.tree);
                Tree closestTree = null;
                double minDistance = double.MaxValue;

                foreach(var t in candidateTrees)
                {
                    double distance = Distance(monkey.tree, t);
                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        closestTree = t;
                    }
                }

                if(closestTree == null || minDistance > distanceToBorder)
                {
                    await WriteRouteToDBAsync(monkey, route);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
                    return route;
                }

                route.Add(closestTree);
                monkey.tree = closestTree;
                visited.Add(closestTree.treeID);
                candidateTrees.Remove(closestTree);

                // Update candidateTrees based on the new position of the monkey
                candidateTrees.RemoveWhere(t => t.hasMonkey || visited.Contains(t.treeID));
            }
        }


        // Writes wood data to database
        public async Task WriteWoodToDBAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{woodID}:write db wood {woodID} start");
            List<DBWoodRecord> records = trees.Values.Select(t =>
                new DBWoodRecord(woodID, t.treeID, t.x, t.y)).ToList();

            await db.WriteWoodRecords(records);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{woodID}:write db wood {woodID} end");
        }

        // Calculates distance between two trees
        private double Distance(Tree a, Tree b)
        {
            return Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
        }

        // Calculates distance to nearest border
        private double DistanceToBorder(Tree tree)
        {
            return Math.Min(Math.Min(tree.x - map.xmin, map.xmax - tree.x),
                            Math.Min(tree.y - map.ymin, map.ymax - tree.y));
        }
    }
}

