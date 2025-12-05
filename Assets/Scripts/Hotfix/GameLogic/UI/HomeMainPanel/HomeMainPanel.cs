using UnityEngine;
using UnityEngine.UI;
using QFramework;
using QAssetBundle;

namespace QFramework.UI
{
	public class HomeMainPanelData : UIPanelData
	{
	}
    /// <summary>
    /// 游戏主界面
    /// </summary>
    public partial class HomeMainPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as HomeMainPanelData ?? new HomeMainPanelData();
            // please add init code here
            ResLoader loader = ResLoader.Allocate();
            btn_goBattle.onClick.AddListener(() =>
			{
                //UIKit.OpenPanel(QAssetBundle.Prefabs_uipanel_ab.BattleInPanel);
                loader.LoadSceneSync(Scenes_ab.Game);
                UIKit.ClosePanel<HomeMainPanel>();
            });
			btn_goBag.onClick.AddListener(() =>
            {
                UIKit.OpenPanel<BagPanel>();
                UIKit.HidePanel<HomeMainPanel>();
            });
        }
		
		protected override void OnOpen(IUIData uiData = null)
		{
        }
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}
	}
}
