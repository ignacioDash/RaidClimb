using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Managers
{
    public class CameraManager : MonoBehaviour, IManager
    {
        public enum CameraPosition
        {
            Default,
            Play,
            Castle,
            Win,
            Lose,
            Opponent,
            None
        }
        
        [SerializeField] private Camera mainCamera;

        [SerializeField] private Transform defaultCameraPosition,
            playCameraPosition,
            winCameraPosition,
            loseCameraPosition,
            editCastleCameraPosition,
            opponentCastlePosition;

        public Camera MainCamera => mainCamera;

        private CameraPosition _currentCameraPosition = CameraPosition.None;
        private Tween _cameraPositionTween, _cameraRotationTween;
        private Tween _mainCameraTween, _playerCameraTween;
        
        private const float CAMERA_ANIMATION_DURATION = 0.4f;
        private const float SPLIT_SCREEN_TWEEN_DURATION = 0.4f;
        
        public async Task Init(object[] args)
        {
        }

        public async Task SetCameraAt(CameraPosition cameraPosition)
        {
            if (_currentCameraPosition == cameraPosition)
                return;
            
            _currentCameraPosition = cameraPosition;
            var cameraAnimation = Task.CompletedTask;
            switch (_currentCameraPosition)
            {
                case CameraPosition.Default:
                    cameraAnimation = AnimateCameraTowardsPosition(defaultCameraPosition);
                    break;
                case CameraPosition.Play:
                    cameraAnimation = AnimateCameraTowardsPosition(playCameraPosition);
                    break;
                case CameraPosition.Castle:
                    cameraAnimation = AnimateCameraTowardsPosition(editCastleCameraPosition);
                    break;
                case CameraPosition.Win:
                    cameraAnimation = AnimateCameraTowardsPosition(winCameraPosition);
                    break;
                case CameraPosition.Lose:
                    cameraAnimation = AnimateCameraTowardsPosition(loseCameraPosition);
                    break;
                case CameraPosition.Opponent:
                    cameraAnimation = AnimateCameraTowardsPosition(opponentCastlePosition);
                    break;
            }

            await Task.WhenAll(cameraAnimation);
        }

        private Task AnimateCameraTowardsPosition(Transform targetPosition)
        {
            if (_cameraPositionTween != null)
            {
                _cameraPositionTween.Kill();
                _cameraPositionTween = null;
            }

            if (_cameraRotationTween != null)
            {
                _cameraRotationTween.Kill();
                _cameraRotationTween = null;
            }

            _cameraPositionTween = mainCamera.transform.DOMove(targetPosition.position, CAMERA_ANIMATION_DURATION).Play()
                .SetLink(mainCamera.gameObject);

            _cameraRotationTween = mainCamera.transform
                .DORotate(targetPosition.rotation.eulerAngles, CAMERA_ANIMATION_DURATION).Play()
                .SetLink(mainCamera.gameObject);

            return Task.WhenAll(_cameraPositionTween.AsyncWaitForCompletion(),
                _cameraRotationTween.AsyncWaitForCompletion());
        }
        
        private async Task AnimateCameraRect(Camera targetCamera, Rect targetRect, float duration = SPLIT_SCREEN_TWEEN_DURATION)
        {
            if (!targetCamera)
                return;

            var startRect = targetCamera.rect;
            var t = 0f;

            Tween tween = DOTween.To(
                    () => t,
                    value =>
                    {
                        t = value;
                        targetCamera.rect = LerpRect(startRect, targetRect, t);
                    },
                    1f,
                    duration)
                .Play()
                .SetEase(Ease.OutCubic);

            await tween.AsyncWaitForCompletion();
        }

        private static Rect LerpRect(Rect from, Rect to, float t)
        {
            return new Rect(
                Mathf.Lerp(from.x, to.x, t),
                Mathf.Lerp(from.y, to.y, t),
                Mathf.Lerp(from.width, to.width, t),
                Mathf.Lerp(from.height, to.height, t)
            );
        }

        public void Cleanup()
        {
            
        }
    }
}