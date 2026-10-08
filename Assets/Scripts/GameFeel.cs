using TMPro;
using UnityEngine;

public class GameFeel : MonoBehaviour
{
    public static GameFeel I { get; private set; }
    public GameConfig config;
    Material particleMaterial;
    AudioSource audioSource;

    void Awake()
    {
        I = this;
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
        particleMaterial = new Material(shader);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        if (Camera.main != null && Camera.main.GetComponent<CameraShake>() == null) Camera.main.gameObject.AddComponent<CameraShake>();
    }

    public void Dash(Vector3 position, Vector3 direction)
    {
        Burst(position, config.playerCyan, config.dashParticleCount, config.dashParticleLifetime, direction * -2f);
        Tone(config.dashToneHz, .09f, config.dashVolume);
    }
    public void Kill(Vector3 position)
    {
        Burst(position, config.ghostPink, config.killParticleCount, config.killParticleLifetime, Vector3.zero);
        Tone(config.killToneHz, .12f, config.killVolume);
    }
    public void Miss(Vector3 position)
    {
        Burst(position, config.warningRed, 10, .25f, Vector3.zero);
        Camera.main?.GetComponent<CameraShake>()?.Kick(config.missCameraShake);
        Popup(position, "-" + config.penaltyTime + " SEC", config.warningRed, 1f, 3.4f);
        Tone(config.missToneHz, .18f, config.missVolume);
    }
    public void Combo(Vector3 position)
    {
        Burst(position, config.playerCyan, config.comboParticleCount, config.comboParticleLifetime, Vector3.zero);
        Popup(position, "DASH READY", config.playerCyan, 1.15f, 3.2f);
        Tone(config.comboToneHz, .20f, config.comboVolume);
    }
    public void Milestone(Vector3 position, int kills) => Popup(position, "+" + config.rewardTime + " SEC  •  " + kills + " KILLS", config.playerCyan, 1.35f, 2.4f);
    public void Super(Vector3 position)
    {
        Burst(position, config.playerCyan, config.superParticleCount, 1.5f, Vector3.zero);
        Popup(position, "+" + config.superRewardTime + " SEC", config.playerCyan, 2.1f, 5.4f);
        Tone(1040f, .38f, .17f);
    }

    void Burst(Vector3 position, Color color, int count, float lifetime, Vector3 velocityBias)
    {
        GameObject go = new GameObject("Arcade Feedback");
        go.transform.position = position + Vector3.up * .2f;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main; main.duration = .1f; main.startLifetime = lifetime; main.startSpeed = 2.8f; main.startSize = .13f; main.startColor = color; main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
        var velocity = ps.velocityOverLifetime; velocity.enabled = velocityBias.sqrMagnitude > .01f; velocity.x = velocityBias.x; velocity.z = velocityBias.z;
        go.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
        Destroy(go, lifetime + .4f);
    }

    void Popup(Vector3 position, string value, Color color, float life, float size)
    {
        GameObject go = new GameObject(value);
        go.transform.SetPositionAndRotation(position + Vector3.up * .35f, Quaternion.Euler(90f, 0f, 0f));
        TextMeshPro text = go.AddComponent<TextMeshPro>();
        text.font = TMP_Settings.defaultFontAsset; text.text = value; text.color = color; text.fontSize = size; text.alignment = TextAlignmentOptions.Center;
        go.AddComponent<PopupMotion>().Initialize(life);
    }

    void Tone(float frequency, float duration, float volume)
    {
        const int rate = 44100;
        int samples = Mathf.CeilToInt(rate * duration);
        AudioClip clip = AudioClip.Create("ArcadeTone", samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++) data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * Mathf.Exp(-5f * i / (float)samples);
        clip.SetData(data, 0); audioSource.PlayOneShot(clip, volume); Destroy(clip, duration + .1f);
    }
}

public class PopupMotion : MonoBehaviour
{
    float life, age;
    public void Initialize(float value) => life = value;
    void Update()
    {
        age += Time.unscaledDeltaTime;
        transform.position += Vector3.forward * Time.deltaTime * GameFeel.I.config.popupRiseSpeed;
        TextMeshPro text = GetComponent<TextMeshPro>(); Color c = text.color; c.a = Mathf.Clamp01(1f - age / life); text.color = c;
        if (age >= life) Destroy(gameObject);
    }
}

public class CameraShake : MonoBehaviour
{
    Vector3 rest;
    float trauma;
    void Awake() => rest = transform.localPosition;
    public void Kick(float amount) => trauma = Mathf.Max(trauma, amount);
    void LateUpdate()
    {
        trauma = Mathf.MoveTowards(trauma, 0, Time.unscaledDeltaTime * .28f);
        transform.localPosition = rest + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * trauma;
    }
}
