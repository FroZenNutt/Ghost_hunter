using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Ghost Hunter/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Timer")]
    public float startTime = 30f;
    public float rewardTime = 1f;
    public float superRewardTime = 15f;
    public float penaltyTime = 3f;
    [Header("Milestones")]
    public int killMilestone = 5;
    public int superMilestone = 100;
    [Header("Player Dash")]
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.40f;
    public float dashEaseExponent = 4.5f;
    public float playerRadius = 0.7f;
    [Header("Combo")]
    public int comboThreshold = 3;
    public float slowmoDuration = 1f;
    public float slowmoFactor = 0.30f;
    [Header("Ghosts")]
    public int ghostFloor = 8;
    public int ghostCap = 15;
    public int initialGhosts = 8;
    public float ghostBaseSpeed = 2.6f;
    public float ghostSpeedCap = 1.5f;
    public float ghostHomingPull = 1f;
    public float ghostRadius = 1.1f;
    public float ghostMinSpawnDistance = 6.5f;
    [Header("Spawning")]
    public float initialSpawnInterval = 2f;
    public float minSpawnInterval = 0.40f;
    public float spawnRampDuration = 20f;
    [Header("Panic Mode")]
    public float panicThreshold = 10f;
    public float panicSpeedMultiplier = 1.6f;
    [Header("Arena")]
    public Vector2 arenaSize = new Vector2(40f, 22f);

    [Header("Dash Feel")]
    [Range(0f, 1f)] public float dashTrailTime = 0.20f;
    public float dashTrailStartWidth = 0.55f;
    public float dashTrailEndWidth = 0.02f;
    [Range(0f, 1f)] public float dashStretchAmount = 0.45f;
    [Range(0.1f, 2f)] public float dashSquashY = 0.82f;
    public float dashScaleRecovery = 12f;
    public float targetMarkerScale = 0.28f;

    [Header("Ghost Feel")]
    public float ghostSpawnPopSpeed = 6f;
    public float ghostWobbleSpeed = 2f;
    public float ghostWobbleHeight = 0.15f;

    [Header("Feedback")]
    public Color playerCyan = new Color(0.49f, 1f, 0.83f, 1f);
    public Color ghostPink = new Color(1f, 0.47f, 0.66f, 1f);
    public Color warningRed = new Color(1f, 0.35f, 0.40f, 1f);
    public Color lowTimeYellow = new Color(1f, 0.91f, 0.49f, 1f);
    public int dashParticleCount = 7;
    public int killParticleCount = 18;
    public int comboParticleCount = 34;
    public int superParticleCount = 60;
    public float dashParticleLifetime = 0.32f;
    public float killParticleLifetime = 0.55f;
    public float comboParticleLifetime = 0.75f;
    public float missCameraShake = 0.10f;
    public float popupRiseSpeed = 1f;

    [Header("Audio")]
    [Range(0f, 1f)] public float dashVolume = 0.10f;
    [Range(0f, 1f)] public float killVolume = 0.11f;
    [Range(0f, 1f)] public float comboVolume = 0.15f;
    [Range(0f, 1f)] public float missVolume = 0.12f;
    public float dashToneHz = 260f;
    public float killToneHz = 560f;
    public float comboToneHz = 840f;
    public float missToneHz = 180f;
}
