using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public Enemy enemyPrefab;
    public Transform target;
    public Transform[] spawnPoints;

    [Header("Dalga ayarları")]
    public int enemyCount = 8;
    public int countIncrease = 3;
    public float spawnInterval = 1.5f;
    public float startDelay = 3f;
    public float breakTime = 25f;
    public int totalWaves = 0;
    public bool autoStart = false;

    [Header("Boss")]
    public Enemy bossPrefab;
    public int bossAfterWave = 2;
    public int bossEscorts = 6;
    public DialogueUI dialogue;
    public Sprite bossFace;
    public Sprite playerFace;
    public string bossName = "Dead Eye";
    public string playerName = "Ben";
    public string[] bossLines =
    {
        "Küçük kale sahibi... Gözüm hep bu duvarlarda.",
        "Ayılarım seni yordu, şimdi sıra bende. Evi yerle bir edeceğim!",
        "Bakalım! Dedemin kalesi sana kalmayacak."
    };

    public System.Action OnWaveStarted;
    public System.Action OnWaveEnded;
    public System.Action OnAllWavesDone;

    public int WaveNumber { get; private set; }
    public bool WaveActive { get; private set; }
    public bool Started { get; private set; }
    public bool BossWave { get; private set; }
    public float BreakTimeLeft { get; private set; }
    public int Alive { get { return alive; } }

    int alive;
    bool bossDown;

    void Start()
    {
        if (autoStart) BeginWave();
    }

    public void BeginWave()
    {
        if (Started) return;
        Started = true;
        StartCoroutine(Run());
    }

    IEnumerator Countdown(float seconds)
    {
        BreakTimeLeft = seconds;
        while (BreakTimeLeft > 0f)
        {
            BreakTimeLeft -= Time.deltaTime;
            yield return null;
        }
        BreakTimeLeft = 0f;
    }

    IEnumerator Run()
    {
        yield return Countdown(startDelay);

        while (true)
        {
            WaveNumber++;
            BossWave = bossPrefab != null && WaveNumber == bossAfterWave + 1;
            int regular = bossPrefab != null && WaveNumber > bossAfterWave + 1 ? WaveNumber - 1 : WaveNumber;
            int count = BossWave ? bossEscorts : enemyCount + (regular - 1) * countIncrease;
            float interval = Mathf.Max(0.6f, spawnInterval - 0.1f * (regular - 1));

            if (BossWave) yield return BossIntro();

            WaveActive = true;
            OnWaveStarted?.Invoke();

            if (BossWave) SpawnOne(bossPrefab);

            for (int i = 0; i < count; i++)
            {
                if (bossDown) break;
                SpawnOne(enemyPrefab);
                yield return new WaitForSeconds(interval);
            }

            while (alive > 0 && !bossDown) yield return null;

            if (bossDown)
            {
                WaveActive = false;
                BossWave = false;
                OnWaveEnded?.Invoke();
                OnAllWavesDone?.Invoke();
                yield break;
            }

            WaveActive = false;
            BossWave = false;
            OnWaveEnded?.Invoke();

            if (totalWaves > 0 && WaveNumber >= totalWaves)
            {
                OnAllWavesDone?.Invoke();
                yield break;
            }

            yield return Countdown(breakTime);
        }
    }

    IEnumerator BossIntro()
    {
        if (dialogue == null || bossLines == null || bossLines.Length == 0) yield break;

        var controller = FindFirstObjectByType<PlayerController>();
        var shooter = FindFirstObjectByType<BowShooter>();
        if (controller != null) controller.enabled = false;
        if (shooter != null) shooter.enabled = false;

        for (int i = 0; i < bossLines.Length; i++)
        {
            bool player = i == bossLines.Length - 1;
            yield return dialogue.ShowLine(player ? playerName : bossName, player ? playerFace : bossFace, bossLines[i]);
        }
        dialogue.Hide();

        if (shooter != null) shooter.enabled = true;
        if (controller != null) controller.enabled = true;
    }

    void SpawnOne(Enemy prefab)
    {
        Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Enemy e = Instantiate(prefab, p.position, Quaternion.identity);
        e.castleTarget = target;
        alive++;

        var hp = e.GetComponent<Health>();
        float last = hp.Current;
        hp.OnChanged += (c, m) =>
        {
            if (c < last) GameAudio.EnemyHit();
            last = c;
        };
        hp.OnDied += () =>
        {
            alive--;
            if (prefab == bossPrefab) bossDown = true;
            GameAudio.EnemyDie();
        };
    }
}
