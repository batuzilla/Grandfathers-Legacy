using System.Collections;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public WaveSpawner spawner;
    public GameObject gate;
    public float closeDuration = 1f;
    public bool reopenAfterWave = false;

    Vector3 fullScale;

    void Awake()
    {
        if (gate == null) return;
        fullScale = gate.transform.localScale;
        gate.SetActive(false);
    }

    void OnEnable()
    {
        if (spawner == null) return;
        spawner.OnWaveStarted += Close;
        spawner.OnWaveEnded += OnWaveEnded;
    }

    void OnDisable()
    {
        if (spawner == null) return;
        spawner.OnWaveStarted -= Close;
        spawner.OnWaveEnded -= OnWaveEnded;
    }

    void OnWaveEnded()
    {
        if (reopenAfterWave && gate != null) gate.SetActive(false);
    }

    void Close()
    {
        if (gate == null) return;
        StopAllCoroutines();
        gate.SetActive(true);
        IgnorePlayer();

        var anim = gate.GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Close");
        else StartCoroutine(Grow());
    }

    void IgnorePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        var playerCols = player.GetComponentsInChildren<Collider2D>();
        var gateCols = gate.GetComponentsInChildren<Collider2D>();
        foreach (var g in gateCols)
            foreach (var p in playerCols)
                Physics2D.IgnoreCollision(g, p, true);
    }

    IEnumerator Grow()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / closeDuration;
            float k = Mathf.Clamp01(t);
            gate.transform.localScale = new Vector3(fullScale.x, Mathf.Lerp(0.1f, fullScale.y, k), fullScale.z);
            yield return null;
        }
        gate.transform.localScale = fullScale;
    }
}