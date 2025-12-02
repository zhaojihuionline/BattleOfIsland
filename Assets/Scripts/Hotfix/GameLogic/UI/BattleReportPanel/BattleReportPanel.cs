using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
	public class BattleReportPanelData : UIPanelData
	{
	}
	public partial class BattleReportPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as BattleReportPanelData ?? new BattleReportPanelData();
			btn_win_restart.onClick.AddListener(() =>
			{
				UIKit.ClosePanel<BattleReportPanel>();
				BattleManagerView.Instance.battleInPanel.battleProcessManager.BackSearchProcess();
			});
            btn_lose_restart.onClick.AddListener(() =>
            {
                UIKit.ClosePanel<BattleReportPanel>();
                BattleManagerView.Instance.battleInPanel.battleProcessManager.BackSearchProcess();
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
