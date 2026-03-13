using System.Collections.Generic;
using Data;
using Units;
using Units.UnitTypes;

namespace Castles
{
    public static class CastleDataByLevel
    {
        public static CastleData GetCastleDataForLevel(int level) // todo: change to data json format
        {
            switch (level)
            {
                case 1:
                    return new CastleData
                    {
                        CastleLevel = 1,
                        CastleSlots = new List<CastleSlot>
                        {
                            new() { SlotNumber = 2, SlotUnit = BaseUnit.UnitTypes.Defender },
                            new() { SlotNumber = 3, SlotUnit = BaseUnit.UnitTypes.Defender },
                            new() { SlotNumber = 5, SlotUnit = BaseUnit.UnitTypes.King },
                        }
                    };
                default:
                    return new CastleData();
            }
        }
    }
}