using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    public Text coinText;

    void OnEnable() { Coin.OnTotalChanged += Refresh; }
    void OnDisable() { Coin.OnTotalChanged -= Refresh; }
    void Start() { Refresh(Coin.Total); }

    void Refresh(int total)
    {
        coinText.text = total.ToString();
    }
}