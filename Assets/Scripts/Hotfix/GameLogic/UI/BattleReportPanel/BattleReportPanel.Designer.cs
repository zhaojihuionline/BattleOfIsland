using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
	// Generate Id:9b18e529-6f91-4750-a45b-aabf1ef3940c
	public partial class BattleReportPanel
	{
		public const string Name = "BattleReportPanel";
		public Button btn_win_restart;
        public Button btn_lose_restart;


        private BattleReportPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
            btn_win_restart = null;
			btn_lose_restart = null;
            mData = null;
		}
		
		public BattleReportPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		BattleReportPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new BattleReportPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
