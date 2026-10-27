using UnityEngine;
using UnityEngine.UI;

public class WaveHUD : MonoBehaviour
{
    public WaveSpawner spawner;
    public Font font;
    public float topOffset = 56f;

    Text label;

    void Start()
    {
        if (spawner == null) spawner = FindFirstObjectByType<WaveSpawner>();

        var canvas = UIBuild.GetCanvas();
        label = UIBuild.Text("WaveLabel", canvas.transform, font, 26, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.gameObject.AddComponent<Outline>().effectColor = Color.black;
        var rt = label.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -topOffset);
        rt.sizeDelta = new Vector2(900f, 40f);
    }

    void Update()
    {
        if (spawner == null || !spawner.Started) { label.text = ""; return; }

        if (spawner.WaveActive)
        {
            label.color = new Color(1f, 0.55f, 0.45f);
            label.text = "DALGA " + spawner.WaveNumber + "   -   Kalan düşman: " + spawner.Alive;
        }
        else
        {
            label.color = new Color(0.7f, 1f, 0.7f);
            label.text = "Sonraki dalga (" + (spawner.WaveNumber + 1) + "): " + Mathf.CeilToInt(spawner.BreakTimeLeft) + " sn   -   Markete uğra!";
        }
    }
}
