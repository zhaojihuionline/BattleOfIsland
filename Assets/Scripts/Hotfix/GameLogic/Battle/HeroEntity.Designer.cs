// Generate Id:4a32ac41-5efb-4c8c-a673-074905c829cd
using UnityEngine;

namespace QFramework.Game
{
	public partial class HeroEntity : QFramework.IController
	{
		QFramework.IArchitecture QFramework.IBelongToArchitecture.GetArchitecture()=>GameApp.Interface;
	}
}
