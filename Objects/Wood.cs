using EscapeFromTheWoods.Objects;
using System;
using System.Collections.Concurrent;
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
        private Random random = new(1);
        public int woodID { get; private set; }
        private Map map;
        private List<Tree> trees;
        private ConcurrentDictionary<int, Monkey> monkeys;

        public Wood(int woodID, List<Tree> trees, Map map, string path, DBwriter db)
        {
            this.woodID = woodID;
            this.trees = trees;
            this.monkeys = new ConcurrentDictionary<int, Monkey>();
            this.map = map;
            this.path = path;
            this.db = db;
        }

        // Place monkey
        public void PlaceMonkeyAsync(string monkeyName, int monkeyID)
        {

            int treeNr;
            do
            {
                treeNr = random.Next(0, trees.Count - 1);
            }
            while(trees[treeNr].hasMonkey);
            Monkey m = new(monkeyID, monkeyName, trees[treeNr]);
            monkeys.TryAdd(m.monkeyID, m);
            trees[treeNr].hasMonkey = true;

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
            List<DBMonkeyRecord> records = new();
            for(int j = 0; j < route.Count; j++)
            {
                records.Add(new DBMonkeyRecord(monkey.monkeyID, monkey.name, woodID, j, route[j].treeID, route[j].x, route[j].y));
            }
            GameMonkeyRecord gmr = new(records);

            await db.WriteMonkeyRecords(gmr);
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
            using(Pen p = new(Color.Green, 1))
            {
                foreach(Tree t in trees)
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
            using(Pen pen = new(color, 1))
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

            HashSet<int> visited = new() { monkey.tree.treeID };
            List<Tree> route = new() { monkey.tree };
            var candidateTrees = new HashSet<Tree>(trees.Where(t => !t.hasMonkey));

            while(true)
            {
                // Directly calculate distance to border
                double distanceToBorder = Math.Min(Math.Min(monkey.tree.x - map.xmin, map.xmax - monkey.tree.x),
                                                   Math.Min(monkey.tree.y - map.ymin, map.ymax - monkey.tree.y));
                Tree closestTree = null;
                double minDistance = double.MaxValue;

                foreach(var t in candidateTrees)
                {
                    // Ddistance between two trees
                    double distance = Math.Sqrt(Math.Pow(monkey.tree.x - t.x, 2) + Math.Pow(monkey.tree.y - t.y, 2));

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
            List<DBWoodRecord> records = trees.Select(t =>
                new DBWoodRecord(woodID, t.treeID, t.x, t.y)).ToList();
            GameWoodRecord gwr = new(records);
            await db.WriteWoodRecords(gwr);
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

