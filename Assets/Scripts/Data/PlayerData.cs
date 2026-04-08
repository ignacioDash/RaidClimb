using System;
using System.Collections.Generic;
using Units.Traps;
using Units.UnitTypes;

namespace Data
{
    [Serializable]
    public class PlayerData
    {
        public UserData UserData;
        public CastleData PlayerCastleData;
        public TrophiesData TrophiesData;
    }

    [Serializable]
    public class TrophiesData
    {
        // todo
    }

    [Serializable]
    public class UserData
    {
        public int UserLevel;
        public string UserName;
    }

    [Serializable]
    public class CastleData
    {
        public int CastleLevel;
        public List<CastleSlot> CastleSlots;

        public CastleData()
        {
            CastleLevel = 1;
            CastleSlots = new List<CastleSlot>
            {
                new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King },
            };
        }
    }

    [Serializable]
    public class CastleSlot
    {
        public CastleSlotId SlotId;
        public BaseUnit.UnitTypes SlotUnit;
        public BaseTrap.TrapTypes SlotTrap;
    }

    public enum CastleSlotId
    {
        // stage 3
        King,
        Stage3Turret1,
        Stage3Turret2,
        Stage3Wall1,
        Stage3Wall2,
        Stage3Wall3,
        // stage 2
        Stage2Turret1,
        Stage2Turret2,
        Stage2Floor1,
        Stage2Floor2,
        Stage2Wall1,
        Stage2Wall2,
        Stage2Wall3,
        Stage2Wall4,
        // stage 1
        Stage1Turret1,
        Stage1Turret2,
        Stage1Floor1,
        Stage1Floor2,
        Stage1Floor3,
        Stage1Wall1,
        Stage1Wall2,
        Stage1Wall3,
        // base
        BaseFloor1,
        BaseFloor2,
        BaseFloor3,
        // extra
        KinWall,
        Stage3Floor1,
    }
}