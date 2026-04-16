using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using TMPro;
using Units;
using Units.Traps;
using Units.UnitTypes;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public abstract class CastleManager : MonoBehaviour, IManager
    {
        [SerializeField] protected List<CastleSlotReference> castleSlots;


        protected abstract string _playerId { get; set; }
        protected abstract CastleData _castleData { get; set; }

        private UnitManager _unitManager;
        private TrapsManager _trapsManager;

        public virtual async Task Init(object[] args)
        {
            _unitManager = GameManager.Instance.GetManager<UnitManager>();
            _trapsManager = GameManager.Instance.GetManager<TrapsManager>();
        }

        public virtual void OnGameStarted()
        {
        }

        public virtual void Cleanup()
        {
        }

        public void OnDissolveCompleted(IDissolve dissolvedUnit)
        {
        }

        protected void UpdateCastleWithCastleData()
        {
            if (_castleData == null)
                return;

            foreach (var slot in _castleData.CastleSlots)
            {
                var spawnPosition = castleSlots.FirstOrDefault(s => s.SlotId == slot.SlotId);
                if (spawnPosition == null)
                    continue;

                SpawnSlot(slot, spawnPosition);
            }
        }

        protected void SpawnSlot(CastleSlot slot, CastleSlotReference spawnPosition)
        {
            if (slot.SlotUnit != BaseUnit.UnitTypes.None)
            {
                var unit = _unitManager.SpawnUnit(slot.SlotUnit, spawnPosition.SlotPosition.position, _playerId);
            }
            else if (slot.SlotTrap != BaseTrap.TrapTypes.None)
            {
                var trap = _trapsManager.SpawnTrap(slot.SlotTrap, spawnPosition.SlotPosition, _playerId);
            }
        }

    }


    [Serializable]
    public class CastleSlotReference
    {
        [SerializeField] private CastleSlotId slotId;
        [SerializeField] private Transform slotPosition;
        [SerializeField] private CastleSlotPurchase slotPurchase;

        public CastleSlotId SlotId => slotId;
        public Transform SlotPosition => slotPosition;
        public CastleSlotPurchase SlotPurchase => slotPurchase;
    }

    [Serializable]
    public class CastleSlotPurchase
    {
        public bool purchasable;
        public Button purchaseButton;
        public TextMeshProUGUI prizeText;
        public int prize;
        public BaseUnit.UnitTypes unitType = BaseUnit.UnitTypes.None;
        public BaseTrap.TrapTypes trapType = BaseTrap.TrapTypes.None;
    }
}