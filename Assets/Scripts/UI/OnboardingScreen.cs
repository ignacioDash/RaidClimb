using System;
using System.Collections.Generic;
using Constants;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OnboardingScreen : MonoBehaviour
    {
        public enum MaskTarget
        {
            None,
            PlayButton,
            Unit1,
            Unit2,
            Unit3,
            SquadMeter,
            Raider,
            Equip
        }

        [SerializeField] private TextMeshProUGUI onboardingText;

        [Header("Overlay Panels")]
        [SerializeField] private GameObject page;
        [SerializeField] private Button fullCoverButton;
        [SerializeField] private RectTransform panelTop;
        [SerializeField] private RectTransform panelBottom;
        [SerializeField] private RectTransform panelLeft;
        [SerializeField] private RectTransform panelRight;

        [Header("Padding")]
        [SerializeField] private float padding = 8f;

        [SerializeField] private List<OnboardingTargets> targets;

        private Canvas _canvas;
        private int _currentStep;
        private int _maxStep;
        private DataManager _dataManager;

        private const int TotalSteps = 7;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            page.SetActive(false);
            fullCoverButton.onClick.AddListener(OnFullCoverTapped);
        }

        public void ShowFromStart()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 0;
            _maxStep = 1;
            AdvanceToNextPendingStep();
        }

        public void ShowInGameSteps()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 2;
            _maxStep = 3;
            AdvanceToNextPendingStep();
        }

        public void ShowSquadSteps()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 4;
            _maxStep = 5;
            AdvanceToNextPendingStep();
        }

        public void ShowTowerSteps()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 6;
            _maxStep = 6;
            AdvanceToNextPendingStep();
        }

        public void CompleteAndHide()
        {
            if (_dataManager != null)
            {
                _dataManager.PlayerData.OnboardingData.CompleteStep(_currentStep);
                _ = _dataManager.Save();
            }

            page.SetActive(false);
        }

        public void Hide()
        {
            page.SetActive(false);
        }

        private void AdvanceToNextPendingStep()
        {
            while (_currentStep <= _maxStep &&
                   _dataManager.PlayerData.OnboardingData.IsStepCompleted(_currentStep))
            {
                _currentStep++;
            }

            if (_currentStep > _maxStep)
            {
                Hide();
                return;
            }

            ShowCurrentStep();
        }

        private void ShowCurrentStep()
        {
            switch (_currentStep)
            {
                case 0:
                    ShowFullCover(OnboardingTexts.Welcome);
                    break;
                case 1:
                    Show(MaskTarget.PlayButton, OnboardingTexts.PlayButton);
                    break;
                case 2:
                    Show(MaskTarget.Unit2, OnboardingTexts.DropUnit);
                    break;
                case 3:
                    Show(MaskTarget.SquadMeter, OnboardingTexts.SquadMeterInfo, tapToDismiss: true);
                    break;
                case 4:
                    Show(MaskTarget.Raider, OnboardingTexts.SelectRaider);
                    break;
                case 5:
                    Show(MaskTarget.Equip, OnboardingTexts.EquipRaider);
                    break;
                case 6:
                    ShowFullCover(OnboardingTexts.CastleUpgrade);
                    break;
                default:
                    Hide();
                    break;
            }
        }

        // call from InGameScreen after a unit is dropped
        public void TryCompleteStep(int step)
        {
            if (_currentStep != step || !page.activeSelf) return;
            CompleteCurrentStepAndAdvance();
        }

        private void CompleteCurrentStepAndAdvance()
        {
            _dataManager.PlayerData.OnboardingData.CompleteStep(_currentStep);
            _ = _dataManager.Save();
            _currentStep++;
            AdvanceToNextPendingStep();
        }

        private void OnFullCoverTapped()
        {
            CompleteCurrentStepAndAdvance();
        }

        private void ShowFullCover(string message)
        {
            page.SetActive(true);
            onboardingText.text = message;
            fullCoverButton.gameObject.SetActive(true);

            panelTop.gameObject.SetActive(true);
            SetPanelFull(panelTop);
            panelBottom.gameObject.SetActive(false);
            panelLeft.gameObject.SetActive(false);
            panelRight.gameObject.SetActive(false);
        }

        private void Show(MaskTarget maskTarget, string message, bool tapToDismiss = false)
        {
            var entry = targets.Find(t => t.mask == maskTarget);
            if (entry == null || !entry.target)
            {
                Hide();
                return;
            }

            page.SetActive(true);
            fullCoverButton.gameObject.SetActive(tapToDismiss);
            onboardingText.text = message;

            panelTop.gameObject.SetActive(true);
            panelBottom.gameObject.SetActive(true);
            panelLeft.gameObject.SetActive(true);
            panelRight.gameObject.SetActive(true);

            FocusOn(entry.target);
        }

        private static void SetPanelFull(RectTransform panel)
        {
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }

        private void FocusOn(RectTransform target)
        {
            var cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);

            var rt = (RectTransform)transform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, RectTransformUtility.WorldToScreenPoint(cam, corners[0]), cam, out var localBL);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, RectTransformUtility.WorldToScreenPoint(cam, corners[2]), cam, out var localTR);

            localBL -= Vector2.one * padding;
            localTR += Vector2.one * padding;

            var halfW = rt.rect.width * 0.5f;
            var halfH = rt.rect.height * 0.5f;

            panelTop.offsetMin = new Vector2(0, localTR.y + halfH);
            panelTop.offsetMax = new Vector2(0, 0);

            panelBottom.offsetMin = new Vector2(0, 0);
            panelBottom.offsetMax = new Vector2(0, localBL.y - halfH);

            panelLeft.offsetMin = new Vector2(0, localBL.y + halfH);
            panelLeft.offsetMax = new Vector2(localBL.x - halfW, localTR.y - halfH);

            panelRight.offsetMin = new Vector2(localTR.x + halfW, localBL.y + halfH);
            panelRight.offsetMax = new Vector2(0, localTR.y - halfH);
        }

        [Serializable]
        public class OnboardingTargets
        {
            public MaskTarget mask;
            public RectTransform target;
        }
    }
}
