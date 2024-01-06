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
        public int woodID { get; set; }
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

        public Task PlaceMonkeyAsync(string monkeyName, int monkeyID)
        {
            var availableTrees = trees.Values.Where(t => !t.hasMonkey).ToList();
            if(availableTrees.Count == 0)
            {
                throw new InvalidOperationException("No available trees to place a monkey.");
            }

            Tree selectedTree = availableTrees[r.Next(availableTrees.Count)];
            Monkey m = new Monkey(monkeyID, monkeyName, selectedTree);
            lock(monkeys)
            {
                monkeys.Add(monkeyID, m);
            }
            selectedTree.hasMonkey = true;

            return Task.CompletedTask;
        }

        public async Task EscapeAsync()
        {
            var escapeTasks = monkeys.Values.Select(monkey => EscapeMonkey(monkey)).ToList();
            var routes = await Task.WhenAll(escapeTasks);
            WriteEscaperoutesToBitmap(routes.ToList());
        }

        private async Task writeRouteToDBAsync(Monkey monkey, List<Tree> route)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"{woodID}:write db routes {woodID},{monkey.name} start");
            List<DBMonkeyRecord> records = new List<DBMonkeyRecord>();
            for(int j = 0; j < route.Count; j++)
            {
                records.Add(new DBMonkeyRecord(monkey.monkeyID, monkey.name, woodID, j, route[j].treeID, route[j].x, route[j].y));
            }
            await db.WriteMonkeyRecords(records);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"{woodID}:write db routes {woodID},{monkey.name} end");
        }

        public void WriteEscaperoutesToBitmap(List<List<Tree>> routes)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} start");

            try
            {
                Color[] cvalues = new Color[] { Color.Red, Color.Yellow, Color.Blue, Color.Cyan, Color.GreenYellow };
                using(Bitmap bm = new Bitmap((map.xmax - map.xmin) * drawingFactor, (map.ymax - map.ymin) * drawingFactor))
                using(Graphics g = Graphics.FromImage(bm))
                {
                    int delta = drawingFactor / 2;
                    using(Pen p = new Pen(Color.Green, 1))
                    {
                        foreach(Tree t in trees.Values)
                        {
                            g.DrawEllipse(p, t.x * drawingFactor, t.y * drawingFactor, drawingFactor, drawingFactor);
                        }
                    }

                    int colorN = 0;
                    foreach(List<Tree> route in routes)
                    {
                        int p1x = route[0].x * drawingFactor + delta;
                        int p1y = route[0].y * drawingFactor + delta;
                        Color color = cvalues[colorN % cvalues.Length];
                        using(Pen pen = new Pen(color, 1))
                        {
                            g.DrawEllipse(pen, p1x - delta, p1y - delta, drawingFactor, drawingFactor);
                            g.FillEllipse(new SolidBrush(color), p1x - delta, p1y - delta, drawingFactor, drawingFactor);

                            for(int i = 1; i < route.Count; i++)
                            {
                                g.DrawLine(pen, p1x, p1y, route[i].x * drawingFactor + delta, route[i].y * drawingFactor + delta);
                                p1x = route[i].x * drawingFactor + delta;
                                p1y = route[i].y * drawingFactor + delta;
                            }
                        }
                        colorN++;
                    }
                    bm.Save(Path.Combine(path, woodID.ToString() + "_escapeRoutes.jpg"), ImageFormat.Jpeg);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error occurred while writing escape routes: {ex.Message}");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{woodID}:write bitmap routes {woodID} end");
        }

        public async Task WriteWoodToDBAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{woodID}:write db wood {woodID} start");
            List<DBWoodRecord> records = new List<DBWoodRecord>();
            foreach(Tree t in trees.Values)
            {
                records.Add(new DBWoodRecord(woodID, t.treeID, t.x, t.y));
            }
            await db.WriteWoodRecords(records);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{woodID}:write db wood {woodID} end");
        }
        public async Task<List<Tree>> EscapeMonkey(Monkey monkey)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{woodID}:start {woodID},{monkey.name}");

            HashSet<int> visited = new HashSet<int>();
            List<Tree> route = new List<Tree>() { monkey.tree };

            while(true)
            {
                visited.Add(monkey.tree.treeID);
                double minDistance = double.MaxValue;
                Tree closestTree = null;

                foreach(Tree t in trees.Values)
                {
                    if(!visited.Contains(t.treeID) && !t.hasMonkey)
                    {
                        double distance = Distance(monkey.tree, t);
                        if(distance < minDistance)
                        {
                            minDistance = distance;
                            closestTree = t;
                        }
                    }
                }

                double distanceToBorder = DistanceToBorder(monkey.tree);
                if(closestTree == null || distanceToBorder < minDistance)
                {
                    await writeRouteToDBAsync(monkey, route);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{woodID}:end {woodID},{monkey.name}");
                    return route;
                }

                route.Add(closestTree);
                monkey.tree = closestTree;
            }
        }

        private double Distance(Tree a, Tree b)
        {
            return Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
        }

        private double DistanceToBorder(Tree tree)
        {
            return Math.Min(Math.Min(tree.x - map.xmin, map.xmax - tree.x),
                            Math.Min(tree.y - map.ymin, map.ymax - tree.y));
        }
    }
}
