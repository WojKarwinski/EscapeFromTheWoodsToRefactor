using System;
using System.Collections.Generic;

namespace EscapeFromTheWoods
{
    public static class WoodBuilder
    {
        public static Wood GetWood(int size, Map map, string path, DBwriter db)
        {
            Random r = new(100);
            List<Tree> trees = new();

            while(trees.Count < size)
            {
                Tree t = new(IDgenerator.GetTreeID(), r.Next(map.xmin, map.xmax), r.Next(map.ymin, map.ymax));
                trees.Add(t);
            }

            Wood w = new(IDgenerator.GetWoodID(), trees, map, path, db);
            return w;
        }
    }

}
