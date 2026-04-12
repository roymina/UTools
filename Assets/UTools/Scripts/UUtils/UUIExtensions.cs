using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UTools
{
    public static class UUIExtensions
    {
        public static Coroutine ToggleAsCanvasGroup(
            this RectTransform rect,
            bool isActive,
            bool useTween = true,
            float tweenDuration = 0.5f,
            Action callback = null,
            bool changeInteractive = true,
            bool ignoreParentGroups = false,
            MonoBehaviour coroutineRunner = null)
        {
            if (rect == null)
            {
                return null;
            }

            CanvasGroup canvasGroup = rect.gameObject.EnsureComponent<CanvasGroup>();
            if (changeInteractive)
            {
                canvasGroup.interactable = isActive;
                canvasGroup.blocksRaycasts = isActive;
            }

            canvasGroup.ignoreParentGroups = ignoreParentGroups;
            float targetAlpha = isActive ? 1f : 0f;

            if (!useTween || tweenDuration <= 0f)
            {
                canvasGroup.alpha = targetAlpha;
                callback?.Invoke();
                return null;
            }

            return UCoroutineRunner.Start(FadeCanvasGroup(canvasGroup, targetAlpha, tweenDuration, callback), coroutineRunner);
        }

        public static Coroutine ToggleAsCanvasGroupAuto(
            this RectTransform rect,
            bool useTween = true,
            float tweenDuration = 0.5f,
            Action callback = null,
            MonoBehaviour coroutineRunner = null)
        {
            if (rect == null)
            {
                return null;
            }

            CanvasGroup canvasGroup = rect.gameObject.EnsureComponent<CanvasGroup>();
            bool isActive = Mathf.Approximately(canvasGroup.alpha, 1f);
            return rect.ToggleAsCanvasGroup(!isActive, useTween, tweenDuration, callback, true, false, coroutineRunner);
        }

        public static void Show(this Transform transform, bool isActive)
        {
            if (transform == null)
            {
                return;
            }

            transform.localScale = isActive ? Vector3.one : Vector3.zero;
        }

        public static void Show(this RectTransform rectTransform, bool isActive)
        {
            ((Transform)rectTransform).Show(isActive);
        }

        public static Coroutine TweenColor(this Image image, Color targetColor, int loopTime = 3, Action callback = null)
        {
            if (image == null)
            {
                return null;
            }

            return UCoroutineRunner.Start(TweenColorCoroutine(image, targetColor, Mathf.Max(0, loopTime), callback), image);
        }

        public static Coroutine MoveOutOfScreen(
            this RectTransform self,
            Placement hideTo = Placement.Top,
            float extraOffset = 0f,
            bool useTween = true,
            float tweenDuration = 0.5f,
            MonoBehaviour coroutineRunner = null)
        {
            if (self == null)
            {
                return null;
            }

            Vector2 targetAnchorPosition = self.anchoredPosition;

            switch (hideTo)
            {
                case Placement.Top:
                    targetAnchorPosition.y = Screen.height / 2f + self.rect.height / 2f + extraOffset;
                    break;
                case Placement.Left:
                    targetAnchorPosition.x = -Screen.width / 2f - self.rect.width / 2f - extraOffset;
                    break;
                case Placement.Bottom:
                    targetAnchorPosition.y = -Screen.height / 2f - self.rect.height / 2f - extraOffset;
                    break;
                case Placement.Right:
                    targetAnchorPosition.x = Screen.width / 2f + self.rect.width / 2f + extraOffset;
                    break;
            }

            if (!useTween || tweenDuration <= 0f)
            {
                self.anchoredPosition = targetAnchorPosition;
                return null;
            }

            return UCoroutineRunner.Start(MoveCoroutine(self, targetAnchorPosition, tweenDuration), coroutineRunner);
        }

        private static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration, Action callback)
        {
            float startAlpha = canvasGroup.alpha;
            float time = 0f;

            while (canvasGroup != null && time < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = targetAlpha;
            }

            callback?.Invoke();
        }

        private static IEnumerator TweenColorCoroutine(Image image, Color targetColor, int loopTime, Action callback)
        {
            Color initialColor = image.color;
            const float duration = 0.5f;

            for (int i = 0; image != null && i < loopTime; i++)
            {
                yield return LerpColor(image, initialColor, targetColor, duration);
                yield return LerpColor(image, targetColor, initialColor, duration);
            }

            if (image != null)
            {
                image.color = initialColor;
            }

            callback?.Invoke();
        }

        private static IEnumerator LerpColor(Image image, Color from, Color to, float duration)
        {
            float time = 0f;
            while (image != null && time < duration)
            {
                image.color = Color.Lerp(from, to, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            if (image != null)
            {
                image.color = to;
            }
        }

        private static IEnumerator MoveCoroutine(RectTransform self, Vector2 targetPosition, float duration)
        {
            Vector2 startPosition = self.anchoredPosition;
            float time = 0f;

            while (self != null && time < duration)
            {
                self.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            if (self != null)
            {
                self.anchoredPosition = targetPosition;
            }
        }
    }
}
