using UnityEngine;

public class RightClickDragCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public bool smoothMove = true;
    public float smoothTime = 0.1f;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    Vector3 oldPosition;
    bool isReturning = false;

    void Start()
    {
        cam = Camera.main;
        targetPosition = transform.position;
        oldPosition = transform.position;
    }

    void Update()
    {
        // 触发回到初始位置
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isReturning = true;
            targetPosition = oldPosition;     // 关键：只设置目标位置，让正常 SmoothDamp 去处理
        }

        // 右键拖动（如果正在返回则禁止手动移动）
        if (!isReturning && Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 right = cam.transform.right;
            right.y = 0;
            right.Normalize();

            Vector3 forward = cam.transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 move = (-right * mouseX + -forward * mouseY) * moveSpeed;
            targetPosition += move * Time.deltaTime;
        }

        // 用一次 SmoothDamp 完成所有移动
        transform.position = smoothMove
            ? Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime)
            : targetPosition;

        // 完成回到原点
        if (isReturning && Vector3.Distance(transform.position, oldPosition) < 0.05f)
        {
            isReturning = false;
        }
    }
}
