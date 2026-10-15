using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public static int Total { get; private set; }
    public static event Action<int> OnTotalChanged;

    public int value = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Total += value;
        OnTotalChanged?.Invoke(Total);
        Destroy(gameObject);
    }
}