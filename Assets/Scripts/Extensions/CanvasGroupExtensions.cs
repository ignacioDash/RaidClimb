using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Extensions
{
    public static class CanvasGroupExtensions
    {
        public static Task FadeCanvasGroup(this CanvasGroup canvasGroup, float target, float duration)
        {
            return canvasGroup.DOFade(target, duration).Play().SetLink(canvasGroup.gameObject)
                .AsyncWaitForCompletion();
        }
    }
}