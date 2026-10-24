using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    public enum MovementType
    {
        Horizontal, // 水平移動 (X軸)
        Vertical    // 垂直移動 (Y軸)
    }

    [Header("移動方向與範圍")]
    [Tooltip("選擇移動方向：Horizontal (水平) 或 Vertical (垂直)")]
    public MovementType movementType = MovementType.Horizontal;

    [Tooltip("移動距離 (來回振幅)")]
    public float moveDistance = 5f;

    [Tooltip("移動速度")]
    public float speed = 2f;

    [Tooltip("時間偏移量 (如果設不同值，可以讓同一組的障礙物交錯移動)")]
    public float timeOffset = 0f;

    private Vector3 startPosition;

    void Start()
    {
        // 記錄障礙物放置在場景中的初始位置
        startPosition = transform.position;
    }

    void Update()
    {
        // 使用 Sin 函數計算滑順的來回運動 (數值在 -1 到 1 之間)
        float wave = Mathf.Sin((Time.time + timeOffset) * speed);

        // 根據選擇的運動類型套用位移
        if (movementType == MovementType.Horizontal)
        {
            // 在原 X 軸基礎上左右移動
            transform.position = startPosition + new Vector3(wave * moveDistance, 0f, 0f);
        }
        else if (movementType == MovementType.Vertical)
        {
            // 在原 Y 軸基礎上上下移動
            transform.position = startPosition + new Vector3(0f, wave * moveDistance, 0f);
        }
    }
}