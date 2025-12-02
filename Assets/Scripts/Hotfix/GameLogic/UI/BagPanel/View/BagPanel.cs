using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace QFramework.UI
{
	[System.Serializable]
	public class TabConfig
	{
		public BagTabButton tabButton;  // 手动拖放的BagTabButton脚本
	}
	
	public class BagPanelData : UIPanelData
	{
	}
	
	public partial class BagPanel : UIPanel, IController
	{
		[Header("Tab设置")]
		[SerializeField] private ToggleGroup tabToggleGroup;
		[SerializeField] private List<TabConfig> tabConfigs = new List<TabConfig>();
		
		[Header("物品列表设置")]
		[SerializeField] private Transform itemContainer;  // Content 节点，用于放置物品
		[SerializeField] private ScrollRect scrollRect;  // ScrollRect 组件，用于重置滚动位置
		[SerializeField] private GameObject bagItemPrefab;  // BagItem 预制体（在 Inspector 中拖放）
		[SerializeField] private int columnsCount = 6;  // 列数
		[SerializeField] private float spacingX = 10f;  // 水平间距
		[SerializeField] private float spacingY = 10f;  // 垂直间距
		
	private IBagTabSystem bagTabSystem;
	private IBagModel bagModel;
	private List<BagItemView> itemViews = new List<BagItemView>();  // 当前显示的物品视图列表
	private GridLayoutGroup gridLayoutGroup;  // 缓存的 GridLayoutGroup 组件
	private Vector2 lastContentSize;  // 记录上次的 Content 尺寸，用于检测尺寸变化
	
	protected override void OnInit(IUIData uiData = null)
	{
		mData = uiData as BagPanelData ?? new BagPanelData();
		
		// 获取系统和模型
		bagTabSystem = this.GetSystem<IBagTabSystem>();
		bagModel = this.GetModel<IBagModel>();
		
		// 如果 itemContainer 未设置，尝试通过路径查找
		FindItemContainer();
		
		// 初始化 Content 尺寸记录
		if (itemContainer != null)
		{
			RectTransform contentRect = itemContainer as RectTransform;
			if (contentRect != null)
			{
				lastContentSize = contentRect.rect.size;
			}
		}
		
		// 动态调整 GridLayoutGroup 的 CellSize 以适配屏幕
		// 延迟一帧执行，确保 Layout 已经计算完成
		UniTask.Void(async () =>
		{
			await UniTask.Yield();
			AdjustGridLayoutCellSize();
		});
		
		// 初始化Tab
		InitTabs();
		
		// 监听Tab切换事件
			this.RegisterEvent<BagTabChangedEvent>(OnTabChanged)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			
			// 监听物品更新事件
			this.RegisterEvent<BagItemsUpdatedEvent>(OnBagItemsUpdated)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			
			// 检查背包是否为空，如果为空则初始化测试数据
			CheckAndInitializeBag();
		}
		
		/// <summary>
		/// 查找物品容器和滚动视图
		/// </summary>
		private void FindItemContainer()
		{
			// 如果已经设置，直接返回
			if (itemContainer != null && scrollRect != null) return;
			
			if (itemContainer == null)
			{
				Debug.LogWarning("BagPanel: ItemContainer未设置，且无法通过路径查找！请检查预制体结构或手动设置。");
			}
		}
		
		/// <summary>
		/// 动态调整 GridLayoutGroup 的 CellSize 以适配屏幕宽度
		/// </summary>
		private void AdjustGridLayoutCellSize()
		{
			if (itemContainer == null)
			{
				FindItemContainer();
			}
			
			if (itemContainer == null)
			{
				Debug.LogWarning("BagPanel: ItemContainer 未设置，无法调整 GridLayout！");
				return;
			}
			
			// 获取或缓存 GridLayoutGroup 组件
			if (gridLayoutGroup == null)
			{
				gridLayoutGroup = itemContainer.GetComponent<GridLayoutGroup>();
			}
			
			if (gridLayoutGroup == null)
			{
				Debug.LogWarning("BagPanel: Content 节点缺少 GridLayoutGroup 组件！");
				return;
			}
			
			// 强制重建布局，确保获取到正确的宽度
			LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer as RectTransform);
			
			// 获取 Content 的实际宽度（考虑 padding）
			RectTransform contentRect = itemContainer as RectTransform;
			float availableWidth = contentRect.rect.width - gridLayoutGroup.padding.left - gridLayoutGroup.padding.right;
			
			// 计算每列的宽度：可用宽度 / 列数 - 间距
			// 总间距 = (列数 - 1) * spacingX
			float totalSpacing = (columnsCount - 1) * spacingX;
			float cellWidth = (availableWidth - totalSpacing) / columnsCount;
			
			// 确保 cellWidth 不为负数或过小
			if (cellWidth <= 0)
			{
				Debug.LogWarning($"BagPanel: 计算出的 CellWidth ({cellWidth}) 无效，使用默认值 160");
				cellWidth = 160f;
			}
			
			// 保持宽高比（正方形），或者可以设置为固定高度
			float cellHeight = cellWidth;  // 如果希望是正方形
			
			// 更新 GridLayoutGroup 设置
			gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
			gridLayoutGroup.spacing = new Vector2(spacingX, spacingY);
			gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			gridLayoutGroup.constraintCount = columnsCount;
			
			// 再次强制重建布局，应用新的 CellSize
			LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
			
			Debug.Log($"BagPanel: 调整 GridLayout CellSize 为 {cellWidth:F2}x{cellHeight:F2}，适配 {columnsCount} 列，可用宽度: {availableWidth:F2}");
		}
		
		/// <summary>
		/// 初始化Tab
		/// </summary>
		private void InitTabs()
		{
			if (tabToggleGroup == null)
			{
				Debug.LogError("BagPanel: TabToggleGroup未设置！");
				return;
			}
			
			// 初始化每个手动拖放的Tab按钮
			for (int i = 0; i < tabConfigs.Count; i++)
			{
				var config = tabConfigs[i];
				if (config.tabButton == null)
				{
					Debug.LogError($"BagPanel: TabConfig[{i}]的TabButton未设置！");
					continue;
				}
				
				// 设置Tab索引
				config.tabButton.TabIndex = i;
				
				// 初始化Tab按钮（由BagTabButton脚本处理所有配置）
				int index = i;  // 闭包问题，需要保存索引
				config.tabButton.Initialize(tabToggleGroup, (tabIndex) =>
				{
					bagTabSystem.SwitchTab(tabIndex);
				});
			}
			
			// 默认选中第一个Tab
			if (tabConfigs.Count > 0 && tabConfigs[0].tabButton != null)
			{
				tabConfigs[0].tabButton.Toggle.isOn = true;
			}
		}
		
		/// <summary>
		/// Tab切换事件处理
		/// </summary>
		private void OnTabChanged(BagTabChangedEvent e)
		{
			// 发送命令处理Tab切换的业务逻辑（由Controller层发送Command）
			// Command 内部会判断是否需要从网络获取数据，并发送 BagItemsUpdatedEvent
			this.SendCommand(new SwitchBagTabCommand 
			{ 
				OldIndex = e.OldIndex, 
				NewIndex = e.NewIndex 
			});
		}
		
		/// <summary>
		/// 物品更新事件处理
		/// </summary>
		private void OnBagItemsUpdated(BagItemsUpdatedEvent e)
		{
			RefreshItemList(e.TabIndex);
		}
		
		/// <summary>
		/// 刷新物品列表
		/// </summary>
		private void RefreshItemList(int tabIndex)
		{
			// 确保 itemContainer 已找到
			if (itemContainer == null)
			{
				FindItemContainer();
			}
			
			if (itemContainer == null)
			{
				Debug.LogError("BagPanel: ItemContainer未设置，且无法通过路径查找！请检查预制体结构或手动设置。");
				return;
			}
			
			// 清空旧物品
			foreach (var itemView in itemViews)
			{
				if (itemView == null) continue;
				
				itemView.OnClicked = null;
				
				if (itemView.gameObject != null)
				{
					GameObject.Destroy(itemView.gameObject);
				}
			}
			itemViews.Clear();
			
			// 获取当前Tab的物品数据
			var items = bagModel.GetItemsByTab(tabIndex);
			
			// 检查预制体是否已设置
			if (bagItemPrefab == null)
			{
				Debug.LogError("BagPanel: BagItem预制体未设置！请在 Inspector 中拖放 BagItem.prefab 到 bagItemPrefab 字段。");
				return;
			}
			
			// 实例化物品
			foreach (var itemData in items)
			{
				var itemObj = GameObject.Instantiate(bagItemPrefab, itemContainer);
				var itemView = itemObj.GetComponent<BagItemView>();
				
				if (itemView != null)
				{
					itemView.SetData(itemData);
					itemView.OnClicked = HandleBagItemClicked;
					itemViews.Add(itemView);
				}
				else
				{
					Debug.LogError("BagPanel: BagItem预制体缺少BagItemView组件！");
					GameObject.Destroy(itemObj);
				}
			}
			
			// 重置滚动位置到顶部
			if (scrollRect != null)
			{
				scrollRect.verticalNormalizedPosition = 1f;
			}
			
		Debug.Log($"BagPanel: 刷新Tab {tabIndex} 的物品列表，数量: {items.Count}");
	}
	
	protected override void OnOpen(IUIData uiData = null)
	{
		// 重新调整布局（防止屏幕尺寸变化或窗口大小改变）
		UniTask.Void(async () =>
		{
			await UniTask.Yield();
			AdjustGridLayoutCellSize();
		});
		
		// 确保显示当前Tab
		int currentIndex = bagTabSystem.CurrentTabIndex;
		if (currentIndex >= 0 && currentIndex < tabConfigs.Count)
		{
			OnTabChanged(new BagTabChangedEvent 
			{ 
				OldIndex = -1, 
				NewIndex = currentIndex 
			});
		}
	}
		
		protected override void OnClose()
		{
			// 清空物品视图
			foreach (var itemView in itemViews)
			{
				if (itemView == null) continue;
				
				itemView.OnClicked = null;
				
				if (itemView.gameObject != null)
				{
					GameObject.Destroy(itemView.gameObject);
				}
			}
			itemViews.Clear();
		}
		
		protected override void OnShow()
		{
		}
		
	protected override void OnHide()
	{
	}
	
	/// <summary>
	/// 检测 Content 尺寸变化，自动调整布局
	/// </summary>
	private void Update()
	{
		// 只在面板显示时检测
		if (!gameObject.activeInHierarchy) return;
		
		// 检测 Content 尺寸变化（更精确，因为 Canvas 缩放会导致 Content 尺寸变化）
		if (itemContainer != null)
		{
			RectTransform contentRect = itemContainer as RectTransform;
			if (contentRect != null)
			{
				Vector2 currentContentSize = contentRect.rect.size;
				// 检查尺寸是否发生变化，且尺寸有效（大于0）
				if (currentContentSize != lastContentSize && currentContentSize.x > 0 && currentContentSize.y > 0)
				{
					lastContentSize = currentContentSize;
					// 延迟一帧执行，确保 Layout 已经更新
					UniTask.Void(async () =>
					{
						await UniTask.Yield();
						AdjustGridLayoutCellSize();
					});
				}
			}
		}
	}
	
	/// <summary>
	/// 检查背包是否为空，如果为空则初始化测试数据
	/// </summary>
		private void CheckAndInitializeBag()
		{
			if (bagModel == null)
			{
				Debug.LogWarning("BagPanel: BagModel 未注册，无法检查背包状态");
				return;
			}

			// 检查所有 Tab 是否都有数据
			bool hasAnyData = false;
			for (int i = 0; i < tabConfigs.Count; i++)
			{
				if (bagModel.IsTabLoaded(i))
				{
					var items = bagModel.GetItemsByTab(i);
					if (items != null && items.Count > 0)
					{
						hasAnyData = true;
						break;
					}
				}
			}

			// 如果所有 Tab 都没有数据，则初始化测试数据
			if (!hasAnyData)
			{
				Debug.Log("BagPanel: 检测到背包为空，开始初始化测试数据...");
				// 使用 UniTask.Void 在后台执行异步初始化，不阻塞 OnInit
				UniTask.Void(async () =>
				{
					await this.SendCommand(new InitializeBagCommand());
				});
			}
		}

		/// <summary>
		/// 处理 BagItem 点击，先通过日志验证交互链路
		/// </summary>
		private void HandleBagItemClicked(BagItemView view)
		{
			if (view?.Data == null)
			{
				Debug.Log("点击 BagItem：数据为空");
				return;
			}
			
			Debug.Log($"点击 BagItem → Tab:{bagTabSystem.CurrentTabIndex} BagId:{view.Data.BagId} ItemId:{view.Data.ItemId}");
			// TODO: 根据设计在此触发详情、指令等后续逻辑
		}
		
		// 实现IController接口
		public IArchitecture GetArchitecture()
		{
			return GameApp.Interface;
		}
	}
}
