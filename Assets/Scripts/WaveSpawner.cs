using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public Enemy enemyPrefab;
    public Transform target;
    public Transform[] spawnPoints;
    public int enemyCount = 12;
    public float spawnInterval = 1.5f;
    public float startDelay = 3f;

    public System.Action OnWaveStarted;
    public System.Action OnWaveEnded;
    int alive;

    void Start()
    {
        StartCoroutine(RunWave());
    }

    IEnumerator RunWave()
    {
        yield return new WaitForSeconds(startDelay);
        OnWaveStarted?.Invoke();

        for (int i = 0; i < enemyCount; i++)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Enemy e = Instantiate(enemyPrefab, p.position, Quaternion.identity);
            e.castleTarget = target;
            alive++;
            e.GetComponent<Health>().OnDied += () => alive--;
            yield return new WaitForSeconds(spawnInterval);
        }

        while (alive > 0) yield return null;
        Debug.Log("Dalga 1 bitti!");
        OnWaveEnded?.Invoke();
    }
}