using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ShopEffect { Custom, HealHouse, HealPlayer, RepairAllWalls }

[System.Serializable]
public class ShopItem
{
    public string title = "Eşya";
    [TextArea(1, 3)] public string description;
    public Sprite icon;
    public int price = 10;
    public ShopEffect effect = ShopEffect.Custom;
    public float amount = 50f;
    public int maxPurchases = 0;
    public UnityEvent onBuy;
    [HideInInspector] public int bought;
}

public class ShopStation : MonoBehaviour
{
    public static bool IsOpen;
    public static int LastCloseFrame = -1;
    public static event System.Action Opened;

    public Font font;
    public KeyCode key = KeyCode.E;
    public float interactRange = 2.5f;
    public bool pauseWhileOpen = true;

    public Health house;

    [Header("Sesler")]
    public AudioClip openSfx;
    public AudioClip buySfx;
    public AudioClip failSfx;

    [Header("Eşyalar (yeni eşya eklemek için listeyi uzat)")]
    public List<ShopItem> items = new List<ShopItem>
    {
        new ShopItem { title = "Duvarları Tamir Et", description = "Yıkılan ve hasar gören tüm duvarları onarır.", price = 15, effect = ShopEffect.RepairAllWalls },
        new ShopItem { title = "Evi Onar", description = "Evin canını 100 doldurur.", price = 10, effect = ShopEffect.HealHouse, amount = 100f },
        new ShopItem { title = "Şifa İksiri", description = "Canını tamamen doldurur.", price = 5, effect = ShopEffect.HealPlayer, amount = 9999f },
    };

    class Row
    {
        public Button button;
        public Text label;
    }

    const float RowHeight = 124f;

    Transform playerTf;
    Health playerHealth;
    GameObject prompt;
    GameObject panel;
    Text coinText;
    Text messageText;
    RectTransform content;
    readonly List<Row> rows = new List<Row>();
    bool open;
    bool pausedByShop;
    float messageTime;

    void Reset()
    {
#if UNITY_EDITOR
        openSfx = EditorClipFinder.Find("Sfx_Click");
        buySfx = EditorClipFinder.Find("Sfx_Buy");
        failSfx = EditorClipFinder.Find("Sfx_Fail");
        if (items.Count >= 3)
        {
            items[0].icon = EditorClipFinder.FindSprite("Shop_Repair");
            items[1].icon = EditorClipFinder.FindSprite("Shop_House");
            items[2].icon = EditorClipFinder.FindSprite("Shop_Potion");
        }
#endif
    }

    void OnEnable() { Coin.OnTotalChanged += OnCoins; }
    void OnDisable() { Coin.OnTotalChanged -= OnCoins; }

    void Start()
    {
        MenuUI.EnsureEventSystem();

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            playerTf = p.transform;
            playerHealth = p.GetComponent<Health>();
        }
        if (house == null)
        {
            var go = GameObject.Find("CastleTarget");
            if (go != null) house = go.GetComponent<Health>();
        }

        Build();
        panel.SetActive(false);
        prompt.SetActive(false);
    }

    void OnDestroy()
    {
        IsOpen = false;
        if (open && pausedByShop) Time.timeScale = 1f;
    }

    void Build()
    {
        var canvas = UIBuild.GetCanvas();

        var pr = UIBuild.Text("ShopPrompt", canvas.transform, font, 30, FontStyle.Bold, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);
        pr.text = "[" + key + "]  MARKET";
        pr.horizontalOverflow = HorizontalWrapMode.Overflow;
        pr.gameObject.AddComponent<Outline>().effectColor = Color.black;
        var prt = pr.rectTransform;
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
        prt.pivot = new Vector2(0.5f, 0f);
        prt.anchoredPosition = new Vector2(0f, 120f);
        prt.sizeDelta = new Vector2(600f, 44f);
        prompt = pr.gameObject;

        var overlay = UIBuild.Image("ShopPanel", canvas.transform, new Color(0f, 0f, 0f, 0.6f));
        overlay.raycastTarget = true;
        panel = overlay.gameObject;
        UIBuild.Stretch(overlay.rectTransform, Vector2.zero, Vector2.zero);

        var win = UIBuild.Image("Window", panel.transform, new Color(0.1f, 0.08f, 0.14f, 0.98f));
        win.raycastTarget = true;
        var outline = win.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f, 1f);
        outline.effectDistance = new Vector2(3f, -3f);
        var wrt = win.rectTransform;
        wrt.anchorMin = wrt.anchorMax = new Vector2(0.5f, 0.5f);
        wrt.sizeDelta = new Vector2(900f, 640f);
        wrt.anchoredPosition = Vector2.zero;

        var title = UIBuild.Text("Title", win.transform, font, 46, FontStyle.Bold, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);
        title.text = "MARKET";
        var trt = title.rectTransform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -16f);
        trt.sizeDelta = new Vector2(500f, 60f);

        coinText = UIBuild.Text("Coins", win.transform, font, 30, FontStyle.Bold, Color.white, TextAnchor.MiddleRight);
        var crt = coinText.rectTransform;
        crt.anchorMin = crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(1f, 1f);
        crt.anchoredPosition = new Vector2(-28f, -26f);
        crt.sizeDelta = new Vector2(280f, 40f);

        var scrollImg = UIBuild.Image("Scroll", win.transform, new Color(0f, 0f, 0f, 0.28f));
        scrollImg.raycastTarget = true;
        var srt = scrollImg.rectTransform;
        srt.anchorMin = Vector2.zero;
        srt.anchorMax = Vector2.one;
        srt.offsetMin = new Vector2(24f, 150f);
        srt.offsetMax = new Vector2(-24f, -86f);

        var viewport = UIBuild.Rect("Viewport", scrollImg.transform);
        UIBuild.Stretch(viewport, new Vector2(4f, 4f), new Vector2(-4f, -4f));
        viewport.gameObject.AddComponent<RectMask2D>();

        content = UIBuild.Rect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;

        var scroll = scrollImg.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;

        messageText = UIBuild.Text("Message", win.transform, font, 26, FontStyle.Bold, new Color(0.8f, 1f, 0.8f), TextAnchor.MiddleCenter);
        var mrt = messageText.rectTransform;
        mrt.anchorMin = mrt.anchorMax = new Vector2(0.5f, 0f);
        mrt.pivot = new Vector2(0.5f, 0f);
        mrt.anchoredPosition = new Vector2(0f, 96f);
        mrt.sizeDelta = new Vector2(860f, 44f);

        MenuUI.MakeButton("KAPAT  [" + key + "]", win.transform, font, new Vector2(320f, 66f), new Vector2(0f, -272f), Close);
    }

    void BuildRows()
    {
        for (int i = content.childCount - 1; i >= 0; i--) Destroy(content.GetChild(i).gameObject);
        rows.Clear();
        content.sizeDelta = new Vector2(0f, items.Count * RowHeight);

        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            var item = items[i];

            var row = UIBuild.Image("Row" + i, content, new Color(0.17f, 0.14f, 0.22f, 0.97f));
            var rrt = row.rectTransform;
            rrt.anchorMin = new Vector2(0f, 1f);
            rrt.anchorMax = new Vector2(1f, 1f);
            rrt.pivot = new Vector2(0.5f, 1f);
            rrt.sizeDelta = new Vector2(-8f, RowHeight - 10f);
            rrt.anchoredPosition = new Vector2(0f, -i * RowHeight);

            var frame = UIBuild.Image("IconFrame", row.transform, new Color(0.07f, 0.06f, 0.1f, 1f));
            var frt = frame.rectTransform;
            frt.anchorMin = frt.anchorMax = new Vector2(0f, 0.5f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.anchoredPosition = new Vector2(12f, 0f);
            frt.sizeDelta = new Vector2(88f, 88f);

            var icon = UIBuild.Image("Icon", frame.transform, Color.white);
            icon.preserveAspect = true;
            UIBuild.Stretch(icon.rectTransform, new Vector2(10f, 10f), new Vector2(-10f, -10f));
            if (item.icon != null)
            {
                item.icon.texture.filterMode = FilterMode.Point;
                icon.sprite = item.icon;
            }
            else icon.enabled = false;

            var name = UIBuild.Text("Name", row.transform, font, 30, FontStyle.Bold, new Color(1f, 0.85f, 0.4f), TextAnchor.LowerLeft);
            name.text = item.title;
            var nrt = name.rectTransform;
            nrt.anchorMin = new Vector2(0f, 0.5f);
            nrt.anchorMax = new Vector2(1f, 1f);
            nrt.offsetMin = new Vector2(116f, 0f);
            nrt.offsetMax = new Vector2(-250f, -8f);

            var desc = UIBuild.Text("Desc", row.transform, font, 22, FontStyle.Normal, new Color(0.82f, 0.82f, 0.88f), TextAnchor.UpperLeft);
            desc.text = item.description;
            var drt = desc.rectTransform;
            drt.anchorMin = new Vector2(0f, 0f);
            drt.anchorMax = new Vector2(1f, 0.5f);
            drt.offsetMin = new Vector2(116f, 8f);
            drt.offsetMax = new Vector2(-250f, 0f);

            var btn = MenuUI.MakeButton("", row.transform, font, new Vector2(220f, 64f), Vector2.zero, () => Buy(index));
            var brt = (RectTransform)btn.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(1f, 0.5f);
            brt.pivot = new Vector2(1f, 0.5f);
            brt.anchoredPosition = new Vector2(-14f, 0f);
            var colors = btn.colors;
            colors.disabledColor = new Color(0.1f, 0.1f, 0.12f, 0.9f);
            btn.colors = colors;
            var label = btn.GetComponentInChildren<Text>();
            label.fontSize = 28;

            rows.Add(new Row { button = btn, label = label });
        }
    }

    void Update()
    {
        if (GameOverUI.IsOver)
        {
            if (open) Close();
            prompt.SetActive(false);
            return;
        }

        bool near = playerTf != null && Vector2.Distance(playerTf.position, transform.position) <= interactRange;
        prompt.SetActive(near && !open);

        if (Input.GetKeyDown(key))
        {
            if (open) Close();
            else if (near && Time.timeScale > 0f) Open();
        }

        if (open && Input.GetKeyDown(KeyCode.Escape)) Close();

        if (messageText != null && messageText.text != "" && Time.unscaledTime - messageTime > 3f)
            messageText.text = "";
    }

    void Open()
    {
        open = true;
        IsOpen = true;
        BuildRows();
        panel.SetActive(true);
        Refresh();
        Say("");
        if (pauseWhileOpen)
        {
            pausedByShop = true;
            Time.timeScale = 0f;
        }
        AudioManager.Play(openSfx);
        if (Opened != null) Opened();
    }

    void Close()
    {
        if (!open) return;
        open = false;
        IsOpen = false;
        LastCloseFrame = Time.frameCount;
        panel.SetActive(false);
        if (pausedByShop)
        {
            pausedByShop = false;
            Time.timeScale = 1f;
        }
        AudioManager.Play(openSfx);
    }

    void OnCoins(int total)
    {
        if (open) Refresh();
    }

    void Refresh()
    {
        coinText.text = "COIN: " + Coin.Total;
        for (int i = 0; i < rows.Count && i < items.Count; i++)
        {
            var it = items[i];
            bool sold = it.maxPurchases > 0 && it.bought >= it.maxPurchases;
            rows[i].label.text = sold ? "TÜKENDİ" : it.price + " COIN";
            rows[i].button.interactable = !sold && Coin.Total >= it.price;
        }
    }

    void Say(string text)
    {
        messageText.text = text;
        messageTime = Time.unscaledTime;
    }

    void Buy(int index)
    {
        if (index < 0 || index >= items.Count) return;
        var it = items[index];
        if (it.maxPurchases > 0 && it.bought >= it.maxPurchases) return;

        if (Coin.Total < it.price)
        {
            Fail("Yeterli coin yok.");
            return;
        }

        string message;
        if (!Apply(it, out message))
        {
            Fail(message);
            return;
        }

        Coin.Spend(it.price);
        it.bought++;
        messageText.color = new Color(0.8f, 1f, 0.8f);
        Say(message);
        AudioManager.Play(buySfx);
        Refresh();
    }

    void Fail(string message)
    {
        messageText.color = new Color(1f, 0.6f, 0.5f);
        Say(message);
        AudioManager.Play(failSfx);
    }

    bool Apply(ShopItem it, out string message)
    {
        switch (it.effect)
        {
            case ShopEffect.HealHouse:
                if (house == null || house.IsDead) { message = "Ev bulunamadı."; return false; }
                if (house.Current >= house.maxHealth - 0.01f) { message = "Evin canı zaten dolu."; return false; }
                house.Heal(it.amount);
                message = "Evin canı yenilendi.";
                return true;

            case ShopEffect.HealPlayer:
                if (playerHealth == null || playerHealth.IsDead) { message = "Canını dolduramazsın."; return false; }
                if (playerHealth.Current >= playerHealth.maxHealth - 0.01f) { message = "Canın zaten dolu."; return false; }
                playerHealth.Heal(it.amount);
                message = "Canın yenilendi.";
                return true;

            case ShopEffect.RepairAllWalls:
                int count = 0;
                foreach (var w in FindObjectsByType<WallSegment>(FindObjectsSortMode.None))
                {
                    if (!w.NeedsRepair) continue;
                    w.Repair();
                    count++;
                }
                if (count == 0) { message = "Tamir edilecek duvar yok."; return false; }
                message = count + " duvar tamir edildi.";
                return true;

            default:
                if (it.onBuy != null) it.onBuy.Invoke();
                message = it.title + " satın alındı.";
                return true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
