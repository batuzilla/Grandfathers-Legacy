using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string gameScene = "SampleScene";
    public string title = "DEDENİN KALESİ";
    public string subtitle = "Dedemizden kalan kaleyi koru!";
    public Font font;

    public AudioClip menuMusic;
    public AudioClip clickSfx;

    GameObject mainPanel;
    GameObject settingsPanel;

    void Reset()
    {
#if UNITY_EDITOR
        menuMusic = EditorClipFinder.Find("Music_Menu");
        clickSfx = EditorClipFinder.Find("Sfx_Click");
#endif
    }

    void Start()
    {
        Time.timeScale = 1f;
        MenuUI.EnsureEventSystem();
        Build();
        AudioManager.Instance.PlayMusic(menuMusic);
    }

    void Build()
    {
        var canvas = UIBuild.GetCanvas();
        MenuUI.ScaleCanvas(canvas);

        var bg = UIBuild.Image("Background", canvas.transform, new Color(0.07f, 0.08f, 0.13f, 1f));
        UIBuild.Stretch(bg.rectTransform, Vector2.zero, Vector2.zero);

        var band = UIBuild.Image("Band", canvas.transform, new Color(0.12f, 0.1f, 0.2f, 1f));
        band.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        band.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        band.rectTransform.sizeDelta = new Vector2(0f, 420f);
        band.rectTransform.anchoredPosition = new Vector2(0f, -60f);

        var t = UIBuild.Text("Title", canvas.transform, font, 96, FontStyle.Bold, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);
        t.text = title;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.gameObject.AddComponent<Outline>().effectColor = new Color(0.25f, 0.1f, 0f, 1f);
        var trt = t.rectTransform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -110f);
        trt.sizeDelta = new Vector2(1700f, 130f);

        var s = UIBuild.Text("Subtitle", canvas.transform, font, 34, FontStyle.Italic, new Color(0.8f, 0.8f, 0.9f), TextAnchor.MiddleCenter);
        s.text = subtitle;
        var srt = s.rectTransform;
        srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 1f);
        srt.pivot = new Vector2(0.5f, 1f);
        srt.anchoredPosition = new Vector2(0f, -250f);
        srt.sizeDelta = new Vector2(1200f, 50f);

        var credit = UIBuild.Text("Credit", canvas.transform, font, 20, FontStyle.Normal, new Color(1f, 1f, 1f, 0.45f), TextAnchor.LowerCenter);
        credit.text = "Prototip  -  Ninja Adventure ve Kenney Tiny Town görselleri (CC0)";
        var crt = credit.rectTransform;
        crt.anchorMin = new Vector2(0f, 0f);
        crt.anchorMax = new Vector2(1f, 0f);
        crt.pivot = new Vector2(0.5f, 0f);
        crt.anchoredPosition = new Vector2(0f, 14f);
        crt.sizeDelta = new Vector2(0f, 36f);

        mainPanel = UIBuild.Rect("MainPanel", canvas.transform).gameObject;
        UIBuild.Stretch((RectTransform)mainPanel.transform, Vector2.zero, Vector2.zero);
        var size = new Vector2(420f, 80f);
        MenuUI.MakeButton("OYNA", mainPanel.transform, font, size, new Vector2(0f, 0f), Play);
        MenuUI.MakeButton("AYARLAR", mainPanel.transform, font, size, new Vector2(0f, -110f), () => ShowSettings(true));
        MenuUI.MakeButton("ÇIKIŞ", mainPanel.transform, font, size, new Vector2(0f, -220f), Quit);

        settingsPanel = UIBuild.Rect("SettingsPanel", canvas.transform).gameObject;
        UIBuild.Stretch((RectTransform)settingsPanel.transform, Vector2.zero, Vector2.zero);

        var header = UIBuild.Text("Header", settingsPanel.transform, font, 52, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        header.text = "AYARLAR";
        var hrt = header.rectTransform;
        hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0.5f);
        hrt.sizeDelta = new Vector2(600f, 70f);
        hrt.anchoredPosition = new Vector2(0f, 80f);

        var am = AudioManager.Instance;
        MenuUI.MakeSlider("Müzik", settingsPanel.transform, font, am.MusicVolume, new Vector2(0f, -10f), v => AudioManager.Instance.MusicVolume = v);
        MenuUI.MakeSlider("Efekt", settingsPanel.transform, font, am.SfxVolume, new Vector2(0f, -80f), v => AudioManager.Instance.SfxVolume = v);
        MenuUI.MakeButton("GERİ", settingsPanel.transform, font, size, new Vector2(0f, -190f), () => ShowSettings(false));

        ShowSettings(false);
    }

    void ShowSettings(bool on)
    {
        Click();
        settingsPanel.SetActive(on);
        mainPanel.SetActive(!on);
    }

    void Play()
    {
        Click();
        SceneManager.LoadScene(gameScene);
    }

    void Quit()
    {
        Click();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Click()
    {
        AudioManager.Play(clickSfx);
    }
}
