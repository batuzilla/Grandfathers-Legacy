using UnityEngine;

[RequireComponent(typeof(Health))]
public class HealthBar : MonoBehaviour
{
    public Vector2 offset = new Vector2(0f, 0.8f);
    public float width = 1f;
    public float height = 0.12f;
    public bool hideWhenFull = false;

    Health health;
    Transform root;
    Transform fill;
    SpriteRenderer fillRenderer;

    void Awake()
    {
        health = GetComponent<Health>();
        Build();
    }

    void OnEnable()
    {
        health.OnChanged += Refresh;
    }

    void OnDisable()
    {
        health.OnChanged -= Refresh;
    }

    void Start()
    {
        Refresh(health.Current, health.maxHealth);
    }

    void Build()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        var sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);

        root = new GameObject("HealthBar").transform;
        root.SetParent(transform, false);
        root.localPosition = new Vector3(offset.x - width / 2f, offset.y, 0f);

        var bg = new GameObject("BG").AddComponent<SpriteRenderer>();
        bg.transform.SetParent(root, false);
        bg.sprite = sprite;
        bg.color = new Color(0f, 0f, 0f, 0.7f);
        bg.sortingOrder = 100;
        bg.transform.localScale = new Vector3(width, height, 1f);

        fillRenderer = new GameObject("Fill").AddComponent<SpriteRenderer>();
        fillRenderer.transform.SetParent(root, false);
        fillRenderer.sprite = sprite;
        fillRenderer.sortingOrder = 101;
        fill = fillRenderer.transform;
    }

    void Refresh(float current, float max)
    {
        float ratio = current / max;
        fill.localScale = new Vector3(width * ratio, height * 0.7f, 1f);
        fill.localPosition = Vector3.zero;
        fillRenderer.color = Color.Lerp(Color.red, Color.green, ratio);
        root.gameObject.SetActive(!(hideWhenFull && ratio >= 1f));
    }
}