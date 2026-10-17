using UnityEngine;
using UnityEngine.UI;

public class HouseUI : MonoBehaviour
{
    public Health house;
    public Font font;
    public string label = "EV";
    public Vector2 size = new Vector2(420f, 30f);

    RectTransform fill;
    Text valueText;

    void Start()
    {
        if (house == null)
        {
            var go = GameObject.Find("CastleTarget");
            if (go != null) house = go.GetComponent<Health>();
        }
        if (house == null)
        {
            Debug.LogError("HouseUI: ev (CastleTarget) Health bulunamadi.");
            return;
        }

        Build();
        house.OnChanged += Refresh;
        Refresh(house.Current, house.maxHealth);
    }

    void OnDestroy()
    {
        if (house != null) house.OnChanged -= Refresh;
    }

    void Build()
    {
        var canvas = UIBuild.GetCanvas();

        var bg = UIBuild.Image("HouseBar", canvas.transform, new Color(0f, 0f, 0f, 0.75f));
        var outline = bg.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        var rt = bg.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -16f);
        rt.sizeDelta = size;

        var f = UIBuild.Image("Fill", bg.transform, Color.green);
        fill = f.rectTransform;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = new Vector2(3f, 3f);
        fill.offsetMax = new Vector2(-3f, -3f);

        valueText = UIBuild.Text("Value", bg.transform, font, 20, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        UIBuild.Stretch(valueText.rectTransform, Vector2.zero, Vector2.zero);
        valueText.gameObject.AddComponent<Shadow>();
    }

    void Refresh(float current, float max)
    {
        float ratio = Mathf.Clamp01(current / max);
        fill.anchorMax = new Vector2(ratio, 1f);
        fill.GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, ratio);
        valueText.text = label + "  " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
    }
}
