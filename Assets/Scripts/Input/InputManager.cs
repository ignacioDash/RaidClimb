using System;
using System.Linq;
using System.Threading.Tasks;
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

        [SerializeField] private GameObject holdPreviewPrefab;

        [SerializeField] private float sensitivity = 0.02f;
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float dragThreshold = 10f;
        [SerializeField] private float holdDelay = 0.5f;
        
        private InputState _state;
        private float _pressTime, _startX, _lastX;
        private Camera _cam;
        private Vector3 _lastValidDropPosition;

        private GameObject _holdPreviewInstance;
        private Collider[] _pathColliders;
        private GameStateManager _gameStateManager;
        
        public Action OnHold;
        public Action<Vector3> OnRelease;
        
        public async Task Init(object[] args)
        {
            _cam = Camera.main;

            _pathColliders = GameObject
                .FindGameObjectsWithTag("Path")
                .Select(o => o.GetComponent<Collider>())
                .ToArray();

            _gameStateManager = GameManager.Instance.GetManager<GameStateManager>();
        }

        public void Cleanup()
        {
            
        }

        private void Update()
        {
            if (_gameStateManager.CurrentState != GameStateManager.GameState.InGame)
                return;
            
            if (Pointer.current == null) return;

            if (Pointer.current.press.wasPressedThisFrame)
            {
                _startX = Pointer.current.position.ReadValue().x;
                _lastX = _startX;
                _pressTime = Time.time;
                _state = InputState.Pressed;
            }

            if (Pointer.current.press.isPressed)
            {
                var pointerPos = Pointer.current.position.ReadValue();
                var currentX = pointerPos.x;
                var deltaFromStart = Mathf.Abs(currentX - _startX);

                switch (_state)
                {
                    case InputState.Pressed:
                        if (deltaFromStart > dragThreshold)
                        {
                            _state = InputState.Dragging;
                            _lastX = currentX;
                        }
                        else if (Time.time - _pressTime >= holdDelay)
                        {
                            _state = InputState.Holding;
                        }

                        break;

                    case InputState.Dragging:
                    {
                        var deltaX = currentX - _lastX;
                        _lastX = currentX;
                        MoveCamera(deltaX);
                        break;
                    }

                    case InputState.Holding:
                    {
                        var (worldPos, canDrop) = GetGroundPosition(pointerPos);
                        OnHolding(worldPos, canDrop);
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

        private (Vector3 worldPos, bool canDrop) GetGroundPosition(Vector2 screenPos)
        {
            var ray = _cam.ScreenPointToRay(screenPos);

            if (!Physics.Raycast(ray, out var hit))
                return (_lastValidDropPosition, false);

            if (hit.collider.CompareTag("Path"))
            {
                _lastValidDropPosition = hit.point;
                return (hit.point, true);
            }

            var closestPoint = _lastValidDropPosition;
            var closestDist = float.MaxValue;

            foreach (var pathObj in _pathColliders)
            {
                if (!pathObj) continue;

                var point = pathObj.ClosestPoint(hit.point);
                var dist = (point - hit.point).sqrMagnitude;

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPoint = point;
                }
            }

            _lastValidDropPosition = closestPoint;
            return (closestPoint, false);
        }

        private void MoveCamera(float deltaX)
        {
            var pos = transform.position;
            pos.x -= deltaX * sensitivity;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }

        private void OnHolding(Vector3 worldPos, bool canDrop)
        {
            if (!_holdPreviewInstance)
                _holdPreviewInstance = Instantiate(holdPreviewPrefab, worldPos, Quaternion.identity);

            _lastValidDropPosition = canDrop ? worldPos : _lastValidDropPosition;
            _lastValidDropPosition += Vector3.up * 0.1f; // avoid ground clipping
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