using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputManager : MonoBehaviour
    {
        private enum InputState
        {
            None,
            Pressed,
            Dragging,
            Holding
        }

        [SerializeField] private float sensitivity = 0.02f;
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float dragThreshold = 10f;
        [SerializeField] private float holdDelay = 0.5f;
        
        private InputState _state;
        private float _pressTime, _startX, _lastX;
        private Camera _cam;
        private Vector3 _lastValidDropPosition;
        
        public static InputManager Instance { get; private set; }

        public Action<Vector3, bool> OnHold;
        public Action<Vector3> OnRelease;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(Instance);

            Instance = this;
            
            _cam = Camera.main;
        }

        private void Update()
        {
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

            var canDrop = hit.collider.CompareTag("Ground");

            if (canDrop)
            {
                _lastValidDropPosition = hit.point;
                return (hit.point, true);
            }

            return (hit.point, false);
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
            OnHold?.Invoke(worldPos, canDrop);
        }

        private void EndHold()
        {
            OnRelease?.Invoke(_lastValidDropPosition);
        }
    }
}