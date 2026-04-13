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
        [SerializeField] private Camera playerCamera;

        [SerializeField] private Transform defaultCameraPosition,
            playCameraPosition,
            winCameraPosition,
            loseCameraPosition,
            editCastleCameraPosition,
            opponentCastlePosition;

        public Camera MainCamera => mainCamera;
        public Camera PlayerCamera => playerCamera;

        private CameraPosition _currentCameraPosition = CameraPosition.None;
        private Tween _cameraPositionTween, _cameraRotationTween;
        private Tween _mainCameraTween, _playerCameraTween;

        private static readonly Rect _mainCameraFullRect = new (0f, 0f, 1f, 1f);
        private static readonly Rect _mainCameraCastleRect = new (0.5f, 0f, 0.5f, 1f);
        private static readonly Rect _playerCameraHiddenRect = new (0f, 0f, 0.01f, 1f);
        private static readonly Rect _playerCameraCastleRect = new (0f, 0f, 0.5f, 1f);
        
        private const float CAMERA_ANIMATION_DURATION = 0.4f;
        private const float SPLIT_SCREEN_TWEEN_DURATION = 0.4f;
        
        public async Task Init(object[] args)
        {
            mainCamera.rect = _mainCameraFullRect;
            playerCamera.rect = _playerCameraHiddenRect;
            playerCamera.enabled = false;
        }

        public async Task SetCameraAt(CameraPosition cameraPosition)
        {
            if (_currentCameraPosition == cameraPosition)
                return;
            
            _currentCameraPosition = cameraPosition;
            var cameraAnimation = Task.CompletedTask;
            var splitScreenAnimation = Task.CompletedTask;
            switch (_currentCameraPosition)
            {
                case CameraPosition.Default:
                    splitScreenAnimation = HideSplitScreen();
                    cameraAnimation = AnimateCameraTowardsPosition(defaultCameraPosition);
                    break;
                case CameraPosition.Play:
                    splitScreenAnimation = ShowSplitScreen();
                    cameraAnimation = AnimateCameraTowardsPosition(playCameraPosition);
                    break;
                case CameraPosition.Castle:
                    cameraAnimation = AnimateCameraTowardsPosition(editCastleCameraPosition);
                    break;
                case CameraPosition.Win:
                    splitScreenAnimation = HideSplitScreen();
                    cameraAnimation = AnimateCameraTowardsPosition(winCameraPosition);
                    break;
                case CameraPosition.Lose:
                    splitScreenAnimation = HideSplitScreen();
                    cameraAnimation = AnimateCameraTowardsPosition(loseCameraPosition);
                    break;
                case CameraPosition.Opponent:
                    splitScreenAnimation = ShowSplitScreen();
                    cameraAnimation = AnimateCameraTowardsPosition(opponentCastlePosition);
                    break;
            }

            await Task.WhenAll(splitScreenAnimation, cameraAnimation);
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

        private async Task ShowSplitScreen()
        {
            _mainCameraTween?.Kill();
            _playerCameraTween?.Kill();

            playerCamera.enabled = true;
            playerCamera.rect = _playerCameraHiddenRect;

            var mainTask = AnimateCameraRect(mainCamera, _mainCameraCastleRect);
            var playerTask = AnimateCameraRect(playerCamera, _playerCameraCastleRect);

            await Task.WhenAll(mainTask, playerTask);
        }

        public async Task HideSplitScreen()
        {
            _mainCameraTween?.Kill();
            _playerCameraTween?.Kill();

            var mainTask = AnimateCameraRect(mainCamera, _mainCameraFullRect);
            var playerTask = AnimateCameraRect(playerCamera, _playerCameraHiddenRect);

            await Task.WhenAll(mainTask, playerTask);

            playerCamera.enabled = false;
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