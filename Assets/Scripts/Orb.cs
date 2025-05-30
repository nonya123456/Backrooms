using UnityEngine;

public class Orb : MonoBehaviour
{
    private bool _collected;

    private void OnTriggerEnter(Collider other)
    {
        if (_collected)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            var playerCollect = other.gameObject.GetComponent<PlayerCollect>();
            playerCollect.Collect();
            _collected = true;
            Destroy(gameObject);
        }
    }
}
