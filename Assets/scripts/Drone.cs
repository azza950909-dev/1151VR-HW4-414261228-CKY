using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class DroneController : MonoBehaviour
{
    [Header("移動速度設定")]
    [Tooltip("水平移動速度")]
    public float moveSpeed = 10f;
    [Tooltip("垂直升降速度")]
    public float verticalSpeed = 8f;
    [Tooltip("旋轉轉向速度")]
    public float turnSpeed = 100f;

    [Header("飛行傾斜視覺效果")]
    [Tooltip("前後移動時的傾斜角度")]
    public float maxPitchAngle = 20f;
    [Tooltip("左右移動時的傾斜角度")]
    public float maxRollAngle = 20f;
    [Tooltip("傾斜平滑速度")]
    public float tiltSmoothTime = 5f;

    [Header("引擎音效設定")]
    [Tooltip("靜止時的引擎底噪音量")]
    public float minVolume = 0.2f;
    [Tooltip("移動時的最大引擎音量")]
    public float maxVolume = 1.0f;

    [Header("UI 提示設定")]
    [Tooltip("把左下角的 ControlTipText 拖進來")]
    public GameObject controlTipUI;

    private bool hasMoved = false;

    private Rigidbody rb;
    private AudioSource audioSource;

    // 輸入變數
    private float moveInputX; // 左右平移 (A/D 或 Left/Right)
    private float moveInputZ; // 前後移動 (W/S 或 Up/Down)
    private float moveInputY; // 上下升降 (Space / LeftShift)
    private float rotateInput; // 左右轉向 (Q/E)

    // 當前傾斜角度
    private float currentPitch = 0f;
    private float currentRoll = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 凍結 X 和 Z 軸的物理旋轉，改由程式碼手動控制傾斜與轉向，防止撞擊後甩飛倒地
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = false; // 飛行器預設不套用重力（或可依需求設定低重力）

        if (audioSource.clip != null)
        {
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void Update()
    {
        // 偵測玩家是否有按下移動鍵 (W, A, S, D, 方向鍵, 空白鍵, 左Ctrl 等)
        float moveH = Input.GetAxis("Horizontal"); // A/D 或 左右方向鍵
        float moveV = Input.GetAxis("Vertical");   // W/S 或 上下方向鍵
        bool jumpOrFall = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift);

        // 如果玩家有任何操作，且提示還沒關閉過
        if (!hasMoved && (Mathf.Abs(moveH) > 0.1f || Mathf.Abs(moveV) > 0.1f || jumpOrFall))
        {
            hasMoved = true;

            // 隱藏左下角的教學 UI
            if (controlTipUI != null)
            {
                controlTipUI.SetActive(false);
            }
        }

        // 1. 讀取玩家輸入
        moveInputX = Input.GetAxis("Horizontal"); // A, D 或 方向鍵左右
        moveInputZ = Input.GetAxis("Vertical");   // W, S 或 方向鍵上下

        // 上下升降 (Space 向上, Left Shift 向下)
        moveInputY = 0f;
        if (Input.GetKey(KeyCode.Space)) moveInputY += 1f;
        if (Input.GetKey(KeyCode.LeftShift)) moveInputY -= 1f;

        // 左右旋轉 (Q 向左轉, E 向右轉)
        rotateInput = 0f;
        if (Input.GetKey(KeyCode.Q)) rotateInput -= 1f;
        if (Input.GetKey(KeyCode.E)) rotateInput += 1f;

        UpdateEngineAudio();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotationAndTilt();
    }

    /// <summary>
    /// 處理位移與升降
    /// </summary>
    private void HandleMovement()
    {
        // 計算相對於飛行器自身朝向的方向向量
        Vector3 moveDirection = transform.forward * moveInputZ + transform.right * moveInputX;
        Vector3 velocity = moveDirection * moveSpeed;

        // 設定 Y 軸垂直升降速度
        velocity.y = moveInputY * verticalSpeed;

        // 應用速度到 Rigidbody
        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// 處理轉向與姿態傾斜
    /// </summary>
    private void HandleRotationAndTilt()
    {
        // 1. 原地水平 Y 軸旋轉 (Q / E)
        float yawDelta = rotateInput * turnSpeed * Time.fixedDeltaTime;
        Quaternion yawRotation = Quaternion.Euler(0f, transform.eulerAngles.y + yawDelta, 0f);

        // 2. 計算機身傾斜 (Pitch: 前後, Roll: 左右)
        float targetPitch = moveInputZ * maxPitchAngle;
        float targetRoll = -moveInputX * maxRollAngle; // 往右移時機身向右傾斜

        // 平滑過渡傾斜角度
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.fixedDeltaTime * tiltSmoothTime);
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.fixedDeltaTime * tiltSmoothTime);

        // 結合 Y 軸旋轉與 Pitch / Roll 姿態
        Quaternion targetRotation = yawRotation * Quaternion.Euler(currentPitch, 0f, currentRoll);
        
        rb.MoveRotation(targetRotation);
    }

    /// <summary>
    /// 動態調整引擎音效大小
    /// </summary>
    private void UpdateEngineAudio()
    {
        if (audioSource == null || audioSource.clip == null) return;

        // 如果玩家正在按任何移動或轉向鍵
        bool isMoving = Mathf.Abs(moveInputX) > 0.1f || 
                        Mathf.Abs(moveInputZ) > 0.1f || 
                        Mathf.Abs(moveInputY) > 0.1f || 
                        Mathf.Abs(rotateInput) > 0.1f;

        if (isMoving)
        {
            // 移動時聲音轉大
            audioSource.volume = Mathf.Lerp(audioSource.volume, maxVolume, Time.deltaTime * 5f);
        }
        else
        {
            // 靜止時聲音轉小（怠速聲）
            audioSource.volume = Mathf.Lerp(audioSource.volume, minVolume, Time.deltaTime * 5f);
        }
    }

}