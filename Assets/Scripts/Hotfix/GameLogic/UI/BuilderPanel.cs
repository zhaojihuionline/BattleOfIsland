using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System.Collections.Generic;

namespace GAME.UI
{
    public class BuilderPanelData : UIPanelData
    {

    }
    public partial class BuilderPanel : UIPanel, IController
    {

        [Header("测试用建筑预制体")]
        public GameObject[] testBuildingPrefabs; // 用于测试的建筑预制体数组

        public Transform itemParent;
        public List<BuilderDownItem> buildItemList = new List<BuilderDownItem>();

        //我需要当前已经解锁的全部建筑和其数量 这可以是个model  因为全局都是这份数据
        //建筑model中包含建筑data 用ID进行维护？？？
        //建筑Data目前需要建造点 占地大小  或者可以用碰撞体直接检测是否可以放置

        protected override void OnInit(IUIData uiData = null)
        {
            mData = uiData as BuilderPanelData ?? new BuilderPanelData();

            //监听拖拽事件 测试用
            TypeEventSystem.Global.Register<DragOutEvent>(x =>
            {
                Debug.Log(x.DraggedObject.name);
                GridTrackerApp.Interface.SendCommand<SelectBuildingCommand>(new SelectBuildingCommand(testBuildingPrefabs[0], true));
            }).UnRegisterWhenGameObjectDestroyed(this);


        }

        protected override void OnOpen(IUIData uiData = null)
        {

        }

        protected override void OnShow()
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnClose()
        {
            //model = null;
        }

        public IArchitecture GetArchitecture()
        {
            return BuilderPanelApp.Interface;
        }
    }
}
