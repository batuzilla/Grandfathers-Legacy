using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 20f;
    public float lifetime = 3f;
    public float spriteAngleOffset = 0f;

    Rigidbody2D rb;

    public void Launch(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (other.CompareTag("Player")) return;

        if (other.GetComponent<WallSegment>() != null)
        {
            Destroy(gameObject);
            return;
        }

        var health = other.GetComponent<Health>();
        if (health != null) health.TakeDamage(damage);

        Destroy(gameObject);
    }
}