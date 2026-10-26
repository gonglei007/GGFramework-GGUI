namespace GGFramework.GGUI
{
    /// <summary>
    /// 引擎无关的补间动画抽象。
    /// NGUI 实现对应 UIPlayTween/UITweener，UGUI 实现可对接 Animator 或 DOTween。
    /// </summary>
    public interface IUITweener
    {
        /// <summary>
        /// 播放补间。
        /// </summary>
        /// <param name="forward">true 正向播放（显示），false 反向播放（隐藏）。</param>
        void Play(bool forward);
    }
}
