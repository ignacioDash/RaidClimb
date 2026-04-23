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
                        CastleSlots = new List<CastleSlot>
                        {
                            new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King },
                        }
                    };
                case 5:
                    return new CastleData
                    {
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
                case 99:
                    return new CastleData
                    {
                        CastleSlots = new List<CastleSlot>
                        {
                            // King
                            new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King },

                            // Stage 3 — tesla coil turrets, thorn walls, floor spikes
                            new() { SlotId = CastleSlotId.Stage3Turret1, SlotUnit = BaseUnit.UnitTypes.TeslaCoil },
                            new() { SlotId = CastleSlotId.Stage3Turret2, SlotUnit = BaseUnit.UnitTypes.TeslaCoil },
                            new() { SlotId = CastleSlotId.Stage3Wall1, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage3Wall2, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage3Wall3, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage3Floor1, SlotTrap = BaseTrap.TrapTypes.Spikes },

                            // Stage 2 — king cobra turrets, thorn walls, floor spikes
                            new() { SlotId = CastleSlotId.Stage2Turret1, SlotUnit = BaseUnit.UnitTypes.KingCobra },
                            new() { SlotId = CastleSlotId.Stage2Turret2, SlotUnit = BaseUnit.UnitTypes.KingCobra },
                            new() { SlotId = CastleSlotId.Stage2Wall1, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage2Wall2, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage2Wall3, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage2Wall4, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage2Floor1, SlotTrap = BaseTrap.TrapTypes.Spikes },
                            new() { SlotId = CastleSlotId.Stage2Floor2, SlotTrap = BaseTrap.TrapTypes.Spikes },

                            // Stage 1 — defender turrets, thorn walls, floor spikes
                            new() { SlotId = CastleSlotId.Stage1Turret1, SlotUnit = BaseUnit.UnitTypes.Defender },
                            new() { SlotId = CastleSlotId.Stage1Turret2, SlotUnit = BaseUnit.UnitTypes.Defender },
                            new() { SlotId = CastleSlotId.Stage1Wall1, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage1Wall2, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage1Wall3, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                            new() { SlotId = CastleSlotId.Stage1Floor1, SlotTrap = BaseTrap.TrapTypes.Spikes },
                            new() { SlotId = CastleSlotId.Stage1Floor2, SlotTrap = BaseTrap.TrapTypes.Spikes },
                            new() { SlotId = CastleSlotId.Stage1Floor3, SlotTrap = BaseTrap.TrapTypes.Spikes },

                            // Extra
                            new() { SlotId = CastleSlotId.KinWall, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                        }
                    };
                default:
                    return new CastleData();
            }
        }
    }
}