using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Units.Traps;
using Units.UnitTypes;
using UnityEngine;

namespace Managers
{
    public abstract class CastleManager : MonoBehaviour, IManager
    {
        [SerializeField] protected List<CastleSlotReference> castleSlots;

        protected abstract string _playerId { get; set; }
        protected abstract CastleData _castleData { get; set; }
        
        public virtual async Task Init(object[] args)
        {
            
        }

        public virtual void OnGameStarted() { }

        public virtual void Cleanup()
        {
            
        }

        protected void UpdateCastleWithCastleData()
        {
            if (_castleData == null)
                return;

            var unitManager = GameManager.Instance.GetManager<UnitManager>();
            var trapsManager = GameManager.Instance.GetManager<TrapsManager>();

            foreach (var slot in _castleData.CastleSlots)
            {
                var spawnPosition = castleSlots.FirstOrDefault(s => s.SlotId == slot.SlotId);
                if (spawnPosition == null)
                    continue;
                
                if (slot.SlotUnit != BaseUnit.UnitTypes.None)
                {
                    var unit = unitManager.SpawnUnit(slot.SlotUnit, spawnPosition.SlotPosition.position, _playerId,
                        BaseUnit.UnitState.Defending);

                    if (!unit) continue;

                    unit.ChangeUnitStateTo(BaseUnit.UnitState.Defending);
                }
                else if (slot.SlotTrap != BaseTrap.TrapTypes.None)
                {
                    var trap = trapsManager.SpawnTrap(slot.SlotTrap, spawnPosition.SlotPosition, _playerId);

                    if (!trap) continue;
                    
                }
            }
        }
    }

    [Serializable]
    public class CastleSlotReference
    {
        [SerializeField] private CastleSlotId slotId;
        [SerializeField] private Transform slotPosition;

        public CastleSlotId SlotId => slotId;
        public Transform SlotPosition => slotPosition;
    }
}