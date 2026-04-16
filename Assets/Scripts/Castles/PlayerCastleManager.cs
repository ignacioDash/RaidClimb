using System.Linq;
using System.Threading.Tasks;
using Constants;
using Data;
using Managers;

namespace Castles
{
    public class PlayerCastleManager : CastleManager
    {
        protected override string _playerId { get; set; }
        protected override CastleData _castleData { get; set; }

        private DataManager _dataManager;
        private CurrencyManager _currencyManager;

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

        public void OnCastleScreenOpened()
        {
            UpdateCastleWithCastleData();

            foreach (var slot in castleSlots.Where(slot => slot.SlotPurchase.purchasable))
            {
                // todo: this needs to be changed if we want upgrades
                var isPurchased = _dataManager.PlayerData.PlayerCastleData.CastleSlots.Any(s => s.SlotId == slot.SlotId);
                
                slot.SlotPurchase.purchaseButton.gameObject.SetActive(!isPurchased);
                slot.SlotPurchase.prizeText.text = slot.SlotPurchase.prize.ToString();
                slot.SlotPurchase.purchaseButton.onClick.AddListener(() => OnPurchaseButton(slot));
            }
        }

        public void OnCastleScreenClosed()
        {
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
            
            _ = _dataManager.Save();
        }
    }
}