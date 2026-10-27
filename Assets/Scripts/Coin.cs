using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    public static int Total { get; private set; }
    public static event Action<int> OnTotalChanged;

    public int value = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        Total = 0;
        OnTotalChanged = null;
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            Total = 0;
            OnTotalChanged?.Invoke(Total);
        };
    }

    public static bool Spend(int amount)
    {
        if (amount <= 0) return true;
        if (Total < amount) return false;
        Total -= amount;
        OnTotalChanged?.Invoke(Total);
        return true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Total += value;
        OnTotalChanged?.Invoke(Total);
        Destroy(gameObject);
    }
}
