using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Data;
using Newtonsoft.Json;
using Units.UnitTypes;
using UnityEngine;

namespace Managers
{
    public class DataManager : IManager
    {
        public PlayerData PlayerData { get; private set; }
        public bool Initialized { get; private set; }

        private CurrencyManager _currencyManager;

        private const string SAVE_DATA_FILE = "SaveData.txt";
        
        public async Task Init(object[] args)
        {
            var initCastleData = new CastleData
            {
                CastleLevel = 1,
                CastleSlots = new List<CastleSlot>
                {
                    new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King },
                    new() { SlotId = CastleSlotId.Stage1Turret1, SlotUnit = BaseUnit.UnitTypes.Defender },
                }
            };

            PlayerData = new PlayerData
            {
                UserData = new UserData { coins = 0, trophies = 70},
                PlayerCastleData = initCastleData,
                SquadData = DefaultSquadData(),
            };

            var path = Path.Combine(Application.persistentDataPath, SAVE_DATA_FILE);

            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);

                try
                {
                    PlayerData = JsonConvert.DeserializeObject<PlayerData>(json);
                    MigratePlayerData();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                await Save();
            }

            Initialized = true;
        }

        private static SquadData DefaultSquadData() => new()
        {
            UnlockedUnits = new List<BaseUnit.UnitTypes>
            {
                BaseUnit.UnitTypes.Melee,
                BaseUnit.UnitTypes.Ranged,
                BaseUnit.UnitTypes.Tank,
            },
            EquippedUnits = new List<BaseUnit.UnitTypes>
            {
                BaseUnit.UnitTypes.Melee,
                BaseUnit.UnitTypes.Ranged,
                BaseUnit.UnitTypes.Tank,
            },
        };

        private void MigratePlayerData()
        {
            if (PlayerData.SquadData == null)
                PlayerData.SquadData = DefaultSquadData();
        }

        public async Task Save()
        {
            var path = Path.Combine(Application.persistentDataPath, SAVE_DATA_FILE);

            var jsonData = JsonConvert.SerializeObject(PlayerData);

            await File.WriteAllTextAsync(path, jsonData);
        }

        public void Cleanup()
        {
            
        }
    }
}