using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GAME.UI
{
	// Generate Id:ed8012be-e1c1-4ff9-b439-675caad50017
	public partial class BuilderPanel
	{
		public const string Name = "BuilderPanel";
		
		[SerializeField]
		public BuilderDownItem BuilderDownItem;
		
		private BuilderPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			BuilderDownItem = null;
			
			mData = null;
		}
		
		public BuilderPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		BuilderPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new BuilderPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
