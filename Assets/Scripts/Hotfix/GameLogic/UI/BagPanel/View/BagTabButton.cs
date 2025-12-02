using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QFramework.UI
{
    /// <summary>
    /// Tab按钮配置脚本，挂载在BagTabButton预制体上
    /// </summary>
    public class BagTabButton : MonoBehaviour
    {
        [Header("Tab配置")]
        [SerializeField] private string tabName;  // Tab名称（用于显示文字）
        [SerializeField] private Sprite normalSprite;  // 未选中状态的背景图
        [SerializeField] private Sprite selectedSprite;  // 选中状态的背景图
        [SerializeField] private Sprite tabIcon;  // Tab图标（可选）
        
        private Toggle toggle;
        private Image backgroundImage;
        private TextMeshProUGUI labelText;  // 使用TMP_Text
        private Image iconImage;
        
        /// <summary>
        /// Tab索引（由BagPanel设置）
        /// </summary>
        public int TabIndex { get; set; }
        
        /// <summary>
        /// Tab名称
        /// </summary>
        public string TabName => tabName;
        
        /// <summary>
        /// Toggle组件
        /// </summary>
        public Toggle Toggle
        {
            get
            {
                if (toggle == null)
                {
                    // Toggle组件在子节点"Toggle"上
                    var toggleObj = transform.Find("Toggle");
                    if (toggleObj != null)
                    {
                        toggle = toggleObj.GetComponent<Toggle>();
                    }
                }
                return toggle;
            }
        }
        
        private void Awake()
        {
            // 获取组件引用
            // Toggle组件在子节点"Toggle"上
            var toggleObj = transform.Find("Toggle");
            if (toggleObj != null)
            {
                toggle = toggleObj.GetComponent<Toggle>();
                // Background在Toggle子节点下
                backgroundImage = toggleObj.Find("Background")?.GetComponent<Image>();
                // Label是"Text (TMP)"，使用TMP_Text
                labelText = toggleObj.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            }
            
            // Icon是可选的，可能在根节点或Toggle节点下
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage == null && toggleObj != null)
            {
                iconImage = toggleObj.Find("Icon")?.GetComponent<Image>();
            }
        }
        
        /// <summary>
        /// 初始化Tab按钮（由BagPanel调用）
        /// </summary>
        public void Initialize(ToggleGroup toggleGroup, System.Action<int> onTabSelected)
        {
            // 确保获取到Toggle组件
            if (toggle == null)
            {
                var toggleObj = transform.Find("Toggle");
                if (toggleObj != null)
                {
                    toggle = toggleObj.GetComponent<Toggle>();
                }
            }
            
            if (toggle == null)
            {
                Debug.LogError("BagTabButton: 找不到Toggle组件！");
                return;
            }
            
            // 确保获取到Background Image
            if (backgroundImage == null && toggle != null)
            {
                var toggleObj = toggle.transform;
                backgroundImage = toggleObj.Find("Background")?.GetComponent<Image>();
            }
            
            // 确保获取到Label Text
            if (labelText == null && toggle != null)
            {
                var toggleObj = toggle.transform;
                labelText = toggleObj.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            }
            
            // 设置Toggle Group
            toggle.group = toggleGroup;
            
            // 设置文字
            if (labelText != null && !string.IsNullOrEmpty(tabName))
            {
                labelText.text = tabName;
            }
            
            // 设置图标
            if (iconImage != null && tabIcon != null)
            {
                iconImage.sprite = tabIcon;
                iconImage.gameObject.SetActive(true);
            }
            
            // 设置初始Sprite（未选中状态）
            if (backgroundImage != null && normalSprite != null)
            {
                backgroundImage.sprite = normalSprite;
            }
            
            // 绑定事件
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((isOn) =>
            {
                UpdateSprite(isOn);
                
                if (isOn && onTabSelected != null)
                {
                    onTabSelected(TabIndex);
                }
            });
        }
        
        /// <summary>
        /// 更新Sprite（根据选中状态）
        /// </summary>
        public void UpdateSprite(bool isSelected)
        {
            if (backgroundImage == null) return;
            
            if (isSelected && selectedSprite != null)
            {
                backgroundImage.sprite = selectedSprite;
            }
            else if (!isSelected && normalSprite != null)
            {
                backgroundImage.sprite = normalSprite;
            }
        }
        
        /// <summary>
        /// 编辑器模式下预览效果
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // 在编辑器中更新显示
                var toggleObj = transform.Find("Toggle");
                if (toggleObj != null)
                {
                    if (toggle == null)
                    {
                        toggle = toggleObj.GetComponent<Toggle>();
                    }
                    if (backgroundImage == null)
                    {
                        backgroundImage = toggleObj.Find("Background")?.GetComponent<Image>();
                    }
                    if (labelText == null)
                    {
                        labelText = toggleObj.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
                    }
                }
                
                if (labelText != null && !string.IsNullOrEmpty(tabName))
                {
                    labelText.text = tabName;
                }
                
                if (backgroundImage != null && toggle != null)
                {
                    UpdateSprite(toggle.isOn);
                }
            }
        }
    }
}

