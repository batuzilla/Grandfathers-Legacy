using UnityEngine;

public class SmokeEmitter : MonoBehaviour
{
    public Texture2D sheet;
    public float interval = 1.1f;
    public float puffScale = 0.45f;
    public int sortingOrder = 7;

    float next;

    void Update()
    {
        if (sheet == null || Time.time < next) return;
        next = Time.time + interval;

        var go = new GameObject("Puff");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(Random.Range(-0.1f, 0.1f), 0f, 0f);
        go.transform.localScale = Vector3.one * puffScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortingOrder;

        var a = go.AddComponent<SheetAnimator>();
        a.sheet = sheet;
        a.cellWidth = 32;
        a.cellHeight = 32;
        a.frames = 6;
        a.fps = 4f;
        a.loop = false;
        a.destroyAtEnd = true;

        go.AddComponent<RiseAndFade>();
    }
}
