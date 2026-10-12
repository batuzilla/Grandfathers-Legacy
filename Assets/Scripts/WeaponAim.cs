using UnityEngine;

public class WeaponAim : MonoBehaviour
{
    private SpriteRenderer bowSpriteRenderer;
    private Transform bowTransform;

    [Header("Katman (Sorting) Ayarları")]
    [SerializeField] private int orderInFront = 3;
    [SerializeField] private int orderBehind = 1;

    [Header("Yönlere Göre Yay Pozisyonları (Offset)")]
    [Tooltip("Aşağı bakarken iki elin tam ortası")]
    public Vector2 downOffset = new Vector2(0f, -0.2f);
    public float downRotation = -90f; // Aşağı bakarken yatay durması için

    [Tooltip("Sağa bakarken")]
    public Vector2 rightOffset = new Vector2(0.25f, -0.05f);
    public float rightRotation = 0f;

    [Tooltip("Sola bakarken")]
    public Vector2 leftOffset = new Vector2(-0.25f, -0.05f);
    public float leftRotation = 0f;

    [Tooltip("Yukarı bakarken")]
    public Vector2 upOffset = new Vector2(0f, 0.1f);
    public float upRotation = 90f;

    private Vector2 lastFacingDirection = Vector2.down;

    void Start()
    {
        bowSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (bowSpriteRenderer != null)
        {
            bowTransform = bowSpriteRenderer.transform;
            bowSpriteRenderer.sortingLayerName = "Default";
        }

        // Başlangıçta aşağı bakış pozisyonunu uygula
        UpdateBowFacing(Vector2.down);
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 4 ana yöne odaklan (Çapraz yerine en baskın yönü seç)
        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f)
        {
            if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
            {
                lastFacingDirection = moveX > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                lastFacingDirection = moveY > 0 ? Vector2.up : Vector2.down;
            }

            UpdateBowFacing(lastFacingDirection);
        }
    }

    void UpdateBowFacing(Vector2 dir)
    {
        if (bowSpriteRenderer == null || bowTransform == null) return;

        // Varsayılan flip sıfırla
        bowSpriteRenderer.flipY = false;
        bowSpriteRenderer.flipX = false;

        // Pivot'un kendi dönüşünü sıfır tutuyoruz, pozisyon ve açıyı Bow'a veriyoruz
        transform.rotation = Quaternion.identity;

        if (dir == Vector2.down)
        {
            bowTransform.localPosition = downOffset;
            bowTransform.localRotation = Quaternion.Euler(0, 0, downRotation);
            bowSpriteRenderer.sortingOrder = orderInFront;
        }
        else if (dir == Vector2.up)
        {
            bowTransform.localPosition = upOffset;
            bowTransform.localRotation = Quaternion.Euler(0, 0, upRotation);
            bowSpriteRenderer.sortingOrder = orderBehind; // Karakterin kafasının arkasında kalsın
        }
        else if (dir == Vector2.right)
        {
            bowTransform.localPosition = rightOffset;
            bowTransform.localRotation = Quaternion.Euler(0, 0, rightRotation);
            bowSpriteRenderer.sortingOrder = orderInFront;
        }
        else if (dir == Vector2.left)
        {
            bowTransform.localPosition = leftOffset;
            bowTransform.localRotation = Quaternion.Euler(0, 0, leftRotation);
            bowSpriteRenderer.flipX = true; // Sola dönerken sprite'ı aynala
            bowSpriteRenderer.sortingOrder = orderInFront;
        }
    }
}