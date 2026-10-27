using System.Collections;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    [System.Serializable]
    public class Line
    {
        public bool dede = true;
        [TextArea(2, 4)] public string text;
        public bool throwItemsAfter;
    }

    [System.Serializable]
    public class BearLine
    {
        public bool player;
        [TextArea(2, 4)] public string text;
    }

    public bool skipIntro;
    public DialogueUI dialogue;
    public WaveSpawner spawner;
    public TutorialSequence tutorial;

    public Texture2D dedeSheet;
    public Sprite dedeFace;
    public Sprite playerFace;
    public string dedeName = "Dede";
    public string playerName = "Ben";

    public GameObject[] throwItems;
    public Vector2 doorPos = new Vector2(0.5f, 3.5f);
    public float walkSpeed = 2f;
    public float meetDistance = 2.2f;
    public float startDelay = 1f;
    public float flightTime = 0.55f;
    public float arcHeight = 1f;

    public Line[] lines =
    {
        new Line { dede = true,  text = "Evlat, sonunda geldin. Bu kale dedemden bana kaldı, şimdi sıra sende." },
        new Line { dede = true,  text = "Karanlık yaratıklar bu tarafa geliyor. Al bu yayı ve okları, kalemizi koru!", throwItemsAfter = true },
        new Line { dede = false, text = "Dede, merak etme. Bu eve kimse zarar veremez!" },
        new Line { dede = true,  text = "Ben içeride olacağım. Duvarlara göz kulak ol, hasar gören yeri tamir etmeyi unutma." },
        new Line { dede = false, text = "Hazırım dede. Gelsinler!" },
    };

    [Header("Ayı sahnesi")]
    public Texture2D bearSheet;
    public Sprite bearFace;
    public string bearName = "Ayı";
    public Vector2 bearStartOffset = new Vector2(0f, -9f);
    public Vector2 bearStopOffset = new Vector2(0f, -3.2f);
    public float bearWalkSpeed = 2.5f;

    public BearLine[] bearLines =
    {
        new BearLine { player = false, text = "Grr... Bu kale artık bizim olacak, ufaklık!" },
        new BearLine { player = true,  text = "Bakalım! Dedemin kalesine kimse el süremez." },
        new BearLine { player = false, text = "Sürümüz geliyor. Seni ve o ihtiyar dedeyi ezeceğiz!" },
        new BearLine { player = true,  text = "Hazırım. Gelin bakalım!" },
    };

    void Reset()
    {
#if UNITY_EDITOR
        bearFace = EditorClipFinder.FindSprite("Bear_Faceset");
        bearSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Bear/Bear_SpriteSheet.png");
#endif
    }

    IEnumerator Start()
    {
        if (spawner == null) spawner = FindFirstObjectByType<WaveSpawner>();
        if (skipIntro)
        {
            if (spawner != null) spawner.BeginWave();
            yield break;
        }

        var playerGo = GameObject.FindGameObjectWithTag("Player");
        var controller = playerGo.GetComponent<PlayerController>();
        var body = playerGo.GetComponent<Rigidbody2D>();
        var animator = playerGo.GetComponent<Animator>();

        foreach (var item in throwItems)
            if (item != null) item.SetActive(false);

        if (controller != null) controller.enabled = false;
        if (body != null) body.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetFloat("Speed", 0f);

        yield return new WaitForSeconds(startDelay);

        Transform dede = CreateDede();
        Vector2 doorFront = new Vector2(doorPos.x, doorPos.y - 1.2f);
        yield return Walk(dede, doorFront, walkSpeed);

        Vector2 meet = (Vector2)playerGo.transform.position + Vector2.up * meetDistance;
        meet.y = Mathf.Clamp(meet.y, -6f, 2.2f);
        yield return Walk(dede, meet, walkSpeed);

        foreach (var line in lines)
        {
            string who = line.dede ? dedeName : playerName;
            Sprite face = line.dede ? dedeFace : playerFace;
            yield return dialogue.ShowLine(who, face, line.text);

            if (line.throwItemsAfter)
            {
                dialogue.Hide();
                yield return ThrowItems(dede.position, playerGo.transform.position);
            }
        }
        dialogue.Hide();

        yield return Walk(dede, doorFront, walkSpeed);
        yield return Walk(dede, doorPos, walkSpeed);
        dede.gameObject.SetActive(false);

        if (controller != null) controller.enabled = true;

        if (tutorial != null) yield return tutorial.Run(playerGo);

        yield return BearScene(playerGo, controller, body, animator);

        if (spawner != null) spawner.BeginWave();
    }

    IEnumerator BearScene(GameObject playerGo, PlayerController controller, Rigidbody2D body, Animator animator)
    {
        if (bearSheet == null && bearFace == null) yield break;

        if (controller != null) controller.enabled = false;
        if (body != null) body.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetFloat("Speed", 0f);
        var shooter = playerGo.GetComponent<BowShooter>();
        if (shooter != null) shooter.enabled = false;

        Vector2 p = playerGo.transform.position;
        Transform bear = CreateBear(p + bearStartOffset);
        if (bear != null) yield return Walk(bear, p + bearStopOffset, bearWalkSpeed);

        foreach (var line in bearLines)
        {
            string who = line.player ? playerName : bearName;
            Sprite face = line.player ? playerFace : bearFace;
            yield return dialogue.ShowLine(who, face, line.text);
        }
        dialogue.Hide();

        if (bear != null)
        {
            yield return Walk(bear, (Vector2)bear.position + bearStartOffset, bearWalkSpeed);
            Destroy(bear.gameObject);
        }

        if (shooter != null) shooter.enabled = true;
        if (controller != null) controller.enabled = true;
    }

    Transform CreateBear(Vector2 pos)
    {
        if (bearSheet == null) return null;
        var go = new GameObject("ScoutBear");
        go.transform.position = pos;
        if (spawner != null && spawner.enemyPrefab != null)
            go.transform.localScale = spawner.enemyPrefab.transform.localScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 6;

        var a = go.AddComponent<DirectionalSheetAnimator>();
        a.sheet = bearSheet;
        a.cell = 16;
        a.downColumn = 0;
        a.leftColumn = 1;
        a.upColumn = 2;
        a.rightColumn = 3;
        a.walkFrames = 4;
        return go.transform;
    }

    Transform CreateDede()
    {
        var go = new GameObject("Dede");
        go.transform.position = doorPos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 6;

        var a = go.AddComponent<DirectionalSheetAnimator>();
        a.sheet = dedeSheet;
        a.downColumn = 0;
        a.upColumn = 1;
        a.leftColumn = 2;
        a.rightColumn = 3;
        a.walkFrames = 4;
        return go.transform;
    }

    IEnumerator Walk(Transform t, Vector2 to, float speed)
    {
        while (Vector2.Distance(t.position, to) > 0.05f)
        {
            t.position = Vector2.MoveTowards(t.position, to, speed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator ThrowItems(Vector2 from, Vector2 playerPos)
    {
        foreach (var item in throwItems)
        {
            if (item == null) continue;
            StartCoroutine(Fly(item.transform, from, PickLanding(playerPos)));
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(flightTime + 0.3f);
    }

    IEnumerator Fly(Transform t, Vector2 from, Vector2 to)
    {
        Quaternion originalRotation = t.rotation;
        t.position = from;
        t.gameObject.SetActive(true);

        float k = 0f;
        while (k < 1f)
        {
            k += Time.deltaTime / flightTime;
            Vector2 p = Vector2.Lerp(from, to, Mathf.Clamp01(k));
            p.y += Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI) * arcHeight;
            t.position = p;
            t.Rotate(0f, 0f, 720f * Time.deltaTime);
            yield return null;
        }
        t.position = to;
        t.rotation = originalRotation;
    }

    Vector2 PickLanding(Vector2 center)
    {
        for (int i = 0; i < 12; i++)
        {
            Vector2 p = center + Random.insideUnitCircle.normalized * Random.Range(1.2f, 2.2f);
            if (p.x < -8.5f || p.x > 8f || p.y < -6.5f || p.y > 2.5f) continue;
            if (Physics2D.OverlapCircle(p, 0.3f) == null) return p;
        }
        return center + Vector2.right * 1.5f;
    }
}
