using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerDeath : MonoBehaviour
{
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        health.OnDied += Die;
    }

    void OnDisable()
    {
        health.OnDied -= Die;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) health.TakeDamage(10f);
    }

    void Die()
    {
        GetComponent<PlayerController>().enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<Animator>().SetFloat("Speed", 0f);
        Debug.Log("Öldün!");
    }
}