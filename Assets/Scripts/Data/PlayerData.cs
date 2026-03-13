using System;
using System.Collections.Generic;
using Units;
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
    }

    [Serializable]
    public class CastleSlot
    {
        public int SlotNumber;
        public BaseUnit.UnitTypes SlotUnit;
    }
}