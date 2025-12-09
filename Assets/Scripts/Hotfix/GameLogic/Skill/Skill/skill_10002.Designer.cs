// Generate Id:604085da-9e85-4061-8283-e59ae5e503ea
using UnityEngine;

namespace QFramework.Game
{
    public partial class skill_10002 : QFramework.IController
    {
        public UnityEngine.GameObject Model;

        public BPathMove BPath;
        [SerializeField] GameObject FX_Skill_Oliver;

        QFramework.IArchitecture QFramework.IBelongToArchitecture.GetArchitecture() => GameApp.Interface;
    }
}