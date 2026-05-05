using System;
using System.Collections.Generic;
using System.Linq;
using Units.Traps;
using Units.UnitTypes;

namespace Data
{
    [Serializable]
    public class PlayerData
    {
        public UserData UserData;
        public CastleData PlayerCastleData;
        public SquadData SquadData;
        public OnboardingData OnboardingData = new();
        public NewContentData NewContentData = new();
    }

    [Serializable]
    public class NewContentData
    {
        public List<int> NewUnitTypes = new();
        public List<int> AcknowledgedCastleMilestones = new();

        public bool IsUnitNew(BaseUnit.UnitTypes unitType) => NewUnitTypes.Contains((int)unitType);

        public void AddNewUnit(BaseUnit.UnitTypes unitType)
        {
            var id = (int)unitType;
            if (!NewUnitTypes.Contains(id))
                NewUnitTypes.Add(id);
        }

        public void MarkUnitSeen(BaseUnit.UnitTypes unitType) => NewUnitTypes.Remove((int)unitType);

        public bool IsCastleMilestoneSeen(int arena) => AcknowledgedCastleMilestones.Contains(arena);

        public void AcknowledgeCastleMilestone(int arena)
        {
            if (!AcknowledgedCastleMilestones.Contains(arena))
                AcknowledgedCastleMilestones.Add(arena);
        }
    }

    [Serializable]
    public class OnboardingData
    {
        public List<int> CompletedSteps = new();

        public bool IsStepCompleted(int step) => CompletedSteps.Contains(step);

        public void CompleteStep(int step)
        {
            if (!CompletedSteps.Contains(step))
                CompletedSteps.Add(step);
        }
    }

    [Serializable]
    public class SquadData
    {
        public List<BaseUnit.UnitTypes> UnlockedUnits = new();
        public List<BaseUnit.UnitTypes> EquippedUnits = new();
    }

    [Serializable]
    public class UserData
    {
        public int coins;
        public int trophies;
        public bool isFirstWin = true;
        public int gamesPlayed;
        public int demotionShieldCharges;
    }

    [Serializable]
    public class CastleData
    {
        public int CastleLevel;
        public List<CastleSlot> CastleSlots;

        public void AddSlot(CastleSlot slot)
        {
            if (CastleSlots.Any(s => s.SlotId == slot.SlotId))
            {
                var existingSlot = CastleSlots.First(s => s.SlotId == slot.SlotId);
                CastleSlots.Remove(existingSlot);
            }

            CastleSlots.Add(slot);
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