using System.Collections;
using UnityEngine;

public class BowShooter : MonoBehaviour
{
    public Arrow arrowPrefab;
    public float fireCooldown = 0.35f;
    public float spawnDistance = 0.5f;

    [Header("Çekme animasyonu")]
    public float fullDrawTime = 0.35f;
    public float minDrawTime = 0.12f;
    public float pullDistance = 0.14f;
    public float kickDistance = 0.07f;
    public float releaseTime = 0.18f;

    public event System.Action Shot;

    public bool Drawing { get; private set; }
    public bool Releasing { get; private set; }
    public float DrawProgress { get; private set; }
    public Vector2 AimDir { get; private set; } = Vector2.down;

    float nextFireTime;
    float drawTime;
    Camera cam;
    Health health;
    Transform bow;
    Vector3 bowRest;
    bool restSaved;

    void Start()
    {
        cam = Camera.main;
        health = GetComponent<Health>();
    }

    void Update()
    {
        UpdateAim();

        if (Releasing) return;

        bool canShoot = (health == null || !health.IsDead) && BowInHand() && InventoryController.Instance.HasArrow();

        if (!Drawing)
        {
            if (canShoot && Input.GetMouseButtonDown(0) && Time.time >= nextFireTime) BeginDraw();
            return;
        }

        if (!canShoot) { CancelDraw(); return; }

        drawTime += Time.deltaTime;
        DrawProgress = Mathf.Clamp01(drawTime / fullDrawTime);
        MoveBow(Vector3.left * (pullDistance * DrawProgress));

        if (Input.GetMouseButtonUp(0))
        {
            if (drawTime >= minDrawTime) StartCoroutine(Release());
            else CancelDraw();
        }
    }

    void UpdateAim()
    {
        if (cam == null) return;
        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(cam.transform.position.z);
        Vector2 d = (Vector2)cam.ScreenToWorldPoint(mouse) - (Vector2)transform.position;
        if (d.sqrMagnitude > 0.0001f) AimDir = d.normalized;
    }

    bool BowInHand()
    {
        var inv = InventoryController.Instance;
        return inv != null && inv.weaponPivot != null && inv.weaponPivot.activeInHierarchy;
    }

    void FindBow()
    {
        var pivot = InventoryController.Instance.weaponPivot.transform;
        bow = pivot.childCount > 0 ? pivot.GetChild(0) : null;
        if (bow != null) { bowRest = bow.localPosition; restSaved = true; }
    }

    void MoveBow(Vector3 offset)
    {
        if (bow != null && restSaved) bow.localPosition = bowRest + offset;
    }

    void BeginDraw()
    {
        FindBow();
        Drawing = true;
        drawTime = 0f;
        DrawProgress = 0f;
    }

    void CancelDraw()
    {
        MoveBow(Vector3.zero);
        Drawing = false;
        DrawProgress = 0f;
    }

    IEnumerator Release()
    {
        Drawing = false;
        Releasing = true;

        if (InventoryController.Instance.ConsumeArrow()) Shoot();
        nextFireTime = Time.time + fireCooldown;

        float t = 0f;
        float from = -pullDistance * DrawProgress;
        while (t < releaseTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / releaseTime);
            float offset = k < 0.3f
                ? Mathf.Lerp(from, kickDistance, k / 0.3f)
                : Mathf.Lerp(kickDistance, 0f, (k - 0.3f) / 0.7f);
            MoveBow(Vector3.right * offset);
            yield return null;
        }

        MoveBow(Vector3.zero);
        DrawProgress = 0f;
        Releasing = false;
    }

    void Shoot()
    {
        Vector2 dir = AimDir;
        Vector2 spawnPos = (Vector2)transform.position + dir * spawnDistance;
        Arrow arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.Launch(dir);
        GameAudio.Shoot();
        if (Shot != null) Shot();
    }
}
