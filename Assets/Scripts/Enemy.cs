using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectRange = 5f;
    public float attackRange = 0.8f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float wallReach = 0.6f;
    public float repathInterval = 0.5f;
    public float separationRadius = 0.9f;
    public float separationStrength = 1.5f;
    public float slotSpread = 2.5f;

    public Transform castleTarget;

    Transform player;
    Health health;
    Rigidbody2D rb;
    SpriteRenderer sr;
    float nextAttackTime;
    float nextRepathTime;
    List<Vector2> path;
    int pathIndex;
    Vector2 slotOffset;
    Vector2 lastCheckPos;
    float nextStuckCheck;
    float unstuckUntil;
    Vector2 unstuckDir;
    float lastWallHitTime = -10f;

    void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        nextRepathTime = Time.time + Random.Range(0f, repathInterval);
        slotOffset = Random.insideUnitCircle * slotSpread;
        lastCheckPos = rb.position;
    }

    void OnEnable()
    {
        health.OnDied += Die;
    }

    void OnDisable()
    {
        health.OnDied -= Die;
    }

    void FixedUpdate()
    {
        if (health.IsDead) return;

        Transform target = ChooseTarget();
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;

        if (toTarget.magnitude <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack(target);
            return;
        }

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;
            Vector2 goal = target.position;
            if (target == castleTarget && toTarget.magnitude > 4f) goal += slotOffset;
            RecalculatePath(goal);
        }

        Vector2 pathDir = NextDirection(toTarget);

        if (TryHitWall(pathDir))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = (pathDir + Separation() * separationStrength).normalized;

        CheckStuck();
        if (Time.time < unstuckUntil) dir = unstuckDir;

        rb.linearVelocity = dir * moveSpeed;
        if (sr != null && Mathf.Abs(dir.x) > 0.01f) sr.flipX = dir.x < 0;
    }

    bool TryHitWall(Vector2 dir)
    {
        var hits = Physics2D.CircleCastAll(rb.position, 0.2f, dir, wallReach);
        foreach (var hit in hits)
        {
            var col = hit.collider;
            if (col.gameObject == gameObject || col.isTrigger) continue;
            if (col.GetComponent<Enemy>() != null) continue;

            var wall = col.GetComponent<WallSegment>();
            if (wall == null) return false;
            if (wall.IsBroken) continue;

            lastWallHitTime = Time.time;
            if (Time.time >= nextAttackTime)
            {
                wall.Hit(attackDamage);
                nextAttackTime = Time.time + attackCooldown;
            }
            return true;
        }
        return false;
    }

    void RecalculatePath(Vector2 targetPos)
    {
        if (Pathfinder.Instance == null) return;
        path = Pathfinder.Instance.FindPath(rb.position, targetPos);
        pathIndex = (path != null && path.Count > 1) ? 1 : 0;
    }

    Vector2 NextDirection(Vector2 toTarget)
    {
        if (path == null || pathIndex >= path.Count) return toTarget.normalized;

        Vector2 toPoint = path[pathIndex] - rb.position;
        while (toPoint.magnitude < 0.25f && pathIndex < path.Count - 1)
        {
            pathIndex++;
            toPoint = path[pathIndex] - rb.position;
        }
        return toPoint.normalized;
    }

    void CheckStuck()
    {
        if (Time.time < nextStuckCheck) return;
        nextStuckCheck = Time.time + 0.7f;

        bool moved = Vector2.Distance(rb.position, lastCheckPos) > 0.25f;
        lastCheckPos = rb.position;

        if (Time.time - lastWallHitTime < 1.5f) return;

        if (!moved && Time.time >= unstuckUntil)
        {
            unstuckDir = Random.insideUnitCircle.normalized;
            unstuckUntil = Time.time + 0.5f;
            nextRepathTime = Time.time + 0.5f;
        }
    }

    Vector2 Separation()
    {
        Vector2 push = Vector2.zero;
        var hits = Physics2D.OverlapCircleAll(rb.position, separationRadius);
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            if (h.GetComponent<Enemy>() == null) continue;
            Vector2 away = rb.position - (Vector2)h.transform.position;
            float d = Mathf.Max(away.magnitude, 0.05f);
            push += away / d * (1f - d / separationRadius);
        }
        return push;
    }

    Transform ChooseTarget()
    {
        if (player != null)
        {
            var ph = player.GetComponent<Health>();
            bool playerAlive = ph != null && !ph.IsDead;
            if (playerAlive && Vector2.Distance(transform.position, player.position) <= detectRange)
                return player;
        }
        return castleTarget;
    }

    void TryAttack(Transform target)
    {
        if (Time.time < nextAttackTime) return;
        var h = target.GetComponent<Health>();
        if (h == null) return;
        h.TakeDamage(attackDamage);
        nextAttackTime = Time.time + attackCooldown;
    }

    void Die()
    {
        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject, 0.1f);
    }
}