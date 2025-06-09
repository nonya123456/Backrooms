using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI overlayText;
    [SerializeField] private PlayerCollect playerCollect;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MonsterAI monsterAI;

    [Header("Game Settings")]
    [SerializeField] private int orbGoal = 5;
    private int _justCollectedCount;
    private bool _isEnded;

    private void OnEnable()
    {
        playerCollect.OnOrbCollected += HandleOrbCollected;
        playerHealth.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        playerCollect.OnOrbCollected -= HandleOrbCollected;
        playerHealth.OnPlayerDied -= HandlePlayerDeath;
    }

    private void Update()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (_isEnded)
        {
            overlayText.enabled = true;
            return;
        }

        overlayText.enabled = _justCollectedCount > 0;
    }

    private void HandleOrbCollected(int current)
    {
        if (_isEnded)
        {
            return;
        }

        if (current >= orbGoal)
        {
            overlayText.text = "You Win";
            _isEnded = true;
            Pause();
        }
        else
        {
            overlayText.text = $"{current} / {orbGoal}";
            StartCoroutine(ShowOrbCountCoroutine());
        }
    }

    private IEnumerator ShowOrbCountCoroutine()
    {
        _justCollectedCount += 1;
        yield return new WaitForSeconds(1f);
        _justCollectedCount -= 1;
    }

    private void HandlePlayerDeath()
    {
        if (_isEnded)
        {
            return;
        }

        overlayText.text = "Game Over";
        _isEnded = true;
        Pause();
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        playerController.enabled = false;
        monsterAI.enabled = false;
    }
}
