using System;
using System.Linq;
using System.Threading.Tasks;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

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
        [SerializeField] private JoystickCameraMovement joystickCameraMovement;

        [SerializeField] private float holdDelay = 0.5f;
        
        private int? _activeHoldFingerId;
        
        private InputState _state;
        private float _pressTime;
        private Camera _cam;
        private Vector3 _lastValidDropPosition;

        private GameObject _holdPreviewInstance;
        private GameStateManager _gameStateManager;
        
        public Action OnHold;
        public Action<Vector3> OnRelease;
        
        public bool Inited { get; private set; }
        
        public async Task Init(object[] args)
        {
            _cam = Camera.main;
            
            _gameStateManager = GameManager.Instance.GetManager<GameStateManager>();

            Inited = true;
        }

        public void Cleanup()
        {
            
        }

        private void Update()
        {
            if (!Inited)
                return;

            if (_gameStateManager?.CurrentState != GameStateManager.GameState.InGame)
                return;

            Touch? activeTouch = null;

            if (_activeHoldFingerId.HasValue)
            {
                foreach (var touch in Touch.activeTouches)
                {
                    if (touch.touchId == _activeHoldFingerId.Value)
                    {
                        activeTouch = touch;
                        break;
                    }
                }

                if (!activeTouch.HasValue)
                {
                    if (_state == InputState.Holding)
                        EndHold();

                    _activeHoldFingerId = null;
                    _state = InputState.None;
                    return;
                }
            }
            else
            {
                foreach (var touch in Touch.activeTouches)
                {
                    if (!touch.isInProgress)
                        continue;

                    if (joystickCameraMovement.ActiveFingerId == touch.touchId)
                        continue;

                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId))
                        continue;

                    if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
                        continue;

                    activeTouch = touch;
                    _activeHoldFingerId = touch.touchId;
                    break;
                }

                if (!activeTouch.HasValue)
                    return;
            }
            
#if UNITY_EDITOR
            var pointerPos = Pointer.current.position.ReadValue();
#else
            var pointerPos = activeTouch.Value.screenPosition;
#endif

            var ray = _cam.ScreenPointToRay(pointerPos);

            if (!Physics.Raycast(ray, out var hit, float.MaxValue))
                return;

            switch (_state)
            {
                case InputState.None:
                {
                    if (!pathsCollider.Contains(hit.collider))
                    {
                        _activeHoldFingerId = null;
                        return;
                    }

                    _pressTime = Time.time;
                    _state = InputState.Pressed;
                    break;
                }

                case InputState.Pressed:
                {
                    if (Time.time - _pressTime >= holdDelay)
                        _state = InputState.Holding;
                    break;
                }

                case InputState.Holding:
                {
                    UpdateValidGroundPosition(hit);
                    OnHolding();
                    break;
                }
            }
        }

        private void UpdateValidGroundPosition(RaycastHit hit)
        {
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