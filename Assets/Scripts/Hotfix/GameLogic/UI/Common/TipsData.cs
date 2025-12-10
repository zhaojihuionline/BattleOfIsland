using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 提示类型枚举
    /// </summary>
    public enum TipsType
    {
        Info,       // 普通信息 (灰色)
        Success,    // 成功 (绿色)
        Error,      // 错误 (红色)
        Warning     // 警告 (黄色)
    }

    /// <summary>
    /// 提示数据
    /// </summary>
    public class TipsData : UIPanelData
    {
        /// <summary>
        /// 提示消息内容(支持富文本)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 提示类型
        /// </summary>
        public TipsType TipsType { get; set; }

        /// <summary>
        /// 自定义显示时长(秒),<=0 使用默认值
        /// </summary>
        public float CustomDuration { get; set; }

        public TipsData(string message, TipsType type = TipsType.Info, float customDuration = 0)
        {
            Message = message;
            TipsType = type;
            CustomDuration = customDuration;
        }
    }
}
