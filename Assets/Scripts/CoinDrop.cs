using UnityEngine;

[RequireComponent(typeof(Health))]
public class CoinDrop : MonoBehaviour
{
    public Coin coinPrefab;
    public int minCoins = 1;
    public int maxCoins = 3;

    void OnEnable() { GetComponent<Health>().OnDied += Drop; }
    void OnDisable() { GetComponent<Health>().OnDied -= Drop; }

    void Drop()
    {
        int n = Random.Range(minCoins, maxCoins + 1);
        for (int i = 0; i < n; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.5f;
            Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);
        }
    }
}