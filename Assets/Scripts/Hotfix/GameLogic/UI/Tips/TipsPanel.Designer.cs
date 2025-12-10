using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
    // TipsPanel Designer
    public partial class TipsPanel
    {
        public const string Name = "TipsPanel";
        
        private TipsData mPrivateData = null;
        
        protected override void ClearUIComponents()
        {
            mData = null;
        }
        
        public TipsData Data
        {
            get
            {
                return mData;
            }
        }
        
        TipsData mData
        {
            get
            {
                return mPrivateData ?? (mPrivateData = new TipsData("", TipsType.Info));
            }
            set
            {
                mUIData = value;
                mPrivateData = value;
            }
        }
    }
}
