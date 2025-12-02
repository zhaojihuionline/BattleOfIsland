using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
	// Generate Id:7c5d8f2c-85f7-494d-a28f-2e0d35625478
	public partial class BattleInPanel
	{
		public const string Name = "BattleInPanel";
		
		[SerializeField]
		public BattleProcessManager battleProcessManager;
		[SerializeField]
		public ActorInfoViewContainer ActorInfoViewContainerInBattle;
        public ActorInfoViewContainer ActorInfoViewContainersearch;

		public Transform searchBattle;
        public Transform inBattle;

        protected override void ClearUIComponents()
		{
            battleProcessManager = null;
			ActorInfoViewContainersearch = null;
            ActorInfoViewContainerInBattle = null;
		}
	}
}
