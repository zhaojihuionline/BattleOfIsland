/****************************************************************************
 * 2025.10 DESKTOP-HUQFF5N
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
	public partial class ActorInfoViewContainer
	{
		[SerializeField] public PlayerInfoContainer PlayerInfoContainer;
		[SerializeField] public PlayerInfoContainer OpponentInfoContainer;

		public void Clear()
		{
			PlayerInfoContainer = null;
			OpponentInfoContainer = null;
		}

		public override string ComponentName
		{
			get { return "ActorInfoViewContainer";}
		}
	}
}
