using System;
using System.Collections;
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
            Equip,
            Castle,
            CastleBuyTarget,
            CastleQuit,
            UnitButtons
        }

        [SerializeField] private GameObject textContainer;
        [SerializeField] private TextMeshProUGUI onboardingText;

        [Header("Overlay Panels")]
        [SerializeField] private GameObject page;
        [SerializeField] private Button fullCoverButton;
        [SerializeField] private RectTransform finger;
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

        private const int TotalSteps = 9;

        private Coroutine _fingerCoroutine;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            page.SetActive(false);
            fullCoverButton.onClick.AddListener(OnFullCoverTapped);
            if (finger) finger.gameObject.SetActive(false);
        }

        public void ShowInGameSteps()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 0;
            _maxStep = 0;
            AdvanceToNextPendingStep();
        }

        public void ShowInGameStep1()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 1;
            _maxStep = 1;
            AdvanceToNextPendingStep();
        }

        public void ShowInGameStep2()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 2;
            _maxStep = 2;
            AdvanceToNextPendingStep();
        }

        public void ShowSquadSteps()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 3;
            _maxStep = 4;
            AdvanceToNextPendingStep();
        }

        public void ShowMainMenuSteps()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 5;
            _maxStep = 5;
            AdvanceToNextPendingStep();
        }

        public void ShowTowerSteps()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 6;
            _maxStep = 7;
            AdvanceToNextPendingStep();
        }

        public void ShowMainMenuPlaySteps()
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            _currentStep = 8;
            _maxStep = 8;
            AdvanceToNextPendingStep();
        }

        public void CompleteAndHide()
        {
            if (_dataManager != null)
            {
                _dataManager.PlayerData.OnboardingData.CompleteStep(_currentStep);
                _ = _dataManager.Save();
            }

            HideFinger();
            page.SetActive(false);
        }

        public void Hide()
        {
            HideFinger();
            page.SetActive(false);
        }

        private void ShowFinger(RectTransform target)
        {
            if (!finger) return;

            var cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var center = (corners[0] + corners[2]) * 0.5f;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform,
                RectTransformUtility.WorldToScreenPoint(cam, center),
                cam, out var localCenter);

            finger.anchoredPosition = localCenter;
            finger.localScale = Vector3.one;
            finger.gameObject.SetActive(true);

            if (_fingerCoroutine != null) StopCoroutine(_fingerCoroutine);
            _fingerCoroutine = StartCoroutine(TapAnimation());
        }

        private void HideFinger()
        {
            if (!finger) return;
            if (_fingerCoroutine != null) { StopCoroutine(_fingerCoroutine); _fingerCoroutine = null; }
            finger.gameObject.SetActive(false);
        }

        private IEnumerator TapAnimation()
        {
            while (true)
            {
                yield return ScaleFinger(Vector3.one, Vector3.one * 0.75f, 0.12f);
                yield return ScaleFinger(Vector3.one * 0.75f, Vector3.one, 0.2f);
                yield return new WaitForSeconds(0.55f);
            }
        }

        private IEnumerator ScaleFinger(Vector3 from, Vector3 to, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                finger.localScale = Vector3.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            finger.localScale = to;
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
                    Show(MaskTarget.Unit1, OnboardingTexts.DropUnit);
                    break;
                case 1:
                    Show(MaskTarget.Unit2, OnboardingTexts.SummonArcher);
                    break;
                case 2:
                    Show(MaskTarget.UnitButtons, OnboardingTexts.KeepSummoning, showFinger: false);
                    break;
                case 3:
                    Show(MaskTarget.Raider, OnboardingTexts.SelectRaider);
                    break;
                case 4:
                    Show(MaskTarget.Equip, null);
                    break;
                case 5:
                    Show(MaskTarget.Castle, OnboardingTexts.TapCastleButton);
                    break;
                case 6:
                    Show(MaskTarget.CastleBuyTarget, OnboardingTexts.BuyCastleSlot, showFinger: false);
                    break;
                case 7:
                    Show(MaskTarget.CastleQuit, null);
                    break;
                case 8:
                    Show(MaskTarget.PlayButton, OnboardingTexts.LetsRaid);
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
            SetText(message);
            fullCoverButton.gameObject.SetActive(true);

            panelTop.gameObject.SetActive(true);
            SetPanelFull(panelTop);
            panelBottom.gameObject.SetActive(false);
            panelLeft.gameObject.SetActive(false);
            panelRight.gameObject.SetActive(false);
            HideFinger();
        }

        private void Show(MaskTarget maskTarget, string message, bool tapToDismiss = false, bool showFinger = true)
        {
            var entry = targets.Find(t => t.mask == maskTarget);
            if (entry == null || !entry.target)
            {
                Hide();
                return;
            }

            page.SetActive(true);
            fullCoverButton.gameObject.SetActive(tapToDismiss);
            SetText(message);

            panelTop.gameObject.SetActive(true);
            panelBottom.gameObject.SetActive(true);
            panelLeft.gameObject.SetActive(true);
            panelRight.gameObject.SetActive(true);

            FocusOn(entry.target);

            if (tapToDismiss || !showFinger)
                HideFinger();
            else
                ShowFinger(entry.target);
        }

        private void SetText(string message)
        {
            var hasText = !string.IsNullOrEmpty(message);
            if (textContainer) textContainer.SetActive(hasText);
            if (onboardingText) onboardingText.text = message;
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
