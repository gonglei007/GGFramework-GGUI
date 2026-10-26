namespace GGFramework.GGUI.NGUI
{
    /// <summary>
    /// 基于 NGUI UIPlayTween 的补间实现。
    /// </summary>
    public class NGUITweener : IUITweener
    {
        private readonly UIPlayTween playTween;

        public NGUITweener(UIPlayTween playTween)
        {
            this.playTween = playTween;
        }

        public void Play(bool forward)
        {
            if (playTween != null)
            {
                playTween.Play(forward);
            }
        }
    }
}
