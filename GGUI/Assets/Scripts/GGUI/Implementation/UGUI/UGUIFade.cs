using System;
using System.Collections;
using UnityEngine;

namespace GGFramework.GGUI.UGUI
{
    public class UGUIFade : MonoBehaviour
    {
        Coroutine running;
        bool revealing;
        public CanvasGroup Group { get; private set; }

        public void Play(bool forward, float duration)
        {
            var group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            Begin(group, forward ? 0 : group.alpha, forward ? 1 : 0, duration, null, false);
        }

        // Keep newly revealed controls hit-testable on the first frame.
        public void Reveal(float duration, Func<bool> motionEnabled = null)
        {
            if (!Group) Group = gameObject.AddComponent<CanvasGroup>();
            Group.interactable = true;
            Group.blocksRaycasts = true;
            Begin(Group, .01f, 1, duration, motionEnabled, true);
        }

        void Begin(CanvasGroup group, float start, float end, float duration, Func<bool> motionEnabled, bool reveal)
        {
            Stop();
            revealing = reveal;
            if (!isActiveAndEnabled || duration <= 0 || motionEnabled?.Invoke() == false)
            {
                group.alpha = end;
                return;
            }
            running = StartCoroutine(Fade(group, start, end, duration, motionEnabled, reveal));
        }

        IEnumerator Fade(CanvasGroup group, float start, float end, float duration, Func<bool> motionEnabled, bool reveal)
        {
            group.alpha = start;
            float elapsed = 0;
            while (elapsed < duration && motionEnabled?.Invoke() != false)
            {
                if (reveal) yield return null;
                if (!group) break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float blend = reveal ? 1 - (1 - t) * (1 - t) * (1 - t) : t;
                group.alpha = Mathf.Lerp(start, end, blend);
                if (!reveal) yield return null;
            }
            if (group) group.alpha = end;
            running = null;
        }

        void Stop()
        {
            if (running != null) StopCoroutine(running);
            running = null;
        }

        protected virtual void OnDisable()
        {
            Stop();
            if (revealing && Group) Group.alpha = 1;
        }
    }
}
