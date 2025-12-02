using System;
using System.Collections;
using UnityEngine;
using QFramework;
using DG.Tweening;

public class BPathMove : MonoBehaviour, IController
{
    private Transform target;
    private Vector3 targetPoint;
    [Header("移动速度")]
    public float speed = 1f;
    [Header("抛物线高度  与目标高度的增量")]
    public float Hight = 3f;
    [Header("距离目标多远算已经到达")]
    public float distance = 0.5f;
    public Action OnMoveComplete;

    public void DOTweenMovePath(Vector3 v3)
    {
        targetPoint = v3;
        Sequence jumpTweener = null;
        jumpTweener = transform.DOJump(v3, Hight, 1, speed)
            .SetEase(Ease.Linear)
             .OnUpdate(() =>
            {
                if (Time.frameCount % 3 == 0 &&
                Vector3.Distance(transform.position, targetPoint) <= distance)
                {
                    jumpTweener.Complete();
                }
            })
            .OnComplete(() =>
            {
                OnMoveComplete?.Invoke();
                Destroy(gameObject);
            });
    }

    public void DOTweenMovePath(Transform tar)
    {
        target = tar;
        Vector3 previousPosition = transform.position;

        Sequence combinedSequence = DOTween.Sequence();

        // Y轴抛物线序列（保持不变）
        float hig = target.position.y;
        Sequence verticalSequence = DOTween.Sequence();
        verticalSequence.Append(transform.DOMoveY(hig + Hight, speed / 2f).SetEase(Ease.OutQuad));
        verticalSequence.Append(transform.DOMoveY(hig, speed / 2f).SetEase(Ease.InQuad));
        combinedSequence.Join(verticalSequence);

        // XZ轴移动：不加入Sequence，在OnUpdate中实时追踪
        float startTime = Time.time;
        Vector3 initialPos = transform.position;

        combinedSequence.OnUpdate(() =>
        {
            // 实时计算XZ轴位置（线性插值）
            float elapsedTime = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsedTime / speed);

            Vector3 currentXZPos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 targetXZPos = new Vector3(target.position.x, 0, target.position.z);
            Vector3 initialXZPos = new Vector3(initialPos.x, 0, initialPos.z);

            // XZ轴线性移动到目标位置
            Vector3 newXZPos = Vector3.Lerp(initialXZPos, targetXZPos, progress);
            transform.position = new Vector3(newXZPos.x, transform.position.y, newXZPos.z);

            // 计算移动方向并旋转
            Vector3 currentPosition = transform.position;
            Vector3 moveDirection = currentPosition - previousPosition;

            if (moveDirection.magnitude > 0.02f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.3f);
            }

            previousPosition = currentPosition;

            // 检查到达条件
            if (Time.frameCount % 3 == 0 &&
                Vector3.Distance(transform.position, target.position) <= distance)
            {
                combinedSequence.Complete();
            }
        })
        .OnComplete(() =>
        {
            OnMoveComplete?.Invoke();
            Destroy(gameObject);
        });
    }

    public IArchitecture GetArchitecture()
    {
        return GameApp.Interface;
    }
}