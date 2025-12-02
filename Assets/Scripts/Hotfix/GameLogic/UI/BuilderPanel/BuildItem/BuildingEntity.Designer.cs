// Generate Id:b8c866e2-4884-438f-90a1-5ff6d386a1ec
using UnityEngine;

namespace GAME.QF
{
	public partial class BuildingEntity : QFramework.IController
	{
		public UnityEngine.MeshRenderer FangZhiGlow_G;
		
		public NeighborAwareEntity NeighborAwareEntity;
		
		QFramework.IArchitecture QFramework.IBelongToArchitecture.GetArchitecture()=>GridTrackerApp.Interface;
	}
}
