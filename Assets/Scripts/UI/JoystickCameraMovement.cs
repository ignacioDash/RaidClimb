using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace UI
{
    public class JoystickCameraMovement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image joystickImage; // todo: to change with the part of the joystick that moves

        [Header("Settings")] [SerializeField] private float sensitivity = 1f;
        [SerializeField] private float minX, minZ;
        [SerializeField] private float maxX, maxZ;
        
        public int? ActiveFingerId { get; private set; }

        private Camera _mainCam;
        private Tween _cameraYTween;

        private bool _movementEnabled;
        private Vector2 _pointerDownPosition;
        private float _targetY;
        private int? _activeFingerId;

        private const float CameraYAnimationDuration = 0.3f;
        private const float JoystickDeadZone = 15f;
        private const float MaxJoystickDistance = 100f;
        private const float Step1X = 35f, Step2X = 45f;
        private const float YBasePosition = 30f, YStep1Position = 38f;

        private void OnEnable()
        {
            _mainCam = Camera.main;
            if (_mainCam != null)
                _targetY = _mainCam.transform.position.y;
        }

        private void OnDisable()
        {
            _cameraYTween?.Kill();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ActiveFingerId = eventData.pointerId;
            _pointerDownPosition = eventData.position;
            _movementEnabled = true;

#if !UNITY_EDITOR
            ActiveFingerId = null;

            foreach (var touch in Touch.activeTouches.Where(touch => touch.isInProgress).Where(touch =>
                         !(Vector2.Distance(touch.screenPosition, eventData.position) > 50f)))
            {
                ActiveFingerId = touch.touchId;
                break;
            }
#endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (ActiveFingerId != eventData.pointerId)
                return;

            // joystickImage.rectTransform.anchoredPosition = Vector2.zero;
            ActiveFingerId = null;
            _movementEnabled = false;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!_movementEnabled || Pointer.current == null || !Pointer.current.press.isPressed)
                return;

            var pointerPos = Pointer.current.position.ReadValue();
#else
            if (!_movementEnabled || ActiveFingerId == null)
                return;

            Touch? touch = null;

            foreach (var t in Touch.activeTouches.Where(t => t.touchId == ActiveFingerId.Value))
            {
                touch = t;
                break;
            }

            if (touch is not { isInProgress: true })
                return;

            var pointerPos = touch.Value.screenPosition;
#endif

            var offset = pointerPos - _pointerDownPosition;
            var magnitude = offset.magnitude;

            var clampedOffset = magnitude > MaxJoystickDistance
                ? offset.normalized * MaxJoystickDistance
                : offset;

            if (magnitude < JoystickDeadZone)
                return;

            var strength = Mathf.Clamp01(magnitude / MaxJoystickDistance);
            var speed = strength * sensitivity;

            var dir = clampedOffset.sqrMagnitude > 0.0001f ? clampedOffset.normalized : Vector2.zero;
            MoveCamera(dir.x * speed, dir.y * speed);
        }

        private void MoveCamera(float inputX, float inputY)
        {
            var pos = _mainCam.transform.position;

            pos.x += inputX;
            pos.z += inputY;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

            _mainCam.transform.position = pos;

            UpdateTargetYForX(pos.x);
        }

        private void UpdateTargetYForX(float x)
        {
            var absX = Mathf.Abs(x);
            var desiredY = absX < Step2X ? YBasePosition : YStep1Position;

            if (Mathf.Approximately(_targetY, desiredY))
                return;

            _targetY = desiredY;

            _cameraYTween?.Kill();
            _cameraYTween = _mainCam.transform
                .DOMoveY(_targetY, CameraYAnimationDuration)
                .SetEase(Ease.OutSine);
        }
    }
}