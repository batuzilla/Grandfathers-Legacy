using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(SpriteRenderer))]
public class WallSegment : MonoBehaviour
{
    public Sprite[] crackOverlays;
    public Sprite brokenSprite;
    public Color damagedTint = new Color(0.6f, 0.6f, 0.6f);
    public GameObject dustPrefab;
    public float hitInterval = 0.5f;

    public bool IsBroken { get; private set; }
    public bool NeedsRepair { get { return IsBroken || health.Current < health.maxHealth - 0.01f; } }

    Health health;
    SpriteRenderer sr;
    SpriteRenderer crackSr;
    Collider2D col;
    Sprite intactSprite;
    float nextHitTime;

    void Awake()
    {
        health = GetComponent<Health>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        var go = new GameObject("Cracks");
        go.transform.SetParent(transform, false);
        crackSr = go.AddComponent<SpriteRenderer>();
        crackSr.sortingLayerID = sr.sortingLayerID;
        crackSr.sortingOrder = sr.sortingOrder + 1;
        crackSr.enabled = false;
    }

    void OnEnable()
    {
        health.OnChanged += OnHit;
        health.OnDied += Break;
        if (!IsBroken) Pathfinder.AddWall(transform.position);
    }

    void OnDisable()
    {
        health.OnChanged -= OnHit;
        health.OnDied -= Break;
        Pathfinder.RemoveWall(transform.position);
    }

    public void Hit(float damage)
    {
        if (IsBroken) return;
        if (Time.time < nextHitTime) return;
        nextHitTime = Time.time + hitInterval;
        health.TakeDamage(damage);
    }

    public void Repair()
    {
        if (IsBroken)
        {
            IsBroken = false;
            col.enabled = true;
            sr.enabled = true;
            if (intactSprite != null) sr.sprite = intactSprite;
            sr.color = Color.white;
            crackSr.enabled = false;

            var bar = transform.Find("HealthBar");
            if (bar != null) bar.gameObject.SetActive(true);

            health.Revive(health.maxHealth);
            Pathfinder.AddWall(transform.position);
        }
        else
        {
            health.Heal(health.maxHealth);
        }
    }

    void OnHit(float current, float max)
    {
        float ratio = current / max;
        sr.color = Color.Lerp(damagedTint, Color.white, ratio);

        if (crackOverlays != null && crackOverlays.Length > 0 && ratio < 1f)
        {
            int i = Mathf.Clamp(Mathf.FloorToInt((1f - ratio) * crackOverlays.Length), 0, crackOverlays.Length - 1);
            crackSr.sprite = crackOverlays[i];
            crackSr.enabled = true;
        }
        else
        {
            crackSr.enabled = false;
        }
    }

    void Break()
    {
        IsBroken = true;
        intactSprite = sr.sprite;
        Pathfinder.RemoveWall(transform.position);
        col.enabled = false;
        crackSr.enabled = false;
        sr.color = Color.white;

        if (brokenSprite != null) sr.sprite = brokenSprite;
        else sr.enabled = false;

        var bar = transform.Find("HealthBar");
        if (bar != null) bar.gameObject.SetActive(false);

        if (dustPrefab != null) Instantiate(dustPrefab, transform.position, Quaternion.identity);
    }
}
