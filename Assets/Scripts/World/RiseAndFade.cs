using UnityEngine;

public class RiseAndFade : MonoBehaviour
{
    public float riseSpeed = 0.5f;
    public float life = 1.5f;

    SpriteRenderer sr;
    float age;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        var c = sr.color;
        c.a = Mathf.Clamp01(1f - age / life);
        sr.color = c;
    }
}
