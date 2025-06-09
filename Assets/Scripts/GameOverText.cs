using TMPro;
using UnityEngine;

public class GameOverText : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TextMeshProUGUI text;

    private void OnEnable()
    {
        text.enabled = false;
        playerHealth.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        playerHealth.OnPlayerDied -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        text.text = "Game Over";
        text.enabled = true;
    }
}
