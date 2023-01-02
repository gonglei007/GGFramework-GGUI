using System;
namespace GGFramework.GGUI
{
    public class TextAdaptor
    {
        public Func<string, string> onGetText = null;
        /// <summary>
        /// 获取（本地化）文本。
        /// 如果没有复制本地化文本回调，直接传key。
        /// </summary>
        /// <param name="text"></param>
        public string GetText(string text)
        {
            if (onGetText == null)
            {
                return text;
            }
            return onGetText(text);
        }
    }
}
