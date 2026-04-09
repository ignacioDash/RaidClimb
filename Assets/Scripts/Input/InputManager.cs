using System;
using System.Threading.Tasks;
using Constants;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
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
        [SerializeField] private Collider spawnArea;
        [SerializeField] private JoystickCameraMovement joystickCameraMovement;

        [SerializeField] private float holdDelay = 0.5f;
        [SerializeField] private LayerMask pathLayer;
        
        private int? _activeHoldFingerId;
        
        private InputState _state;
        private float _pressTime;
        private Camera _cam;

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

                    /*if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId))
                        continue;*/

                    if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
                        continue;

                    activeTouch = touch;
                    _activeHoldFingerId = touch.touchId;
                    break;
                }

                if (!activeTouch.HasValue)
                    return;
            }

            DropUnitAtRandom(activeTouch);
            // DropUnitAtPath(activeTouch);
        }
        
        private void DropUnitAtRandom(Touch? activeTouch)
        {
#if UNITY_EDITOR
            var pointerPressed = Pointer.current != null && Pointer.current.press.isPressed;
#else
            var pointerPressed = activeTouch.HasValue && activeTouch.Value.isInProgress;
#endif

            if (!pointerPressed)
                return;

            switch (_state)
            {
                case InputState.None:
                {
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
                    OnHolding();
                    break;
                }
            }
        }
        
        private Vector3 GetRandomPositionInDropArea()
        {
            var bounds = spawnArea.bounds;

            var x = Random.Range(bounds.min.x, bounds.max.x);
            var z = Random.Range(bounds.min.z, bounds.max.z);

            return new Vector3(x, Values.UNIT_SPAWN_Y, z);
        }

#region OLD_INPUT

        private void DropUnitAtPath(Touch? activeTouch)
        {
#if UNITY_EDITOR
            var pointerPos = Pointer.current.position.ReadValue();
#else
            var pointerPos = activeTouch.Value.screenPosition;
#endif

            var ray = _cam.ScreenPointToRay(pointerPos);

            if (!Physics.Raycast(ray, out var hit, float.MaxValue, pathLayer))
            {
                _activeHoldFingerId = null;
                return;
            }

            switch (_state)
            {
                case InputState.None:
                {
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
            //_lastValidDropPosition = new Vector3(pointInsidePath.x, 1f, pointInsidePath.z);
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
        
#endregion

        private void OnHolding()
        {
            /*if (!_holdPreviewInstance)
                _holdPreviewInstance = Instantiate(holdPreviewPrefab, _lastValidDropPosition, Quaternion.identity);

            _holdPreviewInstance.transform.position = _lastValidDropPosition;*/
            
            OnHold?.Invoke();
        }

        private void EndHold()
        {
            /*if (_holdPreviewInstance)
                Destroy(_holdPreviewInstance.gameObject);*/
            
            var randomPos = GetRandomPositionInDropArea();
                
            OnRelease?.Invoke(new Vector3(randomPos.x, 1f, randomPos.z));
        }
    }
}