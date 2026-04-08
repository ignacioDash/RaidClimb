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

        private readonly List<BaseTrap> _playerTraps = new();
        private readonly List<BaseTrap> _opponentTraps = new();
        
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
            
            if (isPlayerUnit) _playerTraps.Add(trapInstance);
            else _opponentTraps.Add(trapInstance);

            return trapInstance;
        }

        public void Cleanup()
        {
            foreach (var trap in _playerTraps)
            {
                trap.CleanUp();
                if (trap)
                    Destroy(trap.gameObject);
            }

            foreach (var trap in _opponentTraps)
            {
                trap.CleanUp();
                if (trap)
                    Destroy(trap.gameObject);
            }

            _playerTraps.Clear();
            _opponentTraps.Clear();
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