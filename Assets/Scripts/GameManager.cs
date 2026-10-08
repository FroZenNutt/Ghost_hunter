using System;
using UnityEngine;

public enum GamePhase { Start, Play, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    public GameConfig config;
    public GamePhase phase = GamePhase.Start;
    public float timer;
    public int kills;
    public int bestCombo;
    public float slowmoTimer;
    public bool inPanic;

    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnKill;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void Update()
    {
        if (phase != GamePhase.Play) return;
        if (slowmoTimer > 0f)
        {
            slowmoTimer = Mathf.Max(0f, slowmoTimer - Time.unscaledDeltaTime);
            if (slowmoTimer <= 0f) Time.timeScale = 1f;
        }
        timer -= Time.deltaTime;
        inPanic = timer <= config.panicThreshold;
        if (timer <= 0f) { timer = 0f; EndGame(); }
    }

    public void StartGame()
    {
        phase = GamePhase.Play;
        timer = config.startTime;
        kills = 0;
        bestCombo = 0;
        slowmoTimer = 0f;
        inPanic = false;
        Time.timeScale = 1f;
        OnGameStart?.Invoke();
    }

    public void EndGame()
    {
        if (phase == GamePhase.GameOver) return;
        phase = GamePhase.GameOver;
        Time.timeScale = 1f;
        OnGameOver?.Invoke();
    }

    public void RegisterKill(int comboThisDash)
    {
        kills++;
        bestCombo = Mathf.Max(bestCombo, comboThisDash);
        GameObject player = GameObject.FindWithTag("Player");
        if (kills % config.killMilestone == 0)
        {
            timer += config.rewardTime;
            if (player != null) GameFeel.I?.Milestone(player.transform.position, kills);
        }
        if (kills % config.superMilestone == 0)
        {
            timer += config.superRewardTime;
            if (player != null) GameFeel.I?.Super(player.transform.position);
        }
        OnKill?.Invoke();
    }

    public void RegisterMiss()
    {
        timer = Mathf.Max(0f, timer - config.penaltyTime);
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) GameFeel.I?.Miss(player.transform.position);
    }

    public void TriggerCombo()
    {
        slowmoTimer = config.slowmoDuration;
        Time.timeScale = config.slowmoFactor;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) GameFeel.I?.Combo(player.transform.position);
    }
}
