/****************************************************************************
 * 2025.10 DESKTOP-HUQFF5N
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;
using TMPro;

namespace QFramework.UI
{
	public partial class PlayerInfoContainer
	{
		public Image avatar;
		public TMP_Text playerName;
        public TMP_Text WoodCountLabel;
        public TMP_Text StoneCountLabel;
        public TMP_Text MeatCountLabel;
        public void Clear()
		{
		}

		public override string ComponentName
		{
			get { return "PlayerInfoContainer";}
		}
	}
}
