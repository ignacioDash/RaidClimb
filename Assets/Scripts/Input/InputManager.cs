using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputManager : MonoBehaviour, IManager
    {
        private enum InputState
        {
            None,
            Pressed,
            Dragging,
            Holding
        }

        [SerializeField] private Collider[] pathsCollider;
        [SerializeField] private GameObject holdPreviewPrefab;

        [SerializeField] private float sensitivity = 0.02f;
        [SerializeField] private float minX, minZ;
        [SerializeField] private float maxX, maxZ;
        [SerializeField] private float dragThreshold = 10f;
        [SerializeField] private float holdDelay = 0.5f;
        
        private InputState _state;
        private float _pressTime, _startX, _startY, _lastX, _lastY;
        private Camera _cam;
        private Vector3 _lastValidDropPosition;

        private GameObject _holdPreviewInstance;
        private GameStateManager _gameStateManager;
        
        // camera animations
        private Tween _cameraYTween;
        private float _targetY;
        private const float CameraYAnimationDuration = 0.3f;
        private const float Step1X = 40f, Step2X = 50f;
        private const float YBasePosition = 30f, YStep1Position = 38f, YStep2Position = 50f;
        
        public Action OnHold;
        public Action<Vector3> OnRelease;
        
        public bool Inited { get; private set; }
        
        public async Task Init(object[] args)
        {
            _cam = Camera.main;

            _targetY = _cam.transform.position.y;

            _gameStateManager = GameManager.Instance.GetManager<GameStateManager>();

            Inited = true;
        }

        public void Cleanup()
        {
            _cameraYTween?.Kill();
        }

        private void Update()
        {
            if (!Inited)
                return;
            
            if (_gameStateManager?.CurrentState != GameStateManager.GameState.InGame)
                return;
            
            if (Pointer.current == null) return;

            if (Pointer.current.press.wasPressedThisFrame)
            {
                _startX = Pointer.current.position.ReadValue().x;
                _startY = Pointer.current.position.ReadValue().y;
                _lastX = _startX;
                _lastY = _startY;
                _pressTime = Time.time;
                _state = InputState.Pressed;
            }

            if (Pointer.current.press.isPressed)
            {
                var pointerPos = Pointer.current.position.ReadValue();
                var currentX = pointerPos.x;
                var currentY = pointerPos.y;
                var deltaFromStart = Mathf.Abs(currentX - _startX);

                switch (_state)
                {
                    case InputState.Pressed:
                        if (deltaFromStart > dragThreshold)
                        {
                            _state = InputState.Dragging;
                            _lastX = currentX;
                            _lastY = currentY;
                        }
                        else if (Time.time - _pressTime >= holdDelay)
                        {
                            _state = InputState.Holding;
                        }

                        break;

                    case InputState.Dragging:
                    {
                        var deltaX = currentX - _lastX;
                        var deltaY = currentY - _lastY;

                        _lastX = currentX;
                        _lastY = currentY;

                        MoveCamera(deltaX, deltaY);
                        break;
                    }

                    case InputState.Holding:
                    {
                        UpdateValidGroundPosition(pointerPos);
                        OnHolding();
                        break;
                    }
                }
            }

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                if (_state == InputState.Holding)
                    EndHold();

                _state = InputState.None;
            }
        }

        private void UpdateValidGroundPosition(Vector2 screenPos)
        {
            var ray = _cam.ScreenPointToRay(screenPos);

            if (!Physics.Raycast(ray, out var hit, float.MaxValue, ~0, QueryTriggerInteraction.Ignore))
                return;

            var pointInsidePath = GetPointInsidePath(hit.point);
            _lastValidDropPosition = new Vector3(pointInsidePath.x, 1f, pointInsidePath.z);
        }

        private Vector3 GetPointInsidePath(Vector3 point)
        {
            const float insidePadding = 0.5f;

            var closestPoint = Vector3.zero;
            var closestDist = float.MaxValue;
            Collider closestCollider = null;

            foreach (var pathCollider in pathsCollider)
            {
                if (!pathCollider)
                    continue;

                var candidate = pathCollider.ClosestPoint(point);
                var dist = (candidate - point).sqrMagnitude;

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPoint = candidate;
                    closestCollider = pathCollider;
                }
            }

            if (!closestCollider)
                return point; // fallback, shouldn’t really happen

            var center = closestCollider.bounds.center;
            var dirToCenter = center - closestPoint;

            if (dirToCenter.sqrMagnitude > 0.0001f)
                closestPoint += dirToCenter.normalized * insidePadding;

            return closestPoint;
        }

        private void MoveCamera(float deltaX, float deltaY)
        {
            var pos = transform.position;

            pos.x -= deltaX * sensitivity;
            pos.z -= deltaY * sensitivity;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

            transform.position = pos;

            UpdateTargetYForX(pos.x);
        }

        private void UpdateTargetYForX(float x)
        {
            var absX = Mathf.Abs(x);

            //var desiredY = absX < Step1X ? YBasePosition : absX < Step2X ? YStep1Position : YStep2Position;
            var desiredY = absX < Step2X ? YBasePosition : YStep1Position;

            if (Mathf.Approximately(_targetY, desiredY))
                return;

            _targetY = desiredY;

            _cameraYTween?.Kill();

            _cameraYTween = transform
                .DOMoveY(_targetY, CameraYAnimationDuration)
                .SetEase(Ease.OutSine);
        }

        private void OnHolding()
        {
            if (!_holdPreviewInstance)
                _holdPreviewInstance = Instantiate(holdPreviewPrefab, _lastValidDropPosition, Quaternion.identity);

            _holdPreviewInstance.transform.position = _lastValidDropPosition;
            
            OnHold?.Invoke();
        }

        private void EndHold()
        {
            if (_holdPreviewInstance)
                Destroy(_holdPreviewInstance.gameObject);
                
            OnRelease?.Invoke(_lastValidDropPosition);
        }
    }
}