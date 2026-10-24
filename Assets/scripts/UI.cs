using UnityEngine;
using TMPro; // 使用 TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI 物件")]
    public GameObject winPanel; // 把剛剛的 WinText 拖進來

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
        // 遊戲剛開始時，確保獲勝面板是關閉的
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    // 顯示獲勝畫面的公開方法
    public void ShowWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
}