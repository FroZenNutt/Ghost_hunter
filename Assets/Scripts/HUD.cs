using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public GameConfig config;
    public TMP_Text timerText, killsText, bestComboText;
    public Image dashCooldownBar, milestoneProgressBar;
    public Player player;
    void Update()
    {
        if (GameManager.I == null) return;
        timerText.text = Mathf.CeilToInt(GameManager.I.timer).ToString("00");
        bool panic = GameManager.I.inPanic;
        timerText.color = panic ? config.warningRed : (GameManager.I.timer <= config.startTime * .4f ? config.lowTimeYellow : config.playerCyan);
        timerText.transform.localScale = panic ? Vector3.one * (.93f + Mathf.Sin(Time.unscaledTime * 9f) * .07f) : Vector3.one;
        killsText.text = "× " + GameManager.I.kills;
        bestComboText.text = GameManager.I.bestCombo + "×";
        dashCooldownBar.fillAmount = player == null ? 1f : player.CooldownNormalized;
        milestoneProgressBar.fillAmount = (float)(GameManager.I.kills % config.killMilestone) / config.killMilestone;
    }
}
