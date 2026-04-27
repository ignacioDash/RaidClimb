using System.Collections.Generic;
using Data;
using Units.Traps;
using Units.UnitTypes;
using UnityEngine;

namespace Castles
{
    public static class CastleDataByLevel
    {
        private static readonly List<CastleSlotId> TurretPool = new()
        {
            CastleSlotId.Stage3Turret1, CastleSlotId.Stage3Turret2,
            CastleSlotId.Stage2Turret1, CastleSlotId.Stage2Turret2,
            CastleSlotId.Stage1Turret1, CastleSlotId.Stage1Turret2,
        };

        private static readonly List<CastleSlotId> WallPool = new()
        {
            CastleSlotId.KinWall,
            CastleSlotId.Stage3Wall1, CastleSlotId.Stage3Wall2, CastleSlotId.Stage3Wall3,
            CastleSlotId.Stage2Wall1, CastleSlotId.Stage2Wall2, CastleSlotId.Stage2Wall3, CastleSlotId.Stage2Wall4,
            CastleSlotId.Stage1Wall1, CastleSlotId.Stage1Wall2, CastleSlotId.Stage1Wall3,
        };

        private static readonly List<CastleSlotId> FloorPool = new()
        {
            CastleSlotId.Stage3Floor1,
            CastleSlotId.Stage2Floor1, CastleSlotId.Stage2Floor2,
            CastleSlotId.Stage1Floor1, CastleSlotId.Stage1Floor2, CastleSlotId.Stage1Floor3,
        };

        public static CastleData GetCastleDataForLevel(int level, int trophies)
        {
            return level switch
            {
                1  => Build(trophies,defenders: 1),
                2  => Build(trophies,defenders: 1, spikes: 1),
                3  => Build(trophies,defenders: 2, spikes: 1),
                4  => Build(trophies,defenders: 2, walls: 1, spikes: 1),
                5  => Build(trophies,defenders: 2, walls: 1, spikes: 2),
                6  => Build(trophies,defenders: 3, walls: 1, spikes: 2),
                7  => Build(trophies,defenders: 3, walls: 2, spikes: 2),
                8  => Build(trophies,defenders: 3, walls: 2, spikes: 3),
                9  => Build(trophies,defenders: 3, walls: 2, spikes: 3, lava: 1),
                10 => Build(trophies,defenders: 3, cobras: 1, walls: 2, spikes: 3, lava: 1),
                11 => Build(trophies,defenders: 3, cobras: 1, walls: 3, spikes: 3, lava: 1),
                12 => Build(trophies,defenders: 3, cobras: 1, walls: 3, spikes: 4, lava: 1),
                13 => Build(trophies,defenders: 3, cobras: 1, teslas: 1, walls: 3, spikes: 4, lava: 1),
                14 => Build(trophies,defenders: 3, cobras: 1, teslas: 1, walls: 4, spikes: 4, lava: 1),
                15 => Build(trophies,defenders: 3, cobras: 1, teslas: 1, walls: 5, spikes: 4, lava: 1),
                16 => Build(trophies,defenders: 3, cobras: 2, teslas: 1, walls: 5, spikes: 4, lava: 1),
                17 => Build(trophies,defenders: 3, cobras: 2, teslas: 1, walls: 6, spikes: 4, lava: 1),
                18 => Build(trophies,defenders: 3, cobras: 2, teslas: 1, walls: 7, spikes: 4, lava: 1),
                19 => Build(trophies,defenders: 3, cobras: 2, teslas: 1, walls: 7, spikes: 5, lava: 1),
                20 => Build(trophies,defenders: 3, cobras: 2, teslas: 1, walls: 8, spikes: 5, lava: 1),
                99 => Build(trophies,defenders: 2, cobras: 2, teslas: 2, walls: 11, spikes: 7),
                _  => new CastleData(),
            };
        }

        private static CastleData Build(int trophies,
            int defenders = 0, int cobras = 0, int teslas = 0,
            int walls = 0, int spikes = 0, int lava = 0)
        {
            var slots = new List<CastleSlot>
            {
                new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King }
            };

            var gameCount = trophies / 30;
            var turretOffset = gameCount % TurretPool.Count;
            var wallOffset   = gameCount % WallPool.Count;
            var floorOffset  = gameCount % FloorPool.Count;

            var turretIndex = turretOffset;
            for (var i = 0; i < defenders; i++)
                slots.Add(new() { SlotId = TurretPool[turretIndex++ % TurretPool.Count], SlotUnit = BaseUnit.UnitTypes.Defender });
            for (var i = 0; i < cobras; i++)
                slots.Add(new() { SlotId = TurretPool[turretIndex++ % TurretPool.Count], SlotUnit = BaseUnit.UnitTypes.KingCobra });
            for (var i = 0; i < teslas; i++)
                slots.Add(new() { SlotId = TurretPool[turretIndex++ % TurretPool.Count], SlotUnit = BaseUnit.UnitTypes.TeslaCoil });

            for (var i = 0; i < walls; i++)
                slots.Add(new() { SlotId = WallPool[(wallOffset + i) % WallPool.Count], SlotTrap = BaseTrap.TrapTypes.ThornHedge });

            var floorIndex = floorOffset;
            for (var i = 0; i < spikes; i++)
                slots.Add(new() { SlotId = FloorPool[floorIndex++ % FloorPool.Count], SlotTrap = BaseTrap.TrapTypes.Spikes });
            for (var i = 0; i < lava; i++)
                slots.Add(new() { SlotId = FloorPool[floorIndex++ % FloorPool.Count], SlotTrap = BaseTrap.TrapTypes.Lava });

            return new CastleData { CastleSlots = slots };
        }
    }
}
