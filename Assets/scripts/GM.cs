using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("玩家設定")]
    [Tooltip("請把你的無人機 (Drone) 拖曳到這裡")]
    public Transform playerTransform;

    [Header("重生點設定")]
    private Vector3 currentCheckpointPos; // 記錄目前的重生位置

    private bool isGameEnded = false; // 追蹤遊戲是否已結束（過關）
    private bool isInvulnerable = false; // 無敵狀態開關（防止連續碰撞）

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        isGameEnded = false;
        isInvulnerable = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (playerTransform != null)
        {
            currentCheckpointPos = playerTransform.position;
        }
    }

    // 1. 撞到障礙物時觸發（直接傳送回上一個儲存點，不扣血、不失敗）
    public void PlayerHitObstacle()
    {
        if (isGameEnded || isInvulnerable) return;

        Debug.Log("哎呀撞到了！傳送回上一個儲存點。");
        StartCoroutine(RespawnCooldownRoutine());
    }

    // 2. 碰到中繼儲存點 (Checkpoint) 時觸發
    public void ReachCheckpoint(Vector3 newCheckpointPos)
    {
        if (isGameEnded) return;

        currentCheckpointPos = newCheckpointPos;
        Debug.Log("已更新儲存點位置！");
    }

    // 3. 抵達終點時觸發（獲勝）
    public void ReachFinishLine()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        Debug.Log("🎉 恭喜通關！");

        // 呼叫 UI 顯示獲勝畫面
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinScreen();
        }
        
       // 啟動過關協程，確保音效能完整播完再暫停遊戲
        StartCoroutine(WinRoutine());
    }

    IEnumerator WinRoutine()
    {
        // 尋找無人機身上的 PlayerCollision 來直接播放過關音效（確保不會被瞬間切斷）
        if (playerTransform != null)
        {
            PlayerCollision collisionScript = playerTransform.GetComponent<PlayerCollision>();
            if (collisionScript != null)
            {
                collisionScript.PlayFinishSoundExplicitly(); // 呼叫專門播過關音效的方法
            }
        }

        // 讓遊戲繼續跑 1 秒鐘（真實時間），讓過關音效順利播完
        yield return new WaitForSecondsRealtime(2.0f);

        // 之後再將物理與全域聲音暫停
        Time.timeScale = 0f;       
        AudioListener.pause = true;  
    }

    // 執行重生傳送，並給予短暫無敵時間避免連撞
    IEnumerator RespawnCooldownRoutine()
    {
        isInvulnerable = true;

        if (playerTransform != null)
        {
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            playerTransform.position = currentCheckpointPos;
        }

        // 等待 0.5 秒真實時間，避免短時間內重複觸發
        yield return new WaitForSecondsRealtime(0.5f);

        isInvulnerable = false;
    }
   
}