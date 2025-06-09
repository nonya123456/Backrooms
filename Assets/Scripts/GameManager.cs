using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

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
            overlayText.text = "YOU WIN";
            _isEnded = true;
            Pause();
            StartCoroutine(LoadMainMenu());
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

        overlayText.text = "YOU DIED";
        _isEnded = true;
        Pause();
        StartCoroutine(LoadMainMenu());
    }

    private void Pause()
    {
        playerController.enabled = false;
        monsterAI.enabled = false;
    }

    private static IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainMenuScene");
    }
}
