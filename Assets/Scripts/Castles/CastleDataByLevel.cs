using System.Collections.Generic;
using Data;
using Units.Traps;
using Units.UnitTypes;

namespace Castles
{
    public static class CastleDataByLevel
    {
        public static CastleData GetCastleDataForLevel(int level)
        {
            switch (level)
            {
                case 1:
                    return new CastleData
                    {
                        CastleLevel = 1,
                        CastleSlots = new List<CastleSlot>
                        {
                            new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King },
                            new() { SlotId = CastleSlotId.Stage2Turret1, SlotUnit = BaseUnit.UnitTypes.TeslaCoil },
                        }
                    };
                case 5:
                    return new CastleData
                    {
                        CastleLevel = 1,
                        CastleSlots = new List<CastleSlot>
                        {
                            new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King },
                            new() { SlotId = CastleSlotId.Stage2Floor1, SlotTrap = BaseTrap.TrapTypes.Spikes},
                            new() { SlotId = CastleSlotId.Stage2Floor2, SlotTrap = BaseTrap.TrapTypes.Spikes},
                            new() { SlotId = CastleSlotId.Stage2Turret1, SlotUnit = BaseUnit.UnitTypes.Defender },
                            new() { SlotId = CastleSlotId.Stage2Turret2, SlotUnit = BaseUnit.UnitTypes.Defender },
                            new() { SlotId = CastleSlotId.Stage2Wall1, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage2Wall2, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                        }
                    };
                default:
                    return new CastleData();
            }
        }
    }
}