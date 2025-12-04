using UnityEngine;
using TMPro;

namespace QFramework.UI
{
    /// <summary>
    /// PageEmpty 的视图组件
    /// 当物品列表为空时显示的页面
    /// </summary>
    public class PageEmptyView : BagPageViewBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private GameObject emptyIcon;
        
        // 可以根据实际需要添加更多节点引用

        public override void RefreshData(BagItemData itemData)
        {
            // 空状态页面不需要物品数据
            // 可以显示一些提示信息
            if (emptyText != null)
            {
                emptyText.text = "暂无物品";
            }

            if (emptyIcon != null)
            {
                emptyIcon.SetActive(true);
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            // 显示空状态提示
            RefreshData(null);
        }
    }
}

