using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject startScreen, gameOverScreen, hudPanel;
    public Button playButton, restartButton;
    public TMP_Text finalScoreText;
    void Start()
    {
        playButton.onClick.AddListener(OnPlay);
        restartButton.onClick.AddListener(OnPlay);
        GameManager.I.OnGameOver += ShowGameOver;
        ShowStart();
    }
    void OnDestroy() { if (GameManager.I != null) GameManager.I.OnGameOver -= ShowGameOver; }
    void OnPlay() { startScreen.SetActive(false); gameOverScreen.SetActive(false); hudPanel.SetActive(true); GameManager.I.StartGame(); }
    void ShowStart() { startScreen.SetActive(true); gameOverScreen.SetActive(false); hudPanel.SetActive(false); }
    void ShowGameOver() { hudPanel.SetActive(false); finalScoreText.text = "KILLS: " + GameManager.I.kills; gameOverScreen.SetActive(true); }
}
