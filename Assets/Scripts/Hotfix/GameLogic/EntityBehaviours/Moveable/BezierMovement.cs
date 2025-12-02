//using QFramework.Game;
//using System.Collections.Generic;
//using UnityEngine;
///// <summary>
///// 贝塞尔曲线移动行为
///// </summary>
//public class BezierMovement : BaseMovement
//{
//    private Vector3 start;
//    private Vector3 mid;
//    [Header("Movement Settings")]
//    public float duration = 0.8f;// 飞行总时长
//    public float gravity = 9.8f;// 重力强度
//    public bool useGravity = true;// 是否启用重力

//    private List<Vector3> pathPoints;
//    private float elapsedTime;
//    private float totalLength;
//    private float[] segmentLengths;

//    #region 重写父类方法
//    public override void Update()
//    {
//        if (!CanMove) return;
//        if (pathPoints == null || pathPoints.Count < 2) return;
//        if (targetObject == null)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        elapsedTime += Time.deltaTime;

//        // 匀速沿曲线移动
//        float traveled = Mathf.Min(elapsedTime * Speed, totalLength);
//        Vector3 pos = GetPointOnPathByLength(traveled);

//        if (useGravity)
//            pos.y -= 0.5f * gravity * elapsedTime * elapsedTime;

//        selfObject.transform.position = pos;
//        float dis = Vector3.Distance(selfObject.transform.position, targetObject.transform.position);
//        if (dis < 0.5f)
//        {
//            Debug.Log($"打中{targetObject.name}了");

//            if (targetObject)
//            {
//                if (targetObject.GetComponent<BuildEntity>())
//                {
//                    targetObject.GetComponent<BuildEntity>().BeHurt((int)CastDamage);
//                }else if (targetObject.GetComponent<HeroEntity>())
//                {
//                    targetObject.GetComponent<HeroEntity>().BeHurt((int)CastDamage);
//                }
//                else
//                {
//                    Debug.LogWarning("目标对象没有BuildEntity或HeroEntity组件，无法造成伤害");
//                }
//                Destroy(this);
//                Destroy(gameObject);
//            }
//        }

//        //恢复旧版正确的朝向逻辑
//        float t = traveled / totalLength;
//        if (t < 1f)
//        {
//            Vector3 nextPos = GetPointOnPathByRatio(Mathf.Clamp01(t + 0.01f));
//            if (useGravity)
//                nextPos.y -= 0.5f * gravity * (elapsedTime + 0.01f) * (elapsedTime + 0.01f);

//            Vector3 dir = (nextPos - pos).normalized;
//            selfObject.transform.rotation = Quaternion.LookRotation(dir);
//        }
//    }
//    public override void SetCanFly()
//    {
//        CanMove = true;
//        elapsedTime = 0f;
//    }

//    public override void SetDamage(float _damage)
//    {
//        base.SetDamage(_damage);// 这里调用基类方法，直接设置为表中的值
//        //CastDamage = 10;// 这里是临时测试伤害值
//    }
//    #endregion

//    #region 贝塞尔移动具体实现
//    public void SetBezier()
//    {
//        start = selfObject.transform.position;
//        pathPoints = BezierCurve.GetBezierPoints(start, GetDefaultControlPoint(start, targetObject.transform.position + new Vector3(0, 2, 0)), targetObject.transform.position + new Vector3(0, 2, 0));
//        PrecomputeSegmentLengths();

//        Speed = totalLength / duration;
//    }
//    Vector3 GetDefaultControlPoint(Vector3 start, Vector3 end, float heightOffset = 6f)
//    {
//        Vector3 mid = (start + end) / 2f;
//        mid += Vector3.up * heightOffset;
//        return mid;
//    }

//    void PrecomputeSegmentLengths()
//    {
//        segmentLengths = new float[pathPoints.Count - 1];
//        totalLength = 0f;

//        for (int i = 0; i < pathPoints.Count - 1; i++)
//        {
//            float len = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
//            segmentLengths[i] = len;
//            totalLength += len;
//        }
//    }

//    Vector3 GetPointOnPathByLength(float targetLength)
//    {
//        float accumulated = 0f;

//        for (int i = 0; i < segmentLengths.Length; i++)
//        {
//            if (accumulated + segmentLengths[i] >= targetLength)
//            {
//                float remain = targetLength - accumulated;
//                float segRatio = remain / segmentLengths[i];
//                return Vector3.Lerp(pathPoints[i], pathPoints[i + 1], segRatio);
//            }
//            accumulated += segmentLengths[i];
//        }
//        return pathPoints[pathPoints.Count - 1];
//    }

//    Vector3 GetPointOnPathByRatio(float ratio)
//    {
//        float targetLength = ratio * totalLength;
//        float accumulated = 0f;

//        for (int i = 0; i < segmentLengths.Length; i++)
//        {
//            if (accumulated + segmentLengths[i] >= targetLength)
//            {
//                float remain = targetLength - accumulated;
//                float segRatio = remain / segmentLengths[i];
//                return Vector3.Lerp(pathPoints[i], pathPoints[i + 1], segRatio);
//            }
//            accumulated += segmentLengths[i];
//        }
//        return pathPoints[pathPoints.Count - 1];
//    }
//    #endregion
//}