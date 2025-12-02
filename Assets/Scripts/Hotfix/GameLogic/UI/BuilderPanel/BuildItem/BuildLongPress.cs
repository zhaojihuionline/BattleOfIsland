using System;
using QFramework;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildLongPress : MonoBehaviour, IController
{
    [Header("拖拽 Input Action Asset 到这里")]
    public InputActionAsset inputActions;
    private LayerMask layerMask; // 设置为 Build 层级
    private InputAction longPressAction;
    private Camera mainCamera;
    private Collider objectCollider;

    private void Awake()
    {
        mainCamera = Camera.main;
        objectCollider = transform.Find("Model").GetComponentInChildren<Collider>(); // 获取物体的碰撞体
        layerMask = 1 << LayerMask.NameToLayer("Build");
        if (inputActions != null)
        {
            longPressAction = inputActions.FindAction("LongPress");

            if (longPressAction == null)
            {
                Debug.LogError("在 Input Action Asset 中找不到名为 'LongPress' 的 Action！");
            }
        }
        else
        {
            Debug.LogError("请将 Input Action Asset 拖拽到脚本的 inputActions 字段！");
        }

        // 确保物体在 Build 层级
        if (gameObject.layer != LayerMask.NameToLayer("Build"))
        {
            Debug.LogWarning($"物体 {gameObject.name} 不在 Build 层级，长按检测可能无效！");
        }
    }

    private void OnEnable()
    {
        if (longPressAction != null)
        {
            longPressAction.performed += OnLongPressPerformed;
            longPressAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (longPressAction != null)
        {
            longPressAction.performed -= OnLongPressPerformed;
            longPressAction.Disable();
        }
    }

    private void OnLongPressPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("长按一秒 触发");
        if (objectCollider == null)
        {
            Debug.LogWarning($"物体 {gameObject.name} 没有碰撞体，无法检测点击！");
            return;
        }

        Vector2 screenPosition = GetInputPosition(context);

        // 只检测 Build 层级，并且精确比较碰撞体
        if (IsClickingThisBuildObject(screenPosition))
        {
            Debug.Log($"🎯 长按成功！点击了建筑: {gameObject.name}");
            this.SendCommand(new SelectBuildingCommand(gameObject, false));
        }
    }

    private bool IsClickingThisBuildObject(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // 调试信息：显示当前的层级设置
        int buildLayer = LayerMask.NameToLayer("Build");
        Debug.Log($"🔍 调试信息:");
        Debug.Log($"  - Build层级索引: {buildLayer}");
        Debug.Log($"  - 当前物体层级: {gameObject.layer}");
        Debug.Log($"  - LayerMask值: {layerMask} (二进制: {Convert.ToString(layerMask, 2)})");
        Debug.Log($"  - 射线位置: {screenPosition}");

        // 方法1：先检测所有层级，看看是否能命中任何物体
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.Log($"✅ 所有层级检测到: '{hit.collider.gameObject.name}' (层级索引: {hit.collider.gameObject.layer})");
            Debug.Log($"   层级名称: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
        }
        else
        {
            Debug.Log("❌ 所有层级也没有检测到任何物体 - 可能是射线方向问题");
        }

        // 方法2：检测Build层级
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log($"🎯 Build层级检测到: '{hit.collider.gameObject.name}'");
            return hit.collider == objectCollider;
        }
        else
        {
            Debug.Log("❌ Build层级没有检测到任何物体");
            return false;
        }
    }

    private Vector2 GetInputPosition(InputAction.CallbackContext context)
    {
        return context.control.device switch
        {
            Mouse mouse => mouse.position.ReadValue(),
            Touchscreen touchscreen => touchscreen.primaryTouch.position.ReadValue(),
            _ => Vector2.zero
        };
    }

    public IArchitecture GetArchitecture()
    {
        return GridTrackerApp.Interface;
    }
}
