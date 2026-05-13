using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants;
using Data;
using Managers;
using TMPro;
using Units.Traps;
using Units.UnitTypes;
using UnityEngine;
using UnityEngine.UI;

namespace Castles
{
    public class PlayerCastleManager : CastleManager
    {
        [SerializeField] private List<RowLockVisual> rowLockVisuals;

        protected override string _playerId { get; set; }
        protected override CastleData _castleData { get; set; }

        private DataManager _dataManager;
        private CurrencyManager _currencyManager;

        private readonly Dictionary<CastleSlotId, BaseUnit> _slotUnits = new();
        private readonly Dictionary<CastleSlotId, BaseTrap> _slotTraps = new();
        private CastleSlotReference _selectedSwapSlot;

        private static readonly Color SwapIdle     = new Color(0f, 0f, 0f, 0f);
        private static readonly Color SwapSelected = new Color(0f, 0f, 0f, 0.35f);

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

        protected override void UpdateCastleWithCastleData()
        {
            _slotUnits.Clear();
            _slotTraps.Clear();
            if (_castleData == null) return;
            foreach (var slot in _castleData.CastleSlots)
            {
                var slotRef = castleSlots.FirstOrDefault(s => s.SlotId == slot.SlotId);
                if (slotRef != null) SpawnAndTrack(slot, slotRef);
            }
        }

        private void SpawnAndTrack(CastleSlot slot, CastleSlotReference slotRef)
        {
            var (unit, trap) = SpawnSlot(slot, slotRef);
            if (unit != null) _slotUnits[slot.SlotId] = unit;
            if (trap != null) _slotTraps[slot.SlotId] = trap;
        }

        private void DespawnSlot(CastleSlotId slotId)
        {
            if (_slotUnits.TryGetValue(slotId, out var unit) && unit != null)
            {
                GameManager.Instance.GetManager<UnitManager>().RemoveUnit(unit);
                _slotUnits.Remove(slotId);
            }
            if (_slotTraps.TryGetValue(slotId, out var trap) && trap != null)
            {
                GameManager.Instance.GetManager<TrapsManager>().RemovePlayerTrap(trap);
                _slotTraps.Remove(slotId);
            }
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

                if (slot.SlotPurchase.swapButton == null) continue;
                slot.SlotPurchase.swapButton.gameObject.SetActive(isPurchased);
                if (!isPurchased) continue;
                SetSwapHighlight(slot, false);
                slot.SlotPurchase.swapButton.onClick.AddListener(() => OnSwapTapped(slot));
            }
        }

        public void OnCastleScreenClosed()
        {
            _selectedSwapSlot = null;

            foreach (var row in rowLockVisuals)
                row.lockedOverlay.SetActive(false);

            foreach (var slot in castleSlots.Where(slot => slot.SlotPurchase.purchasable))
            {
                slot.SlotPurchase.purchaseButton.gameObject.SetActive(false);
                slot.SlotPurchase.purchaseButton.onClick.RemoveAllListeners();

                if (slot.SlotPurchase.swapButton == null) continue;
                SetSwapHighlight(slot, false);
                slot.SlotPurchase.swapButton.gameObject.SetActive(false);
                slot.SlotPurchase.swapButton.onClick.RemoveAllListeners();
            }
        }

        private void SetSwapHighlight(CastleSlotReference slot, bool selected) =>
            slot.SlotPurchase.swapButton.image.color = selected ? SwapSelected : SwapIdle;

        private void OnSwapTapped(CastleSlotReference tapped)
        {
            if (_selectedSwapSlot == null)
            {
                _selectedSwapSlot = tapped;
                SetSwapHighlight(tapped, true);
                return;
            }

            if (_selectedSwapSlot == tapped)
            {
                SetSwapHighlight(tapped, false);
                _selectedSwapSlot = null;
                return;
            }

            TryExecuteSwap(_selectedSwapSlot, tapped);
            SetSwapHighlight(_selectedSwapSlot, false);
            _selectedSwapSlot = null;
        }

        private void TryExecuteSwap(CastleSlotReference a, CastleSlotReference b)
        {
            if (IsWallSlot(a.SlotId) != IsWallSlot(b.SlotId))
                return;

            var savedSlots = _dataManager.PlayerData.PlayerCastleData.CastleSlots;
            var dataA = savedSlots.FirstOrDefault(s => s.SlotId == a.SlotId);
            var dataB = savedSlots.FirstOrDefault(s => s.SlotId == b.SlotId);

            if (dataA == null || dataB == null)
                return;

            DespawnSlot(a.SlotId);
            DespawnSlot(b.SlotId);

            _dataManager.PlayerData.PlayerCastleData.AddSlot(
                new CastleSlot { SlotId = a.SlotId, SlotUnit = dataB.SlotUnit, SlotTrap = dataB.SlotTrap });
            _dataManager.PlayerData.PlayerCastleData.AddSlot(
                new CastleSlot { SlotId = b.SlotId, SlotUnit = dataA.SlotUnit, SlotTrap = dataA.SlotTrap });

            _castleData = _dataManager.PlayerData.PlayerCastleData;

            SpawnAndTrack(_castleData.CastleSlots.First(s => s.SlotId == a.SlotId), a);
            SpawnAndTrack(_castleData.CastleSlots.First(s => s.SlotId == b.SlotId), b);

            _ = _dataManager.Save();
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

            if (slot.SlotPurchase.swapButton != null)
            {
                slot.SlotPurchase.swapButton.gameObject.SetActive(true);
                SetSwapHighlight(slot, false);
                slot.SlotPurchase.swapButton.onClick.AddListener(() => OnSwapTapped(slot));
            }

            SpawnAndTrack(slotToAdd, slot);
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