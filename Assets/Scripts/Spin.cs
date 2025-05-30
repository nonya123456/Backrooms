using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] private float speed = 45f;

    private void Update()
    {
        transform.Rotate(Vector3.one, speed * Time.deltaTime);
    }
}
