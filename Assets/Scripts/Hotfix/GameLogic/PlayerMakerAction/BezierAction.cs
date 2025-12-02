using HutongGames.PlayMaker;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BezierAction : FsmStateAction
{
    [HutongGames.PlayMaker.Tooltip("The GameObject to Look At.")]
    public FsmGameObject targetObject;
    private Vector3 start;
    public Vector3 mid;
    [HutongGames.PlayMaker.Tooltip("The GameObject to Look At.")]
    public FsmGameObject end;

    [Header("Movement Settings")]
    public float duration = 0.8f;    // 飞行总时长
    public float gravity = 9.8f;     // 重力强度
    public bool useGravity = true;   // 是否启用重力

    private List<Vector3> pathPoints;
    private float elapsedTime;
    private float totalLength;
    private float[] segmentLengths;
    private float speed;

    private bool CanFly = false;
    [HutongGames.PlayMaker.Tooltip("Repeat every frame.")]
    public bool everyFrame = true;
    public override void OnEnter()
    {
        SetBezier();
        SetCanFly();

        if (!everyFrame)
        {
            Finish();
        }
    }

    public override void OnUpdate()
    {
        if (!CanFly) return;
        if (pathPoints == null || pathPoints.Count < 2) return;

        //Debug.Log("update....");
        elapsedTime += Time.deltaTime;

        // 匀速沿曲线移动
        float traveled = Mathf.Min(elapsedTime * speed, totalLength);
        Vector3 pos = GetPointOnPathByLength(traveled);

        if (useGravity)
            pos.y -= 0.5f * gravity * elapsedTime * elapsedTime;

        targetObject.Value.transform.position = pos;

        //恢复旧版正确的朝向逻辑
        float t = traveled / totalLength;
        if (t < 1f)
        {
            Vector3 nextPos = GetPointOnPathByRatio(Mathf.Clamp01(t + 0.01f));
            if (useGravity)
                nextPos.y -= 0.5f * gravity * (elapsedTime + 0.01f) * (elapsedTime + 0.01f);

            Vector3 dir = (nextPos - pos).normalized;
            targetObject.Value.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
    public override void OnExit()
    {
        base.OnExit();
    }

    public void SetBezier()
    {
        start = targetObject.Value.transform.position;
        pathPoints = BezierCurve.GetBezierPoints(start, GetDefaultControlPoint(start, end.Value.transform.position + new Vector3(0, 2, 0)), end.Value.transform.position + new Vector3(0, 2, 0));
        PrecomputeSegmentLengths();

        speed = totalLength / duration;
    }

    public void SetCanFly()
    {
        CanFly = true;
        elapsedTime = 0f;
    }

    Vector3 GetDefaultControlPoint(Vector3 start, Vector3 end, float heightOffset = 6f)
    {
        Vector3 mid = (start + end) / 2f;
        mid += Vector3.up * heightOffset;
        return mid;
    }

    private void PrecomputeSegmentLengths()
    {
        segmentLengths = new float[pathPoints.Count - 1];
        totalLength = 0f;

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            float len = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            segmentLengths[i] = len;
            totalLength += len;
        }
    }

    private Vector3 GetPointOnPathByLength(float targetLength)
    {
        float accumulated = 0f;

        for (int i = 0; i < segmentLengths.Length; i++)
        {
            if (accumulated + segmentLengths[i] >= targetLength)
            {
                float remain = targetLength - accumulated;
                float segRatio = remain / segmentLengths[i];
                return Vector3.Lerp(pathPoints[i], pathPoints[i + 1], segRatio);
            }
            accumulated += segmentLengths[i];
        }
        return pathPoints[pathPoints.Count - 1];
    }

    private Vector3 GetPointOnPathByRatio(float ratio)
    {
        float targetLength = ratio * totalLength;
        float accumulated = 0f;

        for (int i = 0; i < segmentLengths.Length; i++)
        {
            if (accumulated + segmentLengths[i] >= targetLength)
            {
                float remain = targetLength - accumulated;
                float segRatio = remain / segmentLengths[i];
                return Vector3.Lerp(pathPoints[i], pathPoints[i + 1], segRatio);
            }
            accumulated += segmentLengths[i];
        }
        return pathPoints[pathPoints.Count - 1];
    }
}
