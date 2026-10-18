using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public Font font;
    public string menuScene = "MainMenu";

    GameObject panel;
    bool paused;

    void Start()
    {
        MenuUI.EnsureEventSystem();
        Build();
        panel.SetActive(false);
    }

    void Build()
    {
        var canvas = UIBuild.GetCanvas();
        var bg = UIBuild.Image("PausePanel", canvas.transform, new Color(0f, 0f, 0f, 0.82f));
        bg.raycastTarget = true;
        panel = bg.gameObject;
        UIBuild.Stretch(bg.rectTransform, Vector2.zero, Vector2.zero);

        var title = UIBuild.Text("Title", panel.transform, font, 60, FontStyle.Bold, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);
        title.text = "DURAKLATILDI";
        var trt = title.rectTransform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(800f, 80f);
        trt.anchoredPosition = new Vector2(0f, 190f);

        var am = AudioManager.Instance;
        MenuUI.MakeSlider("Müzik", panel.transform, font, am.MusicVolume, new Vector2(0f, 90f), v => AudioManager.Instance.MusicVolume = v);
        MenuUI.MakeSlider("Efekt", panel.transform, font, am.SfxVolume, new Vector2(0f, 20f), v => AudioManager.Instance.SfxVolume = v);

        var size = new Vector2(420f, 76f);
        MenuUI.MakeButton("DEVAM", panel.transform, font, size, new Vector2(0f, -80f), () => SetPaused(false));
        MenuUI.MakeButton("ANA MENÜ", panel.transform, font, size, new Vector2(0f, -170f), ToMenu);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (GameOverUI.IsOver) return;
        SetPaused(!paused);
    }

    void SetPaused(bool value)
    {
        paused = value;
        panel.SetActive(value);
        Time.timeScale = value ? 0f : 1f;
    }

    void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }
}
