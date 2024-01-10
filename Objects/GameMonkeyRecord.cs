using System.Collections.Generic;

namespace EscapeFromTheWoods.Objects
{
    public class GameMonkeyRecord
    {
        public List<DBMonkeyRecord> Records;

        public GameMonkeyRecord(List<DBMonkeyRecord> records)
        {
            Records = records;
        }
    }
}
