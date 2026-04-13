using UnityEngine;

namespace Units
{
    public class DissolveController : MonoBehaviour
    {
        [SerializeField] private MeshRenderer transparentRenderer;
        [SerializeField] private MeshRenderer dissolveRenderer;
        
        [Header("Settings")]
        [SerializeField] private float fillDuration = 2f;
        
        private static readonly int CutoffHeightId = Shader.PropertyToID("_CutoffHeight");

        private Material _dissolveMaterial;
        private Material _transparentMaterial;
        private float _dissolveStart;
        private float _dissolveFinish;
        private float _currentCutoff;
        private bool _isFullyFilled;

        private void OnEnable()
        {
            _transparentMaterial = transparentRenderer.material;
            SetSurfaceTransparent(_transparentMaterial);

            _dissolveMaterial = dissolveRenderer.material;

            var bounds = dissolveRenderer.bounds;
            _dissolveStart = bounds.min.y;
            _dissolveFinish = bounds.max.y;

            _currentCutoff = _dissolveStart;
            _isFullyFilled = false;
            ApplyCutoff(_currentCutoff);
        }
        
        private void ResetFill()
        {
            _currentCutoff = _dissolveStart;
            _isFullyFilled = false;
            ApplyCutoff(_currentCutoff);
        }

        public bool Fill()
        {
            var totalDistance = _dissolveFinish - _dissolveStart;
            var speed = fillDuration > 0f ? totalDistance / fillDuration : totalDistance;

            _currentCutoff = Mathf.MoveTowards(
                _currentCutoff,
                _dissolveFinish,
                speed * Time.deltaTime);

            _isFullyFilled = Mathf.Approximately(_currentCutoff, _dissolveFinish);
            ApplyCutoff(_currentCutoff);
            
            if (_isFullyFilled)
                OnCompleted();

            return _isFullyFilled;
        }

        public void Release()
        {
            if (_isFullyFilled)
                return;

            ResetFill();
        }

        private void OnCompleted()
        {
            SetSurfaceOpaque(_transparentMaterial);
        }

        private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void SetSurfaceTransparent(Material mat)
        {
            if (!mat) return;
            mat.SetFloat(SurfaceId, 1f); // Transparent

            mat.SetColor(EmissionColorId, Color.grey);
        }

        private void SetSurfaceOpaque(Material mat)
        {
            if (!mat) return;
            mat.SetFloat(SurfaceId, 0f); // Opaque

            mat.SetColor(EmissionColorId, Color.white);
        }

        private void ApplyCutoff(float value)
        {
            if (!dissolveRenderer)
                return;

            _dissolveMaterial.SetFloat(CutoffHeightId, value);
        }
    }
}