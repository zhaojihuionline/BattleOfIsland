using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
	// Generate Id:62b50d00-e1cd-43dc-b7f1-b4f176a533a3
	public partial class LoginPanel
	{
		public const string Name = "LoginPanel";
		
		
		private LoginPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public LoginPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		LoginPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new LoginPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
