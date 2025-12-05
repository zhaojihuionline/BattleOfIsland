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

		[Header("Page Views")]
		[SerializeField] private Transform rightNode;  // right 节点
		[SerializeField] private PageUseType1View pageUseType1View;
		[SerializeField] private PageUseType2FixedView pageUseType2FixedView;
		[SerializeField] private PageUseType2RandomView pageUseType2RandomView;
		[SerializeField] private PageUseType2ChoiceView pageUseType2ChoiceView;
		[SerializeField] private PageUseType3View pageUseType3View;
		[SerializeField] private PageUseTypeXView pageUseTypeXView;
		[SerializeField] private PageEmptyView pageEmptyView;

		[Header("Popup Views")]
		[SerializeField] private ObtainRewardsView obtainRewardsView;

		[Header("返回按钮")]
		[SerializeField] private Button returnButton;  // top/group-return 节点的返回按钮

		private IBagTabSystem bagTabSystem;
		private IBagModel bagModel;
		private List<BagItemView> itemViews = new List<BagItemView>();  // 当前显示的物品视图列表
		private GridLayoutGroup gridLayoutGroup;  // 缓存的 GridLayoutGroup 组件
		private Vector2 lastContentSize;  // 记录上次的 Content 尺寸，用于检测尺寸变化

		// 所有 Page View 的列表
		private List<BagPageViewBase> allPageViews = new List<BagPageViewBase>();
		private BagPageViewBase currentActivePageView;
		private BagItemData currentSelectedItemData;  // 当前选中的物品数据

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

			// 初始化 Page Views
			InitPageViews();
			InitPopupViews();

			// 初始化返回按钮
			InitReturnButton();

			// 监听Tab切换事件
			this.RegisterEvent<BagTabChangedEvent>(OnTabChanged)
				.UnRegisterWhenGameObjectDestroyed(gameObject);

			// 监听物品更新事件
			this.RegisterEvent<BagItemsUpdatedEvent>(OnBagItemsUpdated)
				.UnRegisterWhenGameObjectDestroyed(gameObject);

			// 监听奖励弹窗事件
			this.RegisterEvent<RewardsObtainedEvent>(OnRewardsObtained)
				.UnRegisterWhenGameObjectDestroyed(gameObject);

			// 检查背包是否为空，如果为空则初始化测试数据
			CheckAndInitializeBag();
		}

		private void InitPopupViews()
		{
            if (obtainRewardsView == null)
            {
                var popup = transform.Find("popup/ObtainRewards");
                if (popup != null)
                {
                    obtainRewardsView = popup.GetComponent<ObtainRewardsView>();
                }
            }
        }

		/// <summary>
		/// 初始化返回按钮
		/// </summary>
		private void InitReturnButton()
		{
			// 绑定返回按钮点击事件
			if (returnButton != null)
			{
				returnButton.onClick.AddListener(OnReturnButtonClicked);
			}
			else
			{
				Debug.LogWarning("BagPanel: 返回按钮未找到，请检查路径 top/group-return 或手动设置 returnButton");
			}
		}

		/// <summary>
		/// 返回按钮点击事件处理
		/// </summary>
		private void OnReturnButtonClicked()
		{
			Debug.Log("BagPanel: 返回按钮被点击，关闭背包面板并返回主界面");
			// 关闭背包面板，返回主界面
			UIKit.ClosePanel<BagPanel>();
			UIKit.ShowPanel<HomeMainPanel>();
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
			// 获取当前显示的Tab索引
			int currentTabIndex = bagTabSystem.CurrentTabIndex;
			
			// 只有更新的Tab是当前显示的Tab时，才刷新物品列表
			// 这样可以避免更新其他Tab时导致当前Tab被切换
			if (e.TabIndex == currentTabIndex)
			{
				RefreshItemList(e.TabIndex);
				
				// 刷新当前显示的页面视图（如果当前页面显示的是被更新的物品）
				if (currentActivePageView != null && currentSelectedItemData != null)
				{
					// 从BagModel中获取最新的物品数据
					var updatedItemData = bagModel.GetItemByBagId(currentSelectedItemData.BagId);
					
					if (updatedItemData != null)
					{
						// 物品还存在，刷新页面视图
						currentSelectedItemData = updatedItemData;
						currentActivePageView.RefreshData(updatedItemData);
						Debug.Log($"BagPanel: 刷新当前页面视图，BagId={updatedItemData.BagId}, Count={updatedItemData.Count}");
					}
					else
					{
						// 物品已被移除，显示空页面
						currentSelectedItemData = null;
						SwitchPageByItemData(null);
						Debug.Log($"BagPanel: 当前物品已被移除，显示空页面");
					}
				}
			}
			else
			{
				// 更新的不是当前显示的Tab，只更新数据模型，不刷新UI
				Debug.Log($"BagPanel: Tab {e.TabIndex} 的物品已更新，但不是当前显示的Tab（当前Tab: {currentTabIndex}），仅更新数据模型");
			}
		}

		/// <summary>
		/// 获得奖励事件处理
		/// </summary>
		private void OnRewardsObtained(RewardsObtainedEvent e)
		{
			if (e.Deltas == null || e.Deltas.Count == 0) return;

			if (obtainRewardsView == null)
			{
				InitPopupViews();
			}

			if (obtainRewardsView != null)
			{
				obtainRewardsView.Show(e.Deltas);
			}
			else
			{
				Debug.LogWarning("BagPanel: ObtainRewardsView 未设置，无法展示奖励");
			}
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

			// 如果物品列表为空，显示 PageEmpty
			if (items == null || items.Count == 0)
			{
				SwitchPageByItemData(null);
				Debug.Log($"BagPanel: Tab {tabIndex} 的物品列表为空，显示 PageEmpty");
				return;
			}

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
			// 清理返回按钮事件
			if (returnButton != null)
			{
				returnButton.onClick.RemoveListener(OnReturnButtonClicked);
			}

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
				SwitchPageByItemData(null);
				return;
			}

			Debug.Log($"点击 BagItem → Tab:{bagTabSystem.CurrentTabIndex} BagId:{view.Data.BagId} ItemId:{view.Data.ItemId}");

			// 根据物品数据切换显示对应的 Page
			SwitchPageByItemData(view.Data);
		}

		/// <summary>
		/// 初始化所有 Page View
		/// </summary>
		private void InitPageViews()
		{
			// 如果 rightNode 未设置，尝试查找
			if (rightNode == null)
			{
				rightNode = transform.Find("right");
				if (rightNode == null)
				{
					Debug.LogError("BagPanel: 找不到 right 节点！");
					return;
				}
			}

			// 如果 Page View 未设置，尝试通过组件查找
			if (pageUseType1View == null)
			{
				var pageObj = rightNode.Find("PageUseType1")?.gameObject;
				if (pageObj != null) pageUseType1View = pageObj.GetComponent<PageUseType1View>();
			}

			if (pageUseType2FixedView == null)
			{
				var pageObj = rightNode.Find("PageUseType2-Fixed")?.gameObject;
				if (pageObj != null) pageUseType2FixedView = pageObj.GetComponent<PageUseType2FixedView>();
			}

			if (pageUseType2RandomView == null)
			{
				var pageObj = rightNode.Find("PageUseType2-Random")?.gameObject;
				if (pageObj != null) pageUseType2RandomView = pageObj.GetComponent<PageUseType2RandomView>();
			}

			if (pageUseType2ChoiceView == null)
			{
				var pageObj = rightNode.Find("PageUseType2-Choice")?.gameObject;
				if (pageObj != null) pageUseType2ChoiceView = pageObj.GetComponent<PageUseType2ChoiceView>();
			}

			if (pageUseType3View == null)
			{
				var pageObj = rightNode.Find("PageUseType3")?.gameObject;
				if (pageObj != null) pageUseType3View = pageObj.GetComponent<PageUseType3View>();
			}

			if (pageUseTypeXView == null)
			{
				var pageObj = rightNode.Find("PageUseTypeX")?.gameObject;
				if (pageObj != null) pageUseTypeXView = pageObj.GetComponent<PageUseTypeXView>();
			}

			if (pageEmptyView == null)
			{
				var pageObj = rightNode.Find("PageEmpty")?.gameObject;
				if (pageObj != null) pageEmptyView = pageObj.GetComponent<PageEmptyView>();
			}

			// 收集所有 Page View
			allPageViews.Clear();
			if (pageUseType1View != null) allPageViews.Add(pageUseType1View);
			if (pageUseType2FixedView != null) allPageViews.Add(pageUseType2FixedView);
			if (pageUseType2RandomView != null) allPageViews.Add(pageUseType2RandomView);
			if (pageUseType2ChoiceView != null) allPageViews.Add(pageUseType2ChoiceView);
			if (pageUseType3View != null) allPageViews.Add(pageUseType3View);
			if (pageUseTypeXView != null) allPageViews.Add(pageUseTypeXView);
			if (pageEmptyView != null) allPageViews.Add(pageEmptyView);

			// 输出初始化结果
			Debug.Log($"BagPanel: Page Views 初始化完成 - " +
				$"PageUseType1:{(pageUseType1View != null ? "✓" : "✗")} " +
				$"PageUseType2Fixed:{(pageUseType2FixedView != null ? "✓" : "✗")} " +
				$"PageUseType2Random:{(pageUseType2RandomView != null ? "✓" : "✗")} " +
				$"PageUseType2Choice:{(pageUseType2ChoiceView != null ? "✓" : "✗")} " +
				$"PageUseType3:{(pageUseType3View != null ? "✓" : "✗")} " +
				$"PageUseTypeX:{(pageUseTypeXView != null ? "✓" : "✗")} " +
				$"PageEmpty:{(pageEmptyView != null ? "✓" : "✗")}");

			// 检查是否有 Page View 未初始化
			if (allPageViews.Count == 0)
			{
				Debug.LogError("BagPanel: 警告！所有 Page View 都为 null！请确保：\n" +
					"1. 在 Unity 中为每个 Page GameObject 添加对应的 View 脚本组件\n" +
					"2. 在 BagPanel 的 Inspector 中拖放 Page View 引用，或确保 right 节点下的 Page GameObject 名称正确");
			}

			// 默认隐藏所有 Page
			HideAllPages();
		}

		/// <summary>
		/// 隐藏所有 Page
		/// </summary>
		private void HideAllPages()
		{
			foreach (var pageView in allPageViews)
			{
				if (pageView != null)
				{
					pageView.Hide();
				}
			}
			currentActivePageView = null;
		}

		/// <summary>
		/// 切换到指定的 Page View
		/// </summary>
		/// <param name="pageView">要显示的 Page View</param>
		/// <param name="itemData">物品数据</param>
		private void SwitchToPage(BagPageViewBase pageView, BagItemData itemData = null)
		{
			// 隐藏当前 Page
			if (currentActivePageView != null && currentActivePageView != pageView)
			{
				currentActivePageView.Hide();
			}

			// 显示新 Page
			if (pageView != null)
			{
				currentActivePageView = pageView;
				pageView.Show();
				pageView.RefreshData(itemData);
				Debug.Log($"BagPanel: 切换到 Page {pageView.GetType().Name}");
			}
			else
			{
				Debug.LogWarning("BagPanel: 目标 Page View 为 null，尝试显示 Empty Page");
				// 如果找不到目标 Page，显示 Empty Page
				if (pageEmptyView != null)
				{
					SwitchToPage(pageEmptyView, null);
				}
				else
				{
					Debug.LogError("BagPanel: PageEmptyView 也为 null，无法显示任何 Page！请检查 Page View 是否正确初始化。");
				}
			}
		}

		/// <summary>
		/// 根据物品数据决定显示哪个 Page
		/// </summary>
		public void SwitchPageByItemData(BagItemData itemData)
		{
			Debug.Log($"BagPanel: SwitchPageByItemData 被调用，ItemData={(itemData != null ? $"ItemId={itemData.ItemId}" : "null")}");
			
			// 更新当前选中的物品数据
			currentSelectedItemData = itemData;
			
			if (itemData == null)
			{
				Debug.Log("BagPanel: ItemData 为 null，显示 PageEmpty");
				SwitchToPage(pageEmptyView, null);
				return;
			}

			// 使用 BagItemData 中已存储的配置表数据，避免重复查询
			Debug.Log($"BagPanel: 使用 BagItemData 中的配置数据，UseType={itemData.UseType}, UseLevel={itemData.UseLevel}, RewardID={itemData.RewardID}");

			// 1. 首先检查玩家建筑大厅等级是否满足要求
			int playerHallLevel = GetPlayerHallLevel();
			if (playerHallLevel < itemData.UseLevel)
			{
				Debug.Log($"BagPanel: 玩家等级不足 (当前:{playerHallLevel} < 需要:{itemData.UseLevel})，显示 PageUseTypeX");
				SwitchToPage(pageUseTypeXView, itemData);
				return;
			}

			// 2. 根据 UseType 判断显示哪个 Page
			BagPageViewBase targetPage = null;

			switch (itemData.UseType)
			{
				case cfg.Enum_UseType.DisplayUse:  // 1
					targetPage = pageUseType1View;
					Debug.Log($"BagPanel: UseType=DisplayUse，选择 PageUseType1View ({(targetPage != null ? "存在" : "为null")})");
					break;

				case cfg.Enum_UseType.CanUse:  // 2
											   // 根据 RewardID 获取 Reward 配置，根据 RewardType 选择对应的 PageUseType2 变体
					if (itemData.RewardID > 0)
					{
						try
						{
							var rewardConfig = CfgMgr.Instance.Tables.TbReward.Get(itemData.RewardID);
							if (rewardConfig != null)
							{
								Debug.Log($"BagPanel: RewardType={rewardConfig.RewardType}");
								switch (rewardConfig.RewardType)
								{
									case cfg.Enum_RewardType.Fixed:  // 1
										targetPage = pageUseType2FixedView;
										Debug.Log($"BagPanel: 选择 PageUseType2FixedView ({(targetPage != null ? "存在" : "为null")})");
										break;
									case cfg.Enum_RewardType.Random:  // 2
										targetPage = pageUseType2RandomView;
										Debug.Log($"BagPanel: 选择 PageUseType2RandomView ({(targetPage != null ? "存在" : "为null")})");
										break;
									case cfg.Enum_RewardType.Choice:  // 3
										targetPage = pageUseType2ChoiceView;
										Debug.Log($"BagPanel: 选择 PageUseType2ChoiceView ({(targetPage != null ? "存在" : "为null")})");
										break;
									default:
										Debug.LogWarning($"BagPanel: 未知的 RewardType {rewardConfig.RewardType}，使用 Fixed");
										targetPage = pageUseType2FixedView;
										break;
								}
							}
							else
							{
								Debug.LogWarning($"BagPanel: Reward 配置不存在 RewardID={itemData.RewardID}，使用 Fixed");
								targetPage = pageUseType2FixedView;
							}
						}
						catch (System.Exception ex)
						{
							Debug.LogError($"BagPanel: 无法获取 Reward 配置 RewardID={itemData.RewardID}, Error={ex.Message}，使用 Fixed");
							targetPage = pageUseType2FixedView;
						}
					}
					else
					{
						Debug.LogWarning($"BagPanel: 物品 RewardID 为 0，使用 Fixed");
						targetPage = pageUseType2FixedView;
					}
					break;

				case cfg.Enum_UseType.JumpUse:  // 3
					targetPage = pageUseType3View;
					Debug.Log($"BagPanel: UseType=JumpUse，选择 PageUseType3View ({(targetPage != null ? "存在" : "为null")})");
					break;

				default:
					Debug.LogWarning($"BagPanel: 未知的 UseType {itemData.UseType}，显示 Empty");
					targetPage = pageEmptyView;
					break;
			}

			if (targetPage == null)
			{
				Debug.LogWarning($"BagPanel: 目标 Page 为 null，使用 PageEmptyView");
				targetPage = pageEmptyView;
			}

			SwitchToPage(targetPage, itemData);
		}

		/// <summary>
		/// 获取玩家建筑大厅等级
		/// TODO: 根据项目实际情况从 Model 或 System 获取
		/// </summary>
		public int GetPlayerHallLevel()
		{
			// 暂时返回固定值，后续需要从实际的 Model 或 System 获取
			// 例如：return this.GetModel<IBuildingModel>().GetHallLevel();
			return 1;  // 临时值，需要根据项目实际情况实现（统一返回1，避免与PageUseTypeXView不一致）
		}

		// 实现IController接口
		public IArchitecture GetArchitecture()
		{
			return GameApp.Interface;
		}
	}
}
