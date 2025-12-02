using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
	// Generate Id:44dda853-4a1d-43b1-9351-9c6197703e4d
	public partial class BagPanel
	{
		public const string Name = "BagPanel";
		
		
		private BagPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public BagPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		BagPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new BagPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
