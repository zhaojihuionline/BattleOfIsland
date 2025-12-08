using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 随机奖励概率显示项（对应 OneLineProp 预制体）
    /// </summary>
    public class OneLinePropView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image qualityFrameImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI probabilityText;

        private RewardDetai currentRewardDetail;

        /// <summary>
        /// 设置概率项数据
        /// </summary>
        /// <param name="rewardDetail">奖励详情</param>
        /// <param name="totalWeight">权重总和</param>
        public void SetData(RewardDetai rewardDetail, int totalWeight)
        {
            currentRewardDetail = rewardDetail;

            if (rewardDetail == null || rewardDetail.Id_Ref == null)
            {
                ResetView();
                return;
            }

            var itemConfig = rewardDetail.Id_Ref;

            // 显示物品名称（带数量）
            if (nameText != null)
            {
                if (!string.IsNullOrEmpty(itemConfig.Name))
                {
                    nameText.text = $"{itemConfig.Name} X{rewardDetail.Num}";
                }
                else
                {
                    nameText.text = string.Empty;
                }
            }

            if (countText != null)
            {
                countText.text = rewardDetail.Num.ToString();
            }

            if (iconImage != null)
            {
                if (!string.IsNullOrEmpty(itemConfig.ItemIcon0))
                {
                    try
                    {
                        var loader = ResLoader.Allocate();
                        iconImage.sprite = loader.LoadSync<Sprite>(itemConfig.ItemIcon0);
                        iconImage.enabled = iconImage.sprite != null;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"OneLinePropView: 加载图标失败 ItemId={itemConfig.ItemID}, Error={ex.Message}");
                        iconImage.enabled = false;
                    }
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            if (qualityFrameImage != null)
            {
                string qualityPath = BagItemConverter.GetQualitySpritePath(itemConfig.Quality);
                try
                {
                    var loader = ResLoader.Allocate();
                    qualityFrameImage.sprite = loader.LoadSync<Sprite>(qualityPath);
                    qualityFrameImage.enabled = qualityFrameImage.sprite != null;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"OneLinePropView: 加载品质框失败 Quality={itemConfig.Quality}, Error={ex.Message}");
                    qualityFrameImage.enabled = false;
                }
            }

            if (probabilityText != null)
            {
                string probabilityStr = "-";
                if (totalWeight > 0 && rewardDetail.Weight >= 0)
                {
                    float probability = (float)rewardDetail.Weight / totalWeight;
                    probabilityStr = $"{probability * 100f:0.##}%";
                }
                probabilityText.text = probabilityStr;
            }
        }

        private void ResetView()
        {
            if (nameText != null) nameText.text = string.Empty;
            if (countText != null) countText.text = string.Empty;
            if (probabilityText != null) probabilityText.text = "-";
            if (iconImage != null) iconImage.enabled = false;
            if (qualityFrameImage != null) qualityFrameImage.enabled = false;
        }

    }
}

