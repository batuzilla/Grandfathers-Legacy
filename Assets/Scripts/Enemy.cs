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

    public Transform castleTarget;

    Transform player;
    Health health;
    Rigidbody2D rb;
    SpriteRenderer sr;
    float nextAttackTime;

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
        float dist = toTarget.magnitude;

        if (dist > attackRange)
        {
            Vector2 dir = toTarget.normalized;
            rb.linearVelocity = dir * moveSpeed;
            if (sr != null && Mathf.Abs(dir.x) > 0.01f) sr.flipX = dir.x < 0;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack(target);
        }
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