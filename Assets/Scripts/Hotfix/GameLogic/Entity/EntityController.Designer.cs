// Generate Id:2e359ff4-6e69-4d7d-aceb-054f45cdcbf8
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
