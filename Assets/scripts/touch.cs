using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("音效元件")]
    [Tooltip("請把無人機身上的 AudioSource 拖進來")]
    public AudioSource sfxAudioSource;

    [Header("音效檔案 (Audio Clips)")]
    public AudioClip hitObstacleClip;   // 撞到障礙物音效
    public AudioClip checkpointClip;    // 經過檢查點音效
    public AudioClip finishClip;        // 抵達終點音效

    void Start()
    {
        // 如果沒有手動指定，自動抓取自己身上的 AudioSource
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
        }
    }
    // 處理「實體碰撞」（例如撞到會動的障礙物或隱形牆）
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            PlaySound(hitObstacleClip);
            // 呼叫 GameManager 執行扣血與重生
            GameManager.Instance.PlayerHitObstacle();
        }
    }

    // 處理「觸發器穿透」（例如穿過 Checkpoint 或 Finish）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            PlaySound(checkpointClip);
            // 將這個 Checkpoint 的位置傳給 GameManager 儲存
            GameManager.Instance.ReachCheckpoint(other.transform.position);
            
            // 讓吃到的 Checkpoint 暫時消失或關閉，避免重複觸發
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Finish"))
        {
            PlaySound(finishClip);
            // 呼叫 GameManager 觸發過關
            GameManager.Instance.ReachFinishLine();
        }
    }
    void PlaySound(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }
     public void PlayFinishSoundExplicitly()
    {
        PlaySound(finishClip);
    }
}