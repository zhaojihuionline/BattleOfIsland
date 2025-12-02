// Generate Id:f267e410-0301-4e4e-a921-92810ffad8c2
using UnityEngine;

namespace QFramework.Game
{
	public partial class skill_normal_shoot : QFramework.IController
	{
		public UnityEngine.GameObject Model;

		public BPathMove BPath;

		QFramework.IArchitecture QFramework.IBelongToArchitecture.GetArchitecture() => GameApp.Interface;
	}
}
