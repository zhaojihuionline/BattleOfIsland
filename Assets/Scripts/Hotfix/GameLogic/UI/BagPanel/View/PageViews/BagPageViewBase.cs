using UnityEngine;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 背包右侧 Page 的基类
    /// 所有具体的 Page View 都应该继承此类
    /// </summary>
    public abstract class BagPageViewBase : MonoBehaviour, IController
    {
        /// <summary>
        /// 显示该 Page
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        /// <summary>
        /// 隐藏该 Page
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        /// <summary>
        /// 刷新 Page 数据
        /// </summary>
        /// <param name="itemData">当前选中的物品数据</param>
        public abstract void RefreshData(BagItemData itemData);

        /// <summary>
        /// Page 显示时的回调
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// Page 隐藏时的回调
        /// </summary>
        protected virtual void OnHide() { }

        /// <summary>
        /// IController 接口实现：获取架构
        /// </summary>
        public IArchitecture GetArchitecture()
        {
            // 通过父级查找 BagPanel（IController）
            var bagPanel = GetComponentInParent<BagPanel>();
            if (bagPanel != null)
            {
                return bagPanel.GetArchitecture();
            }
            return null;
        }
    }
}

