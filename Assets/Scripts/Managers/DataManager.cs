using System;
using System.IO;
using System.Threading.Tasks;
using Data;
using Newtonsoft.Json;
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
            PlayerData = new PlayerData
            {
                UserData = new UserData(),
                PlayerCastleData = new CastleData(),
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