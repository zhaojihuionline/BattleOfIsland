using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 提示工具类 (Toast Utility)
    /// 提供快捷方法显示各类飘字提示
    /// </summary>
    public static class Tips
    {
        /// <summary>
        /// 显示普通提示
        /// </summary>
        /// <param name="message">提示内容(支持富文本)</param>
        /// <param name="duration">显示时长(秒),<=0 使用默认值</param>
        public static void Show(string message, float duration = 0)
        {
            ShowTips(message, TipsType.Info, duration);
        }

        /// <summary>
        /// 显示成功提示 (绿色)
        /// </summary>
        /// <param name="message">提示内容(支持富文本)</param>
        /// <param name="duration">显示时长(秒),<=0 使用默认值</param>
        public static void ShowSuccess(string message, float duration = 0)
        {
            ShowTips(message, TipsType.Success, duration);
        }

        /// <summary>
        /// 显示错误提示 (红色)
        /// </summary>
        /// <param name="message">提示内容(支持富文本)</param>
        /// <param name="duration">显示时长(秒),<=0 使用默认值</param>
        public static void ShowError(string message, float duration = 0)
        {
            ShowTips(message, TipsType.Error, duration);
        }

        /// <summary>
        /// 显示警告提示 (黄色)
        /// </summary>
        /// <param name="message">提示内容(支持富文本)</param>
        /// <param name="duration">显示时长(秒),<=0 使用默认值</param>
        public static void ShowWarning(string message, float duration = 0)
        {
            ShowTips(message, TipsType.Warning, duration);
        }

        /// <summary>
        /// 清空所有提示
        /// </summary>
        public static void ClearAll()
        {
            var panel = UIKit.GetPanel<TipsPanel>();
            if (panel != null)
            {
                panel.ClearAll();
            }
        }

        private static void ShowTips(string message, TipsType type, float duration)
        {
            // 确保 TipsPanel 已打开，并放置在 PopUI 层级（弹出层）
            var panel = UIKit.GetPanel<TipsPanel>();
            if (panel == null)
            {
                panel = UIKit.OpenPanel<TipsPanel>(UILevel.PopUI);
            }

            var data = new TipsData(message, type, duration);
            panel.EnqueueTips(data);
        }
    }
}
