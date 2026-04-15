using UnityEngine;

public class HelloCuisto : MonoBehaviour
{
    public float speed = 1f;
    private const float RotationA = 81.436f;
    private const float RotationB = 53.26f;

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f);
        float z = Mathf.Lerp(RotationA, RotationB, t);
        transform.localRotation = Quaternion.Euler(-62.816f, -218.957f, z);
    }
}
