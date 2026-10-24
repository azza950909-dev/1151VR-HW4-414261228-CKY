using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟隨目標")]
    public Transform target; // 拖入你的無人機 (Drone)

    [Header("攝影機位置設定")]
    [Tooltip("相對於無人機的偏移量 (建議放在無人機後上方，例如 X:0, Y:3, Z:-6)")]
    public Vector3 offset = new Vector3(0f, 3f, -6f);

    [Tooltip("跟隨的平滑移動速度")]
    public float followSpeed = 10f;

    void FixedUpdate()
    {
        if (target == null) return;

       // 【關鍵修改】只提取無人機的水平轉向 (Y軸 Yaw)，把 X 和 Z 的前傾、側傾歸零
        Quaternion yawRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
        
        // 根據水平轉向計算出相機應該在的位置
        Vector3 targetPosition = target.position + yawRotation * offset;

        // 平滑移動攝影機位置
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);

        // 讓攝影機永遠盯著無人機前方的一點（保持畫面穩定）
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}