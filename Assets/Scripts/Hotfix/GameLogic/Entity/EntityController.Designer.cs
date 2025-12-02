// Generate Id:d904c46a-e8e5-4fae-95b2-0cafed6c36a4
using UnityEngine;

namespace QFramework.Game
{
	public partial class EntityController : QFramework.IController
	{
		public Pathfinding.AIPath aiPath;
		
		public Pathfinding.AIDestinationSetter aiDestination;
		
		public EntityBloodController bloodController;
		
		QFramework.IArchitecture QFramework.IBelongToArchitecture.GetArchitecture()=>GameApp.Interface;
	}
}
