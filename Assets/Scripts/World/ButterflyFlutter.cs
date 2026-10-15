using UnityEngine;

public class ButterflyFlutter : MonoBehaviour
{
    public float speed = 1.2f;
    public float roamRadius = 6f;
    public Vector2 turnTime = new Vector2(0.6f, 1.8f);

    Vector2 home;
    Vector2 dir;
    float nextTurn;

    void Start()
    {
        home = transform.position;
        Turn();
    }

    void Update()
    {
        if (Time.time >= nextTurn) Turn();
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    void Turn()
    {
        nextTurn = Time.time + Random.Range(turnTime.x, turnTime.y);
        Vector2 pos = transform.position;
        Vector2 toHome = home - pos;
        dir = toHome.magnitude > roamRadius ? toHome.normalized : Random.insideUnitCircle.normalized;
    }
}
