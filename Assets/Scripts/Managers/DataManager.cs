using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Data;
using Newtonsoft.Json;
using Units.Traps;
using Units.UnitTypes;
using UnityEngine;

namespace Managers
{
    public class DataManager : IManager
    {
        public PlayerData PlayerData { get; private set; }
        public bool Initialized { get; private set; }

        private const string SAVE_DATA_FILE = "SaveData.txt";
        
        public async Task Init(object[] args)
        {
            var initCastleData = new CastleData
            {
                CastleLevel = 1,
                CastleSlots = new List<CastleSlot>
                {
                    new() { SlotId = CastleSlotId.King, SlotUnit = BaseUnit.UnitTypes.King },
                    new() { SlotId = CastleSlotId.Stage2Floor1, SlotTrap = BaseTrap.TrapTypes.Spikes},
                    new() { SlotId = CastleSlotId.Stage2Floor2, SlotTrap = BaseTrap.TrapTypes.Spikes},
                    new() { SlotId = CastleSlotId.Stage1Turret1, SlotUnit = BaseUnit.UnitTypes.Defender },
                    new() { SlotId = CastleSlotId.Stage1Turret2, SlotUnit = BaseUnit.UnitTypes.Defender },
                    new() { SlotId = CastleSlotId.Stage2Turret1, SlotUnit = BaseUnit.UnitTypes.Defender },
                    new() { SlotId = CastleSlotId.Stage2Turret2, SlotUnit = BaseUnit.UnitTypes.Defender },
                    new() { SlotId = CastleSlotId.Stage2Wall1, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                    new() { SlotId = CastleSlotId.Stage2Wall2, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                    new() { SlotId = CastleSlotId.Stage2Wall3, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                    new() { SlotId = CastleSlotId.Stage2Wall4, SlotTrap = BaseTrap.TrapTypes.ThornHedge },
                }
            };
            
            PlayerData = new PlayerData
            {
                UserData = new UserData(),
                PlayerCastleData = initCastleData,
                TrophiesData = new TrophiesData()
            };
            
            var path = Path.Combine(Application.persistentDataPath, SAVE_DATA_FILE);
            
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);

                try
                {
                    PlayerData = JsonConvert.DeserializeObject<PlayerData>(json);
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