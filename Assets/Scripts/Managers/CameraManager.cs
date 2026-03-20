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
            Lose
        }
        
        [SerializeField] private Camera mainCamera;

        [SerializeField]
        private Transform defaultCameraPosition, playCameraPosition, winCameraPosition, loseCameraPosition, editCastleCameraPosition;

        private CameraPosition _currentCameraPosition;
        private Tween _cameraPositionTween, _cameraRotationTween;

        private const float CAMERA_ANIMATION_DURATION = 0.4f;
        
        public async Task Init(object[] args)
        {
            
        }

        public async Task SetCameraAt(CameraPosition cameraPosition)
        {
            if (_currentCameraPosition == cameraPosition)
                return;
            
            _currentCameraPosition = cameraPosition;
            switch (_currentCameraPosition)
            {
                case CameraPosition.Default:
                    await AnimateCameraTowardsPosition(defaultCameraPosition);
                    break;
                case CameraPosition.Play:
                    await AnimateCameraTowardsPosition(playCameraPosition);
                    break;
                case CameraPosition.Castle:
                    await AnimateCameraTowardsPosition(editCastleCameraPosition);
                    break;
                case CameraPosition.Win:
                    await AnimateCameraTowardsPosition(winCameraPosition);
                    break;
                case CameraPosition.Lose:
                    await AnimateCameraTowardsPosition(loseCameraPosition);
                    break;
            }
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

        public void Cleanup()
        {
            
        }
    }
}