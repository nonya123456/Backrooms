using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void LateUpdate()
    {
        Reset();
    }

    public void Reset()
    {
        if (!target)
        {
            return;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
