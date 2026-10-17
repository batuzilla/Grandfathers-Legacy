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

    public bool skipIntro;
    public DialogueUI dialogue;
    public WaveSpawner spawner;

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
        yield return Walk(dede, doorFront);

        Vector2 meet = (Vector2)playerGo.transform.position + Vector2.up * meetDistance;
        meet.y = Mathf.Clamp(meet.y, -6f, 2.2f);
        yield return Walk(dede, meet);

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

        yield return Walk(dede, doorFront);
        yield return Walk(dede, doorPos);
        dede.gameObject.SetActive(false);

        if (controller != null) controller.enabled = true;
        if (spawner != null) spawner.BeginWave();
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

    IEnumerator Walk(Transform t, Vector2 to)
    {
        while (Vector2.Distance(t.position, to) > 0.05f)
        {
            t.position = Vector2.MoveTowards(t.position, to, walkSpeed * Time.deltaTime);
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
