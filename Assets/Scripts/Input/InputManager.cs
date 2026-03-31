using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
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

        [SerializeField] private float holdDelay = 0.5f;
        
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
            
            if (Pointer.current == null) return;
            
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            
            var pointerPos = Pointer.current.position.ReadValue();
                
            var ray = _cam.ScreenPointToRay(pointerPos);
            
            if (Pointer.current.press.wasPressedThisFrame)
            {
                if (!Physics.Raycast(ray, out var hit, float.MaxValue))
                    return;
                
                if (!pathsCollider.Contains(hit.collider))
                    return;
                
                _pressTime = Time.time;
                _state = InputState.Pressed;
            }

            if (Pointer.current.press.isPressed)
            {
                if (!Physics.Raycast(ray, out var hit, float.MaxValue))
                    return;
                
                switch (_state)
                {
                    case InputState.Pressed:
                        if (Time.time - _pressTime >= holdDelay)
                            _state = InputState.Holding;
                        break;
                    case InputState.Holding:
                    {
                        UpdateValidGroundPosition(hit);
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