using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants;
using Units.Traps;
using UnityEngine;

namespace Managers
{
    public class TrapsManager : MonoBehaviour, IManager
    {
        [SerializeField] private Transform trapsContainer;
        
        [Header("Settings")]
        [SerializeField] private List<TrapsReference> trapReferences;

        public readonly List<BaseTrap> PlayerTraps = new();
        public readonly List<BaseTrap> OpponentTraps = new();
        
        public async Task Init(object[] args)
        {
            
        }

        public BaseTrap SpawnTrap(BaseTrap.TrapTypes trapType, Transform trapSlot, string playerId)
        {
            var baseTrap = trapReferences.FirstOrDefault(t => t.TrapType == trapType);

            if (!baseTrap?.TrapPrefab)
                return null;

            var trapInstance = Instantiate(baseTrap.TrapPrefab, trapSlot.position,
                trapSlot.rotation * Quaternion.Euler(new Vector3(-90, 0, 0)), trapsContainer);

            trapInstance.Init(playerId);

            var isPlayerUnit = playerId == Keys.PLAYER_ID;
            
            if (isPlayerUnit) PlayerTraps.Add(trapInstance);
            else OpponentTraps.Add(trapInstance);

            return trapInstance;
        }

        public void Cleanup()
        {
            foreach (var trap in PlayerTraps)
            {
                trap.CleanUp();
                if (trap)
                    Destroy(trap.gameObject);
            }

            foreach (var trap in OpponentTraps)
            {
                trap.CleanUp();
                if (trap)
                    Destroy(trap.gameObject);
            }

            PlayerTraps.Clear();
            OpponentTraps.Clear();
        }
    }
    
    [Serializable]
    public class TrapsReference
    {
        [SerializeField] private BaseTrap.TrapTypes trapType;
        [SerializeField] private BaseTrap trapPrefab;

        public BaseTrap.TrapTypes TrapType => trapType;
        public BaseTrap TrapPrefab => trapPrefab;
    }
}