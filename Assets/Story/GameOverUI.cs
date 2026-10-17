using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public Health house;
    public Health player;
    public Font font;

    GameObject panel;
    Text title;
    Text sub;
    bool over;
    float shownAt;

    void Start()
    {
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

        Build();
        if (house != null) house.OnDied += HouseFell;
        if (player != null) player.OnDied += PlayerFell;
    }

    void OnDestroy()
    {
        if (house != null) house.OnDied -= HouseFell;
        if (player != null) player.OnDied -= PlayerFell;
    }

    void Build()
    {
        var canvas = UIBuild.GetCanvas();
        var bg = UIBuild.Image("GameOverPanel", canvas.transform, new Color(0f, 0f, 0f, 0.8f));
        panel = bg.gameObject;
        UIBuild.Stretch(bg.rectTransform, Vector2.zero, Vector2.zero);

        title = UIBuild.Text("Title", panel.transform, font, 64, FontStyle.Bold, new Color(1f, 0.4f, 0.35f), TextAnchor.MiddleCenter);
        var trt = title.rectTransform;
        trt.anchorMin = new Vector2(0f, 0.5f);
        trt.anchorMax = new Vector2(1f, 0.5f);
        trt.offsetMin = new Vector2(0f, 0f);
        trt.offsetMax = new Vector2(0f, 90f);

        sub = UIBuild.Text("Sub", panel.transform, font, 28, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);
        var srt = sub.rectTransform;
        srt.anchorMin = new Vector2(0f, 0.5f);
        srt.anchorMax = new Vector2(1f, 0.5f);
        srt.offsetMin = new Vector2(0f, -90f);
        srt.offsetMax = new Vector2(0f, 0f);

        panel.SetActive(false);
    }

    void HouseFell()
    {
        Show("EV YIKILDI!", "Dedeni koruyamadın...  [R] veya tıkla: yeniden dene");
    }

    void PlayerFell()
    {
        StartCoroutine(ShowLater("ÖLDÜN!", "[R] veya tıkla: yeniden dene"));
    }

    IEnumerator ShowLater(string t, string s)
    {
        yield return new WaitForSeconds(1.2f);
        Show(t, s);
    }

    void Show(string t, string s)
    {
        if (over) return;
        over = true;
        shownAt = Time.unscaledTime;
        title.text = t;
        sub.text = s;
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!over || Time.unscaledTime - shownAt < 0.6f) return;
        if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(0)) Restart();
    }

    void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
