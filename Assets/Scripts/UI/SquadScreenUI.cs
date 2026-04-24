using System.Collections.Generic;
using System.Threading.Tasks;
using Managers;
using TMPro;
using Units;
using Units.UnitTypes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SquadScreenUI : BaseScreen
    {
        [SerializeField] private UnitCamerasController unitCamerasController;
        [SerializeField] private Button mainButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private List<UnitUIButton> unitButtons;
        [SerializeField] private UnitUIButton equippedSlot1;
        [SerializeField] private UnitUIButton equippedSlot2;
        [SerializeField] private UnitUIButton equippedSlot3;
        [SerializeField] private RawImage equippedSlotImage1;
        [SerializeField] private RawImage equippedSlotImage2;
        [SerializeField] private RawImage equippedSlotImage3;
        [SerializeField] private TextMeshProUGUI selectedUnitNameText;
        [SerializeField] private TextMeshProUGUI selectedUnitHpText;
        [SerializeField] private TextMeshProUGUI selectedUnitDmgText;
        [SerializeField] private TextMeshProUGUI selectedUnitSpeedText;
        [SerializeField] private TextMeshProUGUI selectedUnitRangeText;

        private BaseUnit.UnitTypes _selectedUnit;
        private bool _hasSelection;

        protected override void OnEnable()
        {
            base.OnEnable();
            mainButton.onClick.AddListener(OnBack);
            equipButton.onClick.AddListener(OnEquip);
        }

        private void OnDisable()
        {
            mainButton.onClick.RemoveListener(OnBack);
            equipButton.onClick.RemoveListener(OnEquip);
        }

        private async void OnBack()
        {
            await GameManager.Instance.GetManager<UIManager>().NavigateTo(UIManager.Screens.MainScreen);
        }

        public override async Task OpenScreen(object[] args)
        {
            await base.OpenScreen(args);

            var dataManager = GameManager.Instance.GetManager<DataManager>();
            var currencyManager = GameManager.Instance.GetManager<CurrencyManager>();
            var trophies = dataManager.PlayerData.UserData.trophies;
            var currentArena = currencyManager.GetArenaForTrophies(trophies);
            var equippedUnits = dataManager.PlayerData.SquadData.EquippedUnits;

            unitCamerasController.ShowAllUnits();

            foreach (var unitButton in unitButtons)
                unitButton.Init(currentArena, equippedUnits.Contains(unitButton.UnitType), OnUnitSelected);

            RefreshSlotButtons(equippedUnits);

            var firstEquipped = equippedUnits.Count > 0 ? equippedUnits[0] : BaseUnit.UnitTypes.None;
            if (firstEquipped != BaseUnit.UnitTypes.None)
                OnUnitSelected(firstEquipped);
            else
                ClearUnitStats();
        }

        private void OnUnitSelected(BaseUnit.UnitTypes unitType)
        {
            _selectedUnit = unitType;
            _hasSelection = true;

            var unitButton = unitButtons.Find(b => b.UnitType == unitType);
            var config = unitButton?.Config;

            if (selectedUnitNameText) selectedUnitNameText.text = config ? config.UnitName : unitType.ToString();
            unitCamerasController.ShowFullBodyUnit(unitType);
            if (selectedUnitHpText) selectedUnitHpText.text = config ? config.Health.ToString() : string.Empty;
            if (selectedUnitDmgText) selectedUnitDmgText.text = config ? config.Damage.ToString() : string.Empty;
            if (selectedUnitSpeedText) selectedUnitSpeedText.text = config ? config.MovementSpeed.ToString() : string.Empty;
            if (selectedUnitRangeText) selectedUnitRangeText.text = config ? config.Range.ToString() : string.Empty;
        }

        private void ClearUnitStats()
        {
            _hasSelection = false;
            if (selectedUnitNameText) selectedUnitNameText.text = string.Empty;
            if (selectedUnitHpText) selectedUnitHpText.text = string.Empty;
            if (selectedUnitDmgText) selectedUnitDmgText.text = string.Empty;
            if (selectedUnitSpeedText) selectedUnitSpeedText.text = string.Empty;
            if (selectedUnitRangeText) selectedUnitRangeText.text = string.Empty;
        }

        private void OnEquip()
        {
            if (!_hasSelection) return;

            var selectedButton = unitButtons.Find(b => b.UnitType == _selectedUnit);
            if (selectedButton == null) return;

            var dataManager = GameManager.Instance.GetManager<DataManager>();
            var equippedUnits = dataManager.PlayerData.SquadData.EquippedUnits;

            while (equippedUnits.Count < 3)
                equippedUnits.Add(BaseUnit.UnitTypes.None);

            if (equippedUnits.Contains(_selectedUnit)) return;

            var slotIndex = selectedButton.Slot - 1;
            equippedUnits[slotIndex] = _selectedUnit;

            foreach (var btn in unitButtons)
                btn.SetEquipped(equippedUnits.Contains(btn.UnitType));

            RefreshSlotButtons(equippedUnits);

            _ = dataManager.Save();
        }

        private void RefreshSlotButtons(List<BaseUnit.UnitTypes> equippedUnits)
        {
            RefreshSlot(equippedSlot1, equippedSlotImage1, equippedUnits, 0);
            RefreshSlot(equippedSlot2, equippedSlotImage2, equippedUnits, 1);
            RefreshSlot(equippedSlot3, equippedSlotImage3, equippedUnits, 2);
        }

        private void RefreshSlot(UnitUIButton slotButton, RawImage slotImage, List<BaseUnit.UnitTypes> equippedUnits, int index)
        {
            var hasUnit = equippedUnits.Count > index && equippedUnits[index] != BaseUnit.UnitTypes.None;
            var unitType = hasUnit ? equippedUnits[index] : BaseUnit.UnitTypes.None;
            var config = hasUnit ? unitButtons.Find(b => b.UnitType == unitType)?.Config : null;

            if (slotButton) slotButton.InitSlot(config, hasUnit);
            if (slotImage) slotImage.texture = hasUnit ? unitCamerasController.GetRenderTexture(unitType) : null;
        }
    }
}
