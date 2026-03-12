using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Units;
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

        public virtual void Cleanup()
        {
            
        }

        protected void UpdateCastleWithCastleData()
        {
            if (_castleData == null)
                return;

            var unitManager = GameManager.Instance.GetManager<UnitManager>();

            foreach (var slot in _castleData.CastleSlots)
            {
                var spawnPosition = castleSlots.FirstOrDefault(s => s.SlotNumber == slot.SlotNumber);
                if (spawnPosition == null)
                    continue;

                var unit = unitManager.SpawnUnit(slot.SlotUnit, spawnPosition.SlotPosition.position, _playerId,
                    BaseUnit.UnitState.Defending);
                if (!unit)
                    continue;
                
                unit.ChangeUnitStateTo(BaseUnit.UnitState.Defending);
            }
        }
    }

    [Serializable]
    public class CastleSlotReference
    {
        [SerializeField] private int slotNumber;
        [SerializeField] private Transform slotPosition;

        public int SlotNumber => slotNumber;
        public Transform SlotPosition => slotPosition;
    }
}