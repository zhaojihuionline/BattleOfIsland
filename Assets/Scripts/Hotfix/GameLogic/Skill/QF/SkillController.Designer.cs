// Generate Id:4ba3810f-c524-41a2-a122-f2d65f5a88b3
using UnityEngine;

namespace QFramework.Game
{
	public partial class SkillController : QFramework.IController
	{
		QFramework.IArchitecture QFramework.IBelongToArchitecture.GetArchitecture()=>GameApp.Interface;
	}
}
