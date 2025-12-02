using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
	// Generate Id:80df5289-0f4e-49c6-b073-4bfd0d453fee
	public partial class HomeMainPanel
	{
		public const string Name = "HomeMainPanel";
		
		[SerializeField]
		public UnityEngine.UI.Button btn_goBattle;
		[SerializeField]
		public UnityEngine.UI.Button btn_goBag;
		
		private HomeMainPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			btn_goBattle = null;
			btn_goBag = null;
			
			mData = null;
		}
		
		public HomeMainPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		HomeMainPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new HomeMainPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
