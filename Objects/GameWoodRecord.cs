using System.Collections.Generic;

namespace EscapeFromTheWoods.Objects
{
    public class GameWoodRecord
    {
        public List<DBWoodRecord> Records;

        public GameWoodRecord(List<DBWoodRecord> records)
        {
            Records = records;
        }
    }
}
