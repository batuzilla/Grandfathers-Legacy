using UnityEngine;

public class BowShooter : MonoBehaviour
{
    public Arrow arrowPrefab;
    public float fireCooldown = 0.4f;
    public float spawnDistance = 0.5f;

    float nextFireTime;
    Camera cam;
    Health health;

    void Start()
    {
        cam = Camera.main;
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (health != null && health.IsDead) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (Time.time < nextFireTime) return;
        if (!BowInHand()) return;

        Shoot();
        nextFireTime = Time.time + fireCooldown;
    }

    bool BowInHand()
    {
        var inv = InventoryController.Instance;
        return inv != null && inv.weaponPivot != null && inv.weaponPivot.activeInHierarchy;
    }

    void Shoot()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(cam.transform.position.z);
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouse);

        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        if (dir == Vector2.zero) return;

        Vector2 spawnPos = (Vector2)transform.position + dir * spawnDistance;
        Arrow arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
        arrow.Launch(dir);
    }
}