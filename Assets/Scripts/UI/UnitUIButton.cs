using System;
using Config;
using TMPro;
using Units.UnitTypes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UnitUIButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI unitCostText;
        [SerializeField] private UnitBaseConfig unitConfig;
        [SerializeField] private BaseUnit.UnitTypes unitType;
        [SerializeField] private int slot;
        [SerializeField] private GameObject lockedIcon;
        [SerializeField] private TextMeshProUGUI arenaUnlockText;
        [SerializeField] private GameObject equippedIndicator;

        public BaseUnit.UnitTypes UnitType => unitType;
        public int Slot => slot;
        public UnitBaseConfig Config => unitConfig;

        private Action<BaseUnit.UnitTypes> _onSelected;

        public void Init(int currentArena, bool isEquipped, Action<BaseUnit.UnitTypes> onSelected)
        {
            if (unitNameText) unitNameText.text = unitConfig ? unitConfig.UnitName : string.Empty;
            if (unitCostText) unitCostText.text = unitConfig ? unitConfig.SquadCost.ToString() : string.Empty;

            var isLocked = unitConfig && currentArena < unitConfig.ArenaUnlock;
            if (lockedIcon) lockedIcon.SetActive(isLocked);
            if (arenaUnlockText) arenaUnlockText.text = unitConfig ? $"Arena {unitConfig.ArenaUnlock}" : string.Empty;

            _onSelected = onSelected;
            SetEquipped(isEquipped);

            button.interactable = !isLocked;
            button.onClick.RemoveAllListeners();
            if (!isLocked)
                button.onClick.AddListener(() => _onSelected?.Invoke(unitType));
        }

        public void SetEquipped(bool isEquipped)
        {
            if (equippedIndicator) equippedIndicator.SetActive(isEquipped);
        }

        public void InitSlot(UnitBaseConfig config, bool hasUnit)
        {
            if (lockedIcon) lockedIcon.SetActive(false);
            if (equippedIndicator) equippedIndicator.SetActive(false);
            if (unitNameText) unitNameText.text = hasUnit && config ? config.UnitName : string.Empty;
            if (unitCostText) unitCostText.text = hasUnit && config ? config.SquadCost.ToString() : string.Empty;
            button.interactable = false;
        }
    }
}
