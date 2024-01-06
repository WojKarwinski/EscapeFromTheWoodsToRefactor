using System;
using System.Collections.Generic;

namespace EscapeFromTheWoods
{
    public static class WoodBuilder
    {
        public static Wood GetWood(int size, Map map, string path, DBwriter db)
        {
            Random r = new Random(100);
            Dictionary<int, Tree> trees = new Dictionary<int, Tree>();
            while(trees.Count < size)
            {
                Tree t = new Tree(IDgenerator.GetTreeID(), r.Next(map.xmin, map.xmax), r.Next(map.ymin, map.ymax));
                trees.TryAdd(t.treeID, t);
            }
            Wood w = new Wood(IDgenerator.GetWoodID(), trees, map, path, db);
            return w;
        }
    }

}
