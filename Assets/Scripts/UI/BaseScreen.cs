using System.Threading.Tasks;
using Extensions;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseScreen : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.4f;
        
        private CanvasGroup _canvasGroup;
        
        protected virtual void OnEnable()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }

        public virtual Task OpenScreen(object[] args)
        {
            return _canvasGroup.FadeCanvasGroup(1, fadeDuration);
        }

        public virtual Task CloseScreen()
        {
            return _canvasGroup.FadeCanvasGroup(1, fadeDuration);
        }
    }
}