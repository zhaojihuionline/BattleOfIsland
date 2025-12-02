/****************************************************************************
 * 2025.9 DESKTOP-JGMP0TA
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GAME.UI
{
	public partial class BuilderDownItem
	{
		[SerializeField] public DragController icon;

		public void Clear()
		{
			icon = null;
		}

		public override string ComponentName
		{
			get { return "BuilderDownItem";}
		}
	}
}
