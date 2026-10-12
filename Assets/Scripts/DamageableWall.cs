using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class DamageableWall : MonoBehaviour
{
    public float maxHealth = 100f;
    public float repairAmountPerSecond = 25f;

    [Header("Görünüm (sağlam → yıkık sırasıyla)")]
    public Sprite[] stateSprites;
    public Color brokenColor = new Color(0.6f, 0.6f, 0.6f);

    public float Health { get; private set; }
    public bool IsDestroyed => Health <= 0f;

    SpriteRenderer sr;
    BoxCollider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        Health = maxHealth;
        UpdateVisual();
    }

    public void TakeDamage(float amount)
    {
        if (IsDestroyed) return;
        Health = Mathf.Max(0, Health - amount);
        UpdateVisual();
    }

    public void Repair(float deltaTime)
    {
        Health = Mathf.Min(maxHealth, Health + repairAmountPerSecond * deltaTime);
        UpdateVisual();
    }

    void UpdateVisual()
    {
        col.enabled = !IsDestroyed;
        float ratio = Health / maxHealth;

        if (stateSprites != null && stateSprites.Length > 0)
        {
            int index = Mathf.Clamp(Mathf.FloorToInt((1f - ratio) * stateSprites.Length),
                                    0, stateSprites.Length - 1);
            sr.sprite = stateSprites[index];
        }
        sr.color = Color.Lerp(brokenColor, Color.white, ratio);
    }
}