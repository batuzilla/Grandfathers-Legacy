using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public Font font;
    public float charsPerSecond = 45f;
    public Vector2 panelSize = new Vector2(900f, 170f);

    GameObject panel;
    Image faceImage;
    Text nameText;
    Text bodyText;
    Text hintText;

    void Awake()
    {
        Build();
        panel.SetActive(false);
    }

    void Build()
    {
        var canvas = UIBuild.GetCanvas();

        var panelImg = UIBuild.Image("DialoguePanel", canvas.transform, new Color(0.08f, 0.07f, 0.11f, 0.94f));
        panel = panelImg.gameObject;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f, 1f);
        outline.effectDistance = new Vector2(3f, -3f);

        var prt = panelImg.rectTransform;
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0f);
        prt.pivot = new Vector2(0.5f, 0f);
        prt.anchoredPosition = new Vector2(0f, 24f);
        prt.sizeDelta = panelSize;

        var frame = UIBuild.Image("FaceFrame", panel.transform, new Color(0.16f, 0.14f, 0.2f, 1f));
        var frt = frame.rectTransform;
        frt.anchorMin = frt.anchorMax = new Vector2(0f, 0.5f);
        frt.pivot = new Vector2(0f, 0.5f);
        frt.anchoredPosition = new Vector2(16f, 0f);
        frt.sizeDelta = new Vector2(138f, 138f);

        faceImage = UIBuild.Image("Face", frame.transform, Color.white);
        faceImage.preserveAspect = true;
        UIBuild.Stretch(faceImage.rectTransform, new Vector2(4f, 4f), new Vector2(-4f, -4f));

        nameText = UIBuild.Text("Name", panel.transform, font, 28, FontStyle.Bold, new Color(1f, 0.85f, 0.4f), TextAnchor.UpperLeft);
        var nrt = nameText.rectTransform;
        nrt.anchorMin = new Vector2(0f, 1f);
        nrt.anchorMax = new Vector2(1f, 1f);
        nrt.offsetMin = new Vector2(170f, -52f);
        nrt.offsetMax = new Vector2(-16f, -14f);

        bodyText = UIBuild.Text("Body", panel.transform, font, 26, FontStyle.Normal, Color.white, TextAnchor.UpperLeft);
        UIBuild.Stretch(bodyText.rectTransform, new Vector2(170f, 36f), new Vector2(-16f, -56f));

        hintText = UIBuild.Text("Hint", panel.transform, font, 18, FontStyle.Italic, new Color(0.7f, 0.7f, 0.75f), TextAnchor.LowerRight);
        var hrt = hintText.rectTransform;
        hrt.anchorMin = hrt.anchorMax = new Vector2(1f, 0f);
        hrt.pivot = new Vector2(1f, 0f);
        hrt.anchoredPosition = new Vector2(-18f, 8f);
        hrt.sizeDelta = new Vector2(300f, 28f);
    }

    public IEnumerator ShowLine(string speaker, Sprite face, string text)
    {
        panel.SetActive(true);
        nameText.text = speaker;
        faceImage.enabled = face != null;
        if (face != null)
        {
            face.texture.filterMode = FilterMode.Point;
            faceImage.sprite = face;
        }
        bodyText.text = "";
        hintText.text = "";

        yield return null;

        float shown = 0f;
        while (shown < text.Length)
        {
            if (Advance()) break;
            shown += charsPerSecond * Time.deltaTime;
            bodyText.text = text.Substring(0, Mathf.Min(Mathf.FloorToInt(shown), text.Length));
            yield return null;
        }

        bodyText.text = text;
        hintText.text = "[Boşluk] devam";
        yield return null;
        while (!Advance()) yield return null;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    static bool Advance()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0);
    }
}
