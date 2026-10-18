using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class MenuUI
{
    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public static void ScaleCanvas(Canvas canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) return;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    public static Button MakeButton(string label, Transform parent, Font font, Vector2 size, Vector2 pos, System.Action onClick)
    {
        var img = UIBuild.Image(label + "Button", parent, Color.white);
        img.raycastTarget = true;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        var outline = img.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f, 1f);
        outline.effectDistance = new Vector2(3f, -3f);

        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor = new Color(0.17f, 0.14f, 0.22f, 1f);
        colors.highlightedColor = new Color(0.32f, 0.25f, 0.42f, 1f);
        colors.pressedColor = new Color(0.55f, 0.42f, 0.16f, 1f);
        colors.selectedColor = colors.normalColor;
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());

        var txt = UIBuild.Text("Label", img.transform, font, 34, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        txt.text = label;
        UIBuild.Stretch(txt.rectTransform, Vector2.zero, Vector2.zero);
        return btn;
    }

    public static Slider MakeSlider(string label, Transform parent, Font font, float value, Vector2 pos, System.Action<float> onChange)
    {
        var root = UIBuild.Rect(label + "Row", parent);
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(640f, 44f);
        root.anchoredPosition = pos;

        var lbl = UIBuild.Text("Label", root, font, 28, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
        var lrt = lbl.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = new Vector2(0.36f, 1f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;

        var sliderRt = UIBuild.Rect("Slider", root);
        sliderRt.anchorMin = new Vector2(0.38f, 0f);
        sliderRt.anchorMax = new Vector2(1f, 1f);
        sliderRt.offsetMin = sliderRt.offsetMax = Vector2.zero;

        var bg = UIBuild.Image("Background", sliderRt, new Color(0.05f, 0.05f, 0.08f, 1f));
        bg.rectTransform.anchorMin = new Vector2(0f, 0.3f);
        bg.rectTransform.anchorMax = new Vector2(1f, 0.7f);
        bg.rectTransform.offsetMin = bg.rectTransform.offsetMax = Vector2.zero;

        var fillArea = UIBuild.Rect("Fill Area", sliderRt);
        fillArea.anchorMin = new Vector2(0f, 0.3f);
        fillArea.anchorMax = new Vector2(1f, 0.7f);
        fillArea.offsetMin = new Vector2(6f, 0f);
        fillArea.offsetMax = new Vector2(-14f, 0f);

        var fill = UIBuild.Image("Fill", fillArea, new Color(1f, 0.8f, 0.3f, 1f));
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.sizeDelta = new Vector2(10f, 0f);

        var handleArea = UIBuild.Rect("Handle Slide Area", sliderRt);
        handleArea.anchorMin = Vector2.zero;
        handleArea.anchorMax = Vector2.one;
        handleArea.offsetMin = new Vector2(10f, 0f);
        handleArea.offsetMax = new Vector2(-10f, 0f);

        var handle = UIBuild.Image("Handle", handleArea, Color.white);
        handle.raycastTarget = true;
        handle.rectTransform.anchorMin = new Vector2(0f, 0f);
        handle.rectTransform.anchorMax = new Vector2(0f, 1f);
        handle.rectTransform.sizeDelta = new Vector2(22f, 0f);

        var slider = sliderRt.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        System.Action<float> refresh = v => lbl.text = label + "  " + Mathf.RoundToInt(v * 100f) + "%";
        slider.SetValueWithoutNotify(value);
        refresh(value);
        slider.onValueChanged.AddListener(v => { refresh(v); onChange(v); });
        return slider;
    }
}
