using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static bool IsOver;

    public Health house;
    public Health player;
    public WaveSpawner spawner;
    public Font font;
    public string menuScene = "MainMenu";
    public float winDelay = 1.5f;

    GameObject panel;
    Text title;
    Text sub;
    bool over;
    float shownAt;

    void Start()
    {
        IsOver = false;
        MenuUI.EnsureEventSystem();

        if (house == null)
        {
            var go = GameObject.Find("CastleTarget");
            if (go != null) house = go.GetComponent<Health>();
        }
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.GetComponent<Health>();
        }
        if (spawner == null) spawner = FindFirstObjectByType<WaveSpawner>();

        Build();
        if (house != null) house.OnDied += HouseFell;
        if (player != null) player.OnDied += PlayerFell;
        if (spawner != null) spawner.OnAllWavesDone += AllWavesWon;
    }

    void OnDestroy()
    {
        IsOver = false;
        if (house != null) house.OnDied -= HouseFell;
        if (player != null) player.OnDied -= PlayerFell;
        if (spawner != null) spawner.OnAllWavesDone -= AllWavesWon;
    }

    void Build()
    {
        var canvas = UIBuild.GetCanvas();
        var bg = UIBuild.Image("GameOverPanel", canvas.transform, new Color(0f, 0f, 0f, 0.82f));
        bg.raycastTarget = true;
        panel = bg.gameObject;
        UIBuild.Stretch(bg.rectTransform, Vector2.zero, Vector2.zero);

        title = UIBuild.Text("Title", panel.transform, font, 80, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        title.gameObject.AddComponent<Outline>().effectColor = Color.black;
        var trt = title.rectTransform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(1400f, 110f);
        trt.anchoredPosition = new Vector2(0f, 130f);

        sub = UIBuild.Text("Sub", panel.transform, font, 34, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);
        var srt = sub.rectTransform;
        srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.sizeDelta = new Vector2(1300f, 100f);
        srt.anchoredPosition = new Vector2(0f, 40f);

        var size = new Vector2(420f, 76f);
        MenuUI.MakeButton("YENİDEN DENE  [R]", panel.transform, font, size, new Vector2(0f, -90f), Restart);
        MenuUI.MakeButton("ANA MENÜ  [M]", panel.transform, font, size, new Vector2(0f, -180f), ToMenu);

        panel.SetActive(false);
    }

    void HouseFell()
    {
        Show("EV YIKILDI!", DefeatText("Dedeni koruyamadın..."), new Color(1f, 0.4f, 0.35f), false);
    }

    void PlayerFell()
    {
        StartCoroutine(ShowLater("ÖLDÜN!", DefeatText("Yeniden dene ve kaleyi koru."), new Color(1f, 0.4f, 0.35f), false, 1.2f));
    }

    void AllWavesWon()
    {
        if (house == null || house.IsDead) return;
        if (player != null && player.IsDead) return;
        StartCoroutine(ShowLater("TEBRİKLER!", "Dedeyi korudun, kale ayakta!", new Color(1f, 0.85f, 0.4f), true, winDelay));
    }

    string DefeatText(string line)
    {
        if (spawner != null && spawner.WaveNumber > 0)
            return line + "\nDalga " + spawner.WaveNumber + " içinde yenildin.";
        return line;
    }

    IEnumerator ShowLater(string t, string s, Color c, bool win, float delay)
    {
        yield return new WaitForSeconds(delay);
        Show(t, s, c, win);
    }

    void Show(string t, string s, Color c, bool win)
    {
        if (over) return;
        over = true;
        IsOver = true;
        shownAt = Time.unscaledTime;
        title.text = t;
        title.color = c;
        sub.text = s;
        panel.SetActive(true);
        Time.timeScale = 0f;

        if (win) GameAudio.Victory();
        else GameAudio.Defeat();
    }

    void Update()
    {
        if (!over || Time.unscaledTime - shownAt < 0.5f) return;
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (Input.GetKeyDown(KeyCode.M)) ToMenu();
    }

    void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }
}
