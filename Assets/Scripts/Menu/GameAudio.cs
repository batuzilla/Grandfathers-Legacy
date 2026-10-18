using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance;

    public WaveSpawner spawner;
    public Health house;
    public Health player;

    [Header("Müzik")]
    public AudioClip peacefulMusic;
    public AudioClip fightMusic;
    public AudioClip victoryJingle;
    public AudioClip defeatJingle;

    [Header("Efektler")]
    public AudioClip shootSfx;
    public AudioClip enemyHitSfx;
    public AudioClip enemyDieSfx;
    public AudioClip houseHitSfx;
    public AudioClip playerHurtSfx;
    public AudioClip waveStartSfx;

    float houseLast;
    float playerLast;
    float houseSfxTime;
    float hurtSfxTime;

    void Reset()
    {
#if UNITY_EDITOR
        peacefulMusic = EditorClipFinder.Find("Music_Peaceful");
        fightMusic = EditorClipFinder.Find("Music_Fight");
        victoryJingle = EditorClipFinder.Find("Jingle_Victory");
        defeatJingle = EditorClipFinder.Find("Jingle_Defeat");
        shootSfx = EditorClipFinder.Find("Sfx_Shoot");
        enemyHitSfx = EditorClipFinder.Find("Sfx_EnemyHit");
        enemyDieSfx = EditorClipFinder.Find("Sfx_EnemyDie");
        houseHitSfx = EditorClipFinder.Find("Sfx_HouseHit");
        playerHurtSfx = EditorClipFinder.Find("Sfx_PlayerHurt");
        waveStartSfx = EditorClipFinder.Find("Sfx_WaveStart");
#endif
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (spawner == null) spawner = FindFirstObjectByType<WaveSpawner>();
        if (house == null)
        {
            var go = GameObject.Find("CastleTarget");
            if (go != null) house = go.GetComponent<Health>();
        }
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.GetComponent<Health>();
        }

        AudioManager.Instance.PlayMusic(peacefulMusic);

        if (spawner != null) spawner.OnWaveStarted += WaveStarted;
        if (house != null) { houseLast = house.Current; house.OnChanged += HouseChanged; }
        if (player != null) { playerLast = player.Current; player.OnChanged += PlayerChanged; }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (spawner != null) spawner.OnWaveStarted -= WaveStarted;
        if (house != null) house.OnChanged -= HouseChanged;
        if (player != null) player.OnChanged -= PlayerChanged;
    }

    void WaveStarted()
    {
        AudioManager.Play(waveStartSfx);
        AudioManager.Instance.PlayMusic(fightMusic);
    }

    void HouseChanged(float current, float max)
    {
        if (current < houseLast && Time.time - houseSfxTime > 0.2f)
        {
            houseSfxTime = Time.time;
            AudioManager.Play(houseHitSfx, 0.9f);
        }
        houseLast = current;
    }

    void PlayerChanged(float current, float max)
    {
        if (current < playerLast && Time.time - hurtSfxTime > 0.2f)
        {
            hurtSfxTime = Time.time;
            AudioManager.Play(playerHurtSfx);
        }
        playerLast = current;
    }

    public static void Shoot() { if (Instance != null) AudioManager.Play(Instance.shootSfx, 0.8f); }
    public static void EnemyHit() { if (Instance != null) AudioManager.Play(Instance.enemyHitSfx, 0.7f); }
    public static void EnemyDie() { if (Instance != null) AudioManager.Play(Instance.enemyDieSfx, 0.9f); }
    public static void Victory() { if (Instance != null) AudioManager.Instance.PlayJingle(Instance.victoryJingle); }
    public static void Defeat() { if (Instance != null) AudioManager.Instance.PlayJingle(Instance.defeatJingle); }
}
