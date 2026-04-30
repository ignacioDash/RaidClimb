using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants;
using Data;
using Managers;
using TMPro;
using UnityEngine;

namespace Castles
{
    public class PlayerCastleManager : CastleManager
    {
        [SerializeField] private List<RowLockVisual> rowLockVisuals;

        protected override string _playerId { get; set; }
        protected override CastleData _castleData { get; set; }

        private DataManager _dataManager;
        private CurrencyManager _currencyManager;

        public Action OnSlotPurchased;

        public override async Task Init(object[] args)
        {
            _playerId = Keys.PLAYER_ID;
            
            await base.Init(args);

            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currencyManager = GameManager.Instance.GetManager<CurrencyManager>();

            while (_dataManager is not { Initialized: true })
            {
                await Task.Yield();
            }
            
            _castleData = _dataManager.PlayerData.PlayerCastleData;
            
            foreach (var slot in castleSlots.Where(slot => slot.SlotPurchase.purchasable))
            {
                slot.SlotPurchase.purchaseButton.gameObject.SetActive(false);
                slot.SlotPurchase.purchaseButton.onClick.RemoveAllListeners();
            }
        }

        public override void OnGameStarted()
        {
            UpdateCastleWithCastleData();
        }

        public void RefreshDefenses()
        {
            _currencyManager = GameManager.Instance.GetManager<CurrencyManager>();
            GameManager.Instance.GetManager<UnitManager>().Cleanup();
            GameManager.Instance.GetManager<TrapsManager>().CleanupPlayerTraps();
            _castleData = _dataManager.PlayerData.PlayerCastleData;
            UpdateCastleWithCastleData();
        }

        public void OnCastleScreenOpened()
        {
            var currentArena = _currencyManager.GetArenaForTrophies(
                _dataManager.PlayerData.UserData.trophies);

            foreach (var row in rowLockVisuals)
            {
                var locked = currentArena < row.arenaUnlock;
                row.lockedOverlay.SetActive(locked);
                if (locked)
                    row.lockedLabel.text = $"Arena {row.arenaUnlock}";
            }

            foreach (var slot in castleSlots.Where(slot => slot.SlotPurchase.purchasable))
            {
                var isPurchased = _dataManager.PlayerData.PlayerCastleData.CastleSlots.Any(s => s.SlotId == slot.SlotId);
                var isUnlocked = currentArena >= slot.SlotPurchase.arenaUnlock;

                slot.SlotPurchase.purchaseButton.gameObject.SetActive(!isPurchased && isUnlocked);
                slot.SlotPurchase.prizeText.text = slot.SlotPurchase.prize.ToString();
                if (!isPurchased && isUnlocked)
                    slot.SlotPurchase.purchaseButton.onClick.AddListener(() => OnPurchaseButton(slot));
            }
        }

        public void OnCastleScreenClosed()
        {
            foreach (var row in rowLockVisuals)
                row.lockedOverlay.SetActive(false);

            foreach (var slot in castleSlots.Where(slot => slot.SlotPurchase.purchasable))
            {
                slot.SlotPurchase.purchaseButton.gameObject.SetActive(false);
                slot.SlotPurchase.purchaseButton.onClick.RemoveAllListeners();
            }
        }

        private void OnPurchaseButton(CastleSlotReference slot)
        {
            if (_dataManager.PlayerData.UserData.coins < slot.SlotPurchase.prize)
                return;

            _dataManager.PlayerData.UserData.coins -= slot.SlotPurchase.prize;
            _currencyManager.AddCoins(-slot.SlotPurchase.prize);

            var slotToAdd = new CastleSlot
                { SlotId = slot.SlotId, SlotUnit = slot.SlotPurchase.unitType, SlotTrap = slot.SlotPurchase.trapType };
            
            _dataManager.PlayerData.PlayerCastleData.AddSlot(slotToAdd);
            _castleData = _dataManager.PlayerData.PlayerCastleData;

            slot.SlotPurchase.purchaseButton.gameObject.SetActive(false);

            SpawnSlot(slotToAdd, slot);
            OnSlotPurchased?.Invoke();

            _ = _dataManager.Save();
        }
    }

    [Serializable]
    public class RowLockVisual
    {
        public int arenaUnlock;
        public GameObject lockedOverlay;
        public TextMeshProUGUI lockedLabel;
    }
}