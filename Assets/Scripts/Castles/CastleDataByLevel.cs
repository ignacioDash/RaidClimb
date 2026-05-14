using System.Collections.Generic;
using Data;
using Units.Traps;
using Units.UnitTypes;
using UnityEngine;

namespace Castles
{
    public static class CastleDataByLevel
    {
        private static readonly List<CastleSlotId> NonWallPool = new()
        {
            CastleSlotId.Stage1Turret1, CastleSlotId.Stage1Turret2,
            CastleSlotId.Stage1Floor1, CastleSlotId.Stage1Floor2, CastleSlotId.Stage1Floor3,
            CastleSlotId.Stage2Turret1, CastleSlotId.Stage2Turret2,
            CastleSlotId.Stage2Floor1, CastleSlotId.Stage2Floor2,
            CastleSlotId.Stage3Turret1, CastleSlotId.Stage3Turret2,
            CastleSlotId.Stage3Floor1,
        };

        private static readonly List<CastleSlotId> WallPool = new()
        {
            CastleSlotId.Stage1Wall1, CastleSlotId.Stage1Wall2, CastleSlotId.Stage1Wall3,
            CastleSlotId.Stage2Wall1, CastleSlotId.Stage2Wall2, CastleSlotId.Stage2Wall3, CastleSlotId.Stage2Wall4,
            CastleSlotId.Stage3Wall1, CastleSlotId.Stage3Wall2, CastleSlotId.Stage3Wall3,
            CastleSlotId.KinWall,
        };

        public static CastleData GetCastleDataForLevel(int level, int trophies)
        {
            return level switch
            {
                1  => Build(level,defenders: 1),
                2  => Build(level,defenders: 1, spikes: 1),
                3  => Build(level,defenders: 2, spikes: 1),
                4  => Build(level,defenders: 2, walls: 1, spikes: 1),
                5  => Build(level,defenders: 2, walls: 1, spikes: 2),
                6  => Build(level,defenders: 3, walls: 1, spikes: 2),
                7  => Build(level,defenders: 3, walls: 2, spikes: 2),
                8  => Build(level,defenders: 3, walls: 2, spikes: 3),
                9  => Build(level,defenders: 3, walls: 2, spikes: 3, lava: 1),
                10 => Build(level,defenders: 3, cobras: 1, walls: 2, spikes: 3, lava: 1),
                11 => Build(level,defenders: 3, cobras: 1, walls: 3, spikes: 3, lava: 1),
                12 => Build(level,defenders: 3, cobras: 1, walls: 3, spikes: 4, lava: 1),
                13 => Build(level,defenders: 3, cobras: 1, teslas: 1, walls: 3, spikes: 4, lava: 1),
                14 => Build(level,defenders: 3, cobras: 1, teslas: 1, walls: 4, spikes: 4, lava: 1),
                15 => Build(level,defenders: 3, cobras: 1, teslas: 1, walls: 5, spikes: 4, lava: 1),
                16 => Build(level,defenders: 3, cobras: 2, teslas: 1, walls: 5, spikes: 4, lava: 1),
                17 => Build(level,defenders: 3, cobras: 2, teslas: 1, walls: 6, spikes: 4, lava: 1),
                18 => Build(level,defenders: 3, cobras: 2, teslas: 1, walls: 7, spikes: 4, lava: 1),
                19 => Build(level,defenders: 3, cobras: 2, teslas: 1, walls: 7, spikes: 5, lava: 1),
                20 => Build(level,defenders: 3, cobras: 2, teslas: 1, walls: 8, spikes: 5, lava: 1),
                99 => Build(level,defenders: 2, cobras: 2, teslas: 2, walls: 11, spikes: 7),
                _  => new CastleData(),
            };
        }

        private static CastleData Build(int level,
            int defenders = 0, int cobras = 0, int teslas = 0,
            int walls = 0, int spikes = 0, int lava = 0)
        {
            var slots = new List<CastleSlot>
            {
                new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King }
            };

            var nonWall   = level == 1 ? ShuffleFirst(NonWallPool, 5) : Shuffle(NonWallPool);
            var wallSlots = level == 1 ? ShuffleFirst(WallPool, 3)   : Shuffle(WallPool);

            var nonWallIndex = 0;
            for (var i = 0; i < defenders; i++)
                slots.Add(new() { SlotId = nonWall[nonWallIndex++], SlotUnit = BaseUnit.UnitTypes.Defender });
            for (var i = 0; i < cobras; i++)
                slots.Add(new() { SlotId = nonWall[nonWallIndex++], SlotUnit = BaseUnit.UnitTypes.KingCobra });
            for (var i = 0; i < teslas; i++)
                slots.Add(new() { SlotId = nonWall[nonWallIndex++], SlotUnit = BaseUnit.UnitTypes.TeslaCoil });

            for (var i = 0; i < walls; i++)
            {
                var trap = Random.value > 0.5f ? BaseTrap.TrapTypes.Saws : BaseTrap.TrapTypes.ThornHedge;
                slots.Add(new() { SlotId = wallSlots[i], SlotTrap = trap });
            }

            for (var i = 0; i < spikes; i++)
                slots.Add(new() { SlotId = nonWall[nonWallIndex++], SlotTrap = BaseTrap.TrapTypes.Spikes });
            for (var i = 0; i < lava; i++)
                slots.Add(new() { SlotId = nonWall[nonWallIndex++], SlotTrap = BaseTrap.TrapTypes.Lava });

            return new CastleData { CastleSlots = slots };
        }

        private static List<T> Shuffle<T>(List<T> source)
        {
            var list = new List<T>(source);
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }

        private static List<T> ShuffleFirst<T>(List<T> source, int count)
        {
            var list = new List<T>(source);
            for (var i = count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }
    }
}
