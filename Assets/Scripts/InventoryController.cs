using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;

    [Header("Görseller (Theme Wood)")]
    public Sprite cellSprite;
    public Sprite selectedSprite;
    public Font font;

    [Header("Düzen")]
    public int slotCount = 4;
    public float slotSize = 76f;
    public float spacing = 8f;
    public float bottomMargin = 16f;
    public int maxArrowStack = 20;
    public bool infiniteArrows = true;

    [Header("Eldeki Silah")]
    public GameObject weaponPivot;

    [Header("Eşya Tanıma")]
    public Sprite bowSprite;
    public Sprite arrowSprite;

    Sprite[] items;
    int[] counts;
    Image[] frames;
    Image[] icons;
    Text[] countTexts;
    float[] pop;
    int selected;

    void Awake()
    {
        Instance = this;
        items = new Sprite[slotCount];
        counts = new int[slotCount];
        pop = new float[slotCount];
    }

    void Start()
    {
        BuildUI();
        for (int i = 0; i < slotCount; i++) RefreshSlot(i);
        UpdateEquippedItem();
    }

    void BuildUI()
    {
        var canvas = UIBuild.GetCanvas();
        var bar = UIBuild.Rect("Hotbar_Runtime", canvas.transform);
        bar.SetAsFirstSibling();
        bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 0f);
        bar.pivot = new Vector2(0.5f, 0f);
        bar.anchoredPosition = new Vector2(0f, bottomMargin);
        bar.sizeDelta = new Vector2(slotCount * slotSize + (slotCount - 1) * spacing, slotSize);

        frames = new Image[slotCount];
        icons = new Image[slotCount];
        countTexts = new Text[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            var frame = UIBuild.Image("Slot" + (i + 1), bar, Color.white);
            frames[i] = frame;
            var frt = frame.rectTransform;
            frt.anchorMin = frt.anchorMax = new Vector2(0f, 0.5f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.sizeDelta = new Vector2(slotSize, slotSize);
            frt.anchoredPosition = new Vector2(i * (slotSize + spacing), 0f);

            var icon = UIBuild.Image("Icon", frame.transform, Color.white);
            icon.preserveAspect = true;
            icons[i] = icon;
            UIBuild.Stretch(icon.rectTransform, new Vector2(12f, 12f), new Vector2(-12f, -12f));

            var key = UIBuild.Text("Key", frame.transform, font, 16, FontStyle.Bold, new Color(1f, 1f, 1f, 0.85f), TextAnchor.UpperLeft);
            key.text = (i + 1).ToString();
            key.gameObject.AddComponent<Shadow>();
            UIBuild.Stretch(key.rectTransform, new Vector2(6f, 4f), new Vector2(-4f, -4f));

            var ct = UIBuild.Text("Count", frame.transform, font, 22, FontStyle.Bold, Color.white, TextAnchor.LowerRight);
            ct.gameObject.AddComponent<Outline>().effectColor = Color.black;
            UIBuild.Stretch(ct.rectTransform, new Vector2(4f, 4f), new Vector2(-8f, -4f));
            countTexts[i] = ct;
        }
    }

    void Update()
    {
        HandleInput();
        AnimateSlots();
    }

    void HandleInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SelectSlot((selected - 1 + slotCount) % slotCount);
        else if (scroll < 0f) SelectSlot((selected + 1) % slotCount);

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
    }

    void AnimateSlots()
    {
        if (frames == null) return;
        for (int i = 0; i < slotCount; i++)
        {
            float target = i == selected ? 1.12f : 1f;
            float s = Mathf.Lerp(frames[i].rectTransform.localScale.x, target, 15f * Time.unscaledDeltaTime);
            frames[i].rectTransform.localScale = new Vector3(s, s, 1f);

            if (pop[i] > 0f)
            {
                pop[i] = Mathf.Max(0f, pop[i] - Time.unscaledDeltaTime * 4f);
                float p = 1f + 0.5f * Mathf.Sin(pop[i] * Mathf.PI);
                icons[i].rectTransform.localScale = new Vector3(p, p, 1f);
            }
        }
    }

    void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;
        selected = index;
        for (int i = 0; i < slotCount; i++) RefreshSlot(i);
        UpdateEquippedItem();
    }

    void RefreshSlot(int i)
    {
        if (frames == null) return;

        var frame = frames[i];
        bool sel = i == selected;
        Sprite bg = sel ? selectedSprite : cellSprite;
        frame.sprite = bg;
        if (bg != null) bg.texture.filterMode = FilterMode.Point;
        frame.color = bg != null ? Color.white : (sel ? new Color(1f, 0.9f, 0.2f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.7f));

        bool has = items[i] != null;
        icons[i].enabled = has;
        if (has)
        {
            items[i].texture.filterMode = FilterMode.Point;
            icons[i].sprite = items[i];
        }

        bool isArrow = has && items[i] == arrowSprite && !infiniteArrows;
        countTexts[i].text = isArrow ? counts[i].ToString() : "";
    }

    void UpdateEquippedItem()
    {
        if (weaponPivot == null) return;
        Sprite held = items[selected];
        weaponPivot.SetActive(held != null && held == bowSprite);
    }

    public bool AddItem(Sprite itemSprite)
    {
        if (itemSprite == arrowSprite)
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (items[i] == arrowSprite && counts[i] < maxArrowStack)
                {
                    counts[i]++;
                    pop[i] = 1f;
                    RefreshSlot(i);
                    return true;
                }
            }
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (items[i] == null)
            {
                items[i] = itemSprite;
                counts[i] = 1;
                pop[i] = 1f;
                RefreshSlot(i);
                UpdateEquippedItem();
                return true;
            }
        }

        Debug.Log("Envanter dolu!");
        return false;
    }

    public bool HasBow()
    {
        for (int i = 0; i < slotCount; i++)
            if (items[i] != null && items[i] == bowSprite) return true;
        return false;
    }

    public bool HasArrow()
    {
        for (int i = 0; i < slotCount; i++)
            if (items[i] != null && items[i] == arrowSprite) return true;
        return false;
    }

    public bool ConsumeArrow()
    {
        if (infiniteArrows) return HasArrow();
        for (int i = 0; i < slotCount; i++)
        {
            if (items[i] != null && items[i] == arrowSprite)
            {
                counts[i]--;
                if (counts[i] <= 0)
                {
                    items[i] = null;
                    counts[i] = 0;
                }
                pop[i] = 0.6f;
                RefreshSlot(i);
                UpdateEquippedItem();
                return true;
            }
        }
        return false;
    }
}
