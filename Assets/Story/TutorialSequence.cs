using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialStepType { Move, PickBow, PickArrow, EquipBow, Shoot, OpenShop, Info }

public class TutorialSequence : MonoBehaviour
{
    [System.Serializable]
    public class Step
    {
        public TutorialStepType type;
        [TextArea(1, 3)] public string text;
        public float infoSeconds = 5f;
    }

    public Font font;
    public bool skipTutorial;
    public KeyCode skipKey = KeyCode.T;
    public float moveDistance = 4f;
    public AudioClip stepDoneSfx;

    public Step[] steps =
    {
        new Step { type = TutorialStepType.Move,     text = "W A S D ile hareket et." },
        new Step { type = TutorialStepType.PickBow,  text = "Yerdeki yayı al: yanına git ve [E] tuşuna bas." },
        new Step { type = TutorialStepType.PickArrow,text = "Oku da al: yanına git ve [E] tuşuna bas." },
        new Step { type = TutorialStepType.EquipBow, text = "Yayı eline al: 1-4 tuşları veya fare tekerleği ile yayın olduğu slotu seç." },
        new Step { type = TutorialStepType.Shoot,    text = "Sol tıkı basılı tut ve bırak: ok fareye doğru fırlar." },
        new Step { type = TutorialStepType.OpenShop, text = "Evin sağındaki tezgâha git ve [E] tuşuna bas. Yıkılan duvarları buradan tamir edersin." },
        new Step { type = TutorialStepType.Info,     text = "Hazırsın! Yaratıklar yaklaşıyor, evi koru.", infoSeconds = 3f },
    };

    GameObject panel;
    Text header;
    Text body;
    bool shot;
    bool shopOpened;

    void Reset()
    {
#if UNITY_EDITOR
        stepDoneSfx = EditorClipFinder.Find("Sfx_Buy");
#endif
    }

    void Awake()
    {
        Build();
        panel.SetActive(false);
    }

    void Build()
    {
        var canvas = UIBuild.GetCanvas();
        var bg = UIBuild.Image("TutorialPanel", canvas.transform, new Color(0.08f, 0.07f, 0.11f, 0.85f));
        panel = bg.gameObject;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.4f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        var rt = bg.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -110f);
        rt.sizeDelta = new Vector2(560f, 140f);

        header = UIBuild.Text("Header", panel.transform, font, 22, FontStyle.Bold, new Color(1f, 0.85f, 0.4f), TextAnchor.UpperLeft);
        var hrt = header.rectTransform;
        hrt.anchorMin = new Vector2(0f, 1f);
        hrt.anchorMax = new Vector2(1f, 1f);
        hrt.offsetMin = new Vector2(16f, -38f);
        hrt.offsetMax = new Vector2(-16f, -8f);

        body = UIBuild.Text("Body", panel.transform, font, 26, FontStyle.Normal, Color.white, TextAnchor.UpperLeft);
        UIBuild.Stretch(body.rectTransform, new Vector2(16f, 30f), new Vector2(-16f, -42f));

        var hint = UIBuild.Text("Hint", panel.transform, font, 18, FontStyle.Italic, new Color(0.7f, 0.7f, 0.75f), TextAnchor.LowerRight);
        hint.text = "[" + skipKey + "] öğreticiyi geç";
        var irt = hint.rectTransform;
        irt.anchorMin = new Vector2(0f, 0f);
        irt.anchorMax = new Vector2(1f, 0f);
        irt.offsetMin = new Vector2(16f, 6f);
        irt.offsetMax = new Vector2(-16f, 32f);
    }

    public IEnumerator Run(GameObject playerGo)
    {
        if (skipTutorial || steps == null || steps.Length == 0) yield break;

        var inv = InventoryController.Instance;
        var shooter = playerGo.GetComponent<BowShooter>();
        shot = false;
        shopOpened = false;
        System.Action onShot = () => shot = true;
        System.Action onShop = () => shopOpened = true;
        if (shooter != null) shooter.Shot += onShot;
        ShopStation.Opened += onShop;

        panel.SetActive(true);
        bool skipped = false;

        for (int i = 0; i < steps.Length && !skipped; i++)
        {
            var step = steps[i];
            header.text = "GÖREV " + (i + 1) + "/" + steps.Length;
            header.color = new Color(1f, 0.85f, 0.4f);
            body.text = step.text;
            body.color = Color.white;

            Vector2 startPos = playerGo.transform.position;
            float timer = 0f;

            while (true)
            {
                if (Input.GetKeyDown(skipKey)) { skipped = true; break; }

                bool done = false;
                switch (step.type)
                {
                    case TutorialStepType.Move:
                        done = Vector2.Distance(startPos, playerGo.transform.position) >= moveDistance;
                        break;
                    case TutorialStepType.PickBow:
                        done = inv != null && inv.HasBow();
                        break;
                    case TutorialStepType.PickArrow:
                        done = inv != null && inv.HasArrow();
                        break;
                    case TutorialStepType.EquipBow:
                        done = inv != null && inv.weaponPivot != null && inv.weaponPivot.activeInHierarchy;
                        break;
                    case TutorialStepType.Shoot:
                        done = shot;
                        break;
                    case TutorialStepType.OpenShop:
                        done = shopOpened;
                        break;
                    case TutorialStepType.Info:
                        timer += Time.deltaTime;
                        done = timer >= step.infoSeconds;
                        break;
                }
                if (done) break;
                yield return null;
            }

            if (skipped) break;

            if (step.type != TutorialStepType.Info)
            {
                header.text = "TAMAM!";
                header.color = new Color(0.6f, 1f, 0.6f);
                body.color = new Color(0.6f, 1f, 0.6f);
                AudioManager.Play(stepDoneSfx);
                float t = 0f;
                while (t < 0.8f)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
        }

        if (shooter != null) shooter.Shot -= onShot;
        ShopStation.Opened -= onShop;
        panel.SetActive(false);
    }
}
