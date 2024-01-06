using System.Threading;

namespace EscapeFromTheWoods
{
    public static class IDgenerator
    {
        private static int treeID = 0;
        private static int woodID = 0;
        private static int monkeyID = 0;

        public static int GetTreeID()
        {
            return Interlocked.Increment(ref treeID);
        }

        public static int GetMonkeyID()
        {
            return Interlocked.Increment(ref monkeyID);
        }

        public static int GetWoodID()
        {
            return Interlocked.Increment(ref woodID);
        }
    }
}
