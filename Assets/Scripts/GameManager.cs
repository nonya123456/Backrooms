using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerCollect playerCollect;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MonsterAI monsterAI;

    private void OnEnable()
    {
        playerHealth.OnPlayerDied += Pause;
        playerCollect.OnGoalReached += Pause;
    }

    private void OnDisable()
    {
        playerHealth.OnPlayerDied -= Pause;
        playerCollect.OnGoalReached -= Pause;
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        playerController.enabled = false;
        monsterAI.enabled = false;
    }
}
