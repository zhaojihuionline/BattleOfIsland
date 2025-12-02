using UnityEngine;
using QFramework;

namespace GAME.QF
{
    public partial class BuildingEntity : ViewController
    {
        //建筑点
        private Transform[] pointlist = new Transform[4];
        //底板
        private Transform panel;
        //大小
        [SerializeField] private int scale = 1;

        [Header("预览颜色设置")]
        public Color canPlaceColor = Color.green;     // 可放置时的预览颜色（绿色）
        public Color cannotPlaceColor = Color.red;    // 不可放置时的预览颜色（红色）
        public Color normalColor = Color.clear;    // 不可放置时的预览颜色（红色）

        BuildingPlacementModel bModel;
        GridTrackerModel gModel;
        IGridTrackerSystem gSystem;

        private void Awake()
        {
            bModel = this.GetModel<BuildingPlacementModel>();
            gModel = this.GetModel<GridTrackerModel>();
            gSystem = this.GetSystem<IGridTrackerSystem>();

            FSM.AddState(States.Normalstate, new Normalstate(FSM, this));
            FSM.AddState(States.Previewstate, new Previewstate(FSM, this));
            FSM.AddState(States.Functionalstate, new Functionalstate(FSM, this));

            // 初始状态为普通状态
            FSM.StartState(States.Normalstate);

            var Direction = transform.Find("Direction");
            if (Direction != null)
            {
                for (int i = 0; i < pointlist.Length; i++)
                {
                    pointlist[i] = Direction.GetChild(i);
                }
            }

            // 注册事件监听
            SetupEventListeners();
        }

        public Transform GetPointChild(int index)
        {
            return pointlist[index];
        }
        /// <summary>
        /// 设置事件监听
        /// </summary>
        private void SetupEventListeners()
        {
            // 监听建筑放置确认事件
            this.RegisterEvent<BuildingPlacementConfirmedEvent>(e =>
            {
                // 只处理当前预览实例的事件
                if (bModel.CurrentBuildingPrefab == gameObject)
                {
                    ChangeState(States.Normalstate);
                    NeighborAwareEntity.StopDragging();
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 监听建筑选中事件
            this.RegisterEvent<SelectBuildingCommand>(e =>
            {
                // 只处理当前预览实例的事件
                if (bModel.CurrentBuildingPrefab == gameObject)
                {
                    ChangeState(States.Previewstate);
                    NeighborAwareEntity.StartDragging();
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 监听建筑放置取消事件
            this.RegisterEvent<BuildingPlacementCanceledEvent>(e =>
            {
                if (bModel.CurrentBuildingPrefab == gameObject)
                {

                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 监听网格数据更新事件，用于更新预览颜色
            this.RegisterEvent<GridTrackerDataUpdatedEvent>(e =>
            {
                if (bModel.CurrentBuildingPrefab == gameObject && FSM.CurrentStateId == States.Previewstate)
                {
                    bModel.CanPlaceAtCurrentPosition = CheckIfCanPlace();
                    UpdatePreviewColor(bModel.CanPlaceAtCurrentPosition);
                    NeighborAwareEntity.UpdateWhileDragging();
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 预览模式监听
            this.RegisterEvent<StartBuildingPlacementEvent>(e =>
            {
                if (bModel.CurrentBuildingPrefab == gameObject && FSM.CurrentStateId != States.Previewstate)
                {
                    ChangeState(States.Previewstate);
                    ActionKit.NextFrame(() =>
                    {
                        bModel.CanPlaceAtCurrentPosition = CheckIfCanPlace();
                        UpdatePreviewColor(bModel.CanPlaceAtCurrentPosition);
                        NeighborAwareEntity.StartDragging();
                    }).Start(this);
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        public void ChangeState(States target)
        {
            FSM.ChangeState(target);
        }

        public void RefState(States target)
        {
            FSM.ChangeState(target);
        }

        /// <summary>
        /// 更新预览颜色
        /// </summary>
        public void UpdatePreviewColor(bool canPlace)
        {
            ChangeCoolor(canPlace ? Color.green : Color.red);
        }

        public void ChangeCoolor(Color color)
        {
            if (FangZhiGlow_G != null)
            {
                FangZhiGlow_G.material.color = color;
                //Debug.Log($"更新颜色: {color}, 可放置: {color == Color.green}");
            }
        }

        /// <summary>
        /// 设置发光对象的可见性
        /// </summary>
        public void SetGlowVisibility(bool visible)
        {
            if (FangZhiGlow_G != null)
            {
                FangZhiGlow_G.gameObject.SetActive(visible);
                //Debug.Log($"设置发光对象可见性: {visible}");
            }
        }

        /// <summary>
        /// 检查当前位置是否可以放置建筑
        /// </summary>
        private bool CheckIfCanPlace()
        {
            if (FangZhiGlow_G == null) return true;

            // 直接使用FangZhiGlow_G当前的BoxCollider进行检测
            BoxCollider box = FangZhiGlow_G.GetComponent<BoxCollider>();
            if (box == null) return true;

            if (box == null) return false;

            Vector3 center = box.transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale * 0.95f);
            Quaternion orientation = box.transform.rotation;

            LayerMask obstacleMask = 1 << LayerMask.NameToLayer("BuildObstacle");

            Collider[] overlappingColliders = Physics.OverlapBox(center, halfExtents, orientation, obstacleMask);

            // 检查是否有除了自身以外的碰撞器
            foreach (Collider col in overlappingColliders)
            {
                if (col != box && col.transform != box.transform)
                    return false;
            }

            return true;
        }

        private void OnDestroy()
        {
            FSM.Clear();
        }

        /// <summary>
        /// 建筑状态 当前三种 
        /// </summary>
        public enum States
        {
            Previewstate,       //预览状态
            Normalstate,        //普通状态
            Functionalstate,    //功能状态
        }

        public FSM<States> FSM = new FSM<States>();

        public class Previewstate : AbstractState<States, BuildingEntity>
        {
            public Previewstate(FSM<States> fsm, BuildingEntity target) : base(fsm, target)
            {
            }

            protected override bool OnCondition()
            {
                return mFSM.CurrentStateId != States.Previewstate;
            }

            protected override void OnEnter()
            {
                Debug.Log("进入Previewstate");
                var gridModel = mTarget.GetModel<BuildingPlacementModel>();
                mTarget.UpdatePreviewColor(gridModel.CanPlaceAtCurrentPosition);
            }

            protected override void OnExit()
            {
            }
        }

        public class Normalstate : AbstractState<States, BuildingEntity>
        {
            public Normalstate(FSM<States> fsm, BuildingEntity target) : base(fsm, target)
            {
            }

            protected override bool OnCondition()
            {
                return mFSM.CurrentStateId != States.Normalstate;
            }

            protected override void OnEnter()
            {
                Debug.Log("进入 Normalstate");
                mTarget.ChangeCoolor(Color.clear);
            }
        }

        public class Functionalstate : AbstractState<States, BuildingEntity>
        {
            public Functionalstate(FSM<States> fsm, BuildingEntity target) : base(fsm, target)
            {
            }

            protected override bool OnCondition()
            {
                return mFSM.CurrentStateId != States.Functionalstate;
            }

            protected override void OnEnter()
            {
                mTarget.ChangeCoolor(Color.clear);
            }
        }

        // 在Inspector中值改变时调用（仅在编辑模式下）
        private void OnValidate()
        {
            if (FangZhiGlow_G == null) return;

            var Direction = transform.Find("Direction");
            if (Direction == null) return;

            for (int i = 0; i < pointlist.Length; i++)
            {
                pointlist[i] = Direction.GetChild(i);
            }
            panel = FangZhiGlow_G.gameObject.transform;
            float g = 0;
            if (scale % 2 == 1)
            {
                g = scale / 2 - scale / 4;
            }
            else
            {
                g = scale / 2 - 0.5f - scale / 5;
            }

            pointlist[0].position = new Vector3(g, 0, -g);
            pointlist[1].position = new Vector3(-g, 0, -g);
            pointlist[2].position = new Vector3(-g, 0, g);
            pointlist[3].position = new Vector3(g, 0, g);

            panel.localScale = new Vector3(scale, 0.1f, scale);
        }
    }
}
