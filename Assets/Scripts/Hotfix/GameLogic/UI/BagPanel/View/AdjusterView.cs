using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QFramework.UI
{
    /// <summary>
    /// 数量调节器组件
    /// 用于选择使用物品的数量
    /// </summary>
    public class AdjusterView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider slider;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private TextMeshProUGUI countText;

        private int minValue = 1;
        private int maxValue = 1;
        private int currentValue = 1;

        /// <summary>
        /// 当前值
        /// </summary>
        public int CurrentValue
        {
            get => currentValue;
            private set
            {
                currentValue = Mathf.Clamp(value, minValue, maxValue);
                UpdateUI();
                OnValueChanged?.Invoke(currentValue);
            }
        }

        /// <summary>
        /// 值变化回调
        /// </summary>
        public System.Action<int> OnValueChanged;

        private void Awake()
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            if (minusButton != null)
            {
                minusButton.onClick.AddListener(OnMinusButtonClicked);
            }

            if (plusButton != null)
            {
                plusButton.onClick.AddListener(OnPlusButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }

            if (minusButton != null)
            {
                minusButton.onClick.RemoveListener(OnMinusButtonClicked);
            }

            if (plusButton != null)
            {
                plusButton.onClick.RemoveListener(OnPlusButtonClicked);
            }
        }

        /// <summary>
        /// 设置数值范围
        /// </summary>
        public void SetRange(int min, int max)
        {
            minValue = Mathf.Max(1, min);
            maxValue = Mathf.Max(minValue, max);
            
            if (slider != null)
            {
                slider.minValue = minValue;
                slider.maxValue = maxValue;
            }

            CurrentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        }

        /// <summary>
        /// 设置当前值
        /// </summary>
        public void SetValue(int value)
        {
            CurrentValue = value;
        }

        private void OnSliderValueChanged(float value)
        {
            int intValue = Mathf.RoundToInt(value);
            if (intValue != currentValue)
            {
                currentValue = intValue;
                UpdateUI();
                OnValueChanged?.Invoke(currentValue);
            }
        }

        private void OnMinusButtonClicked()
        {
            CurrentValue = currentValue - 1;
        }

        private void OnPlusButtonClicked()
        {
            CurrentValue = currentValue + 1;
        }

        private void UpdateUI()
        {
            // 更新 Slider
            if (slider != null)
            {
                slider.value = currentValue;
            }

            // 更新文本
            if (countText != null)
            {
                countText.text = currentValue.ToString();
            }

            // 更新按钮状态
            if (minusButton != null)
            {
                minusButton.interactable = currentValue > minValue;
            }

            if (plusButton != null)
            {
                plusButton.interactable = currentValue < maxValue;
            }
        }
    }
}

