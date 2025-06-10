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
    private bool _isEnded;
    private int _showCount;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        playerCollect.OnOrbCollected += HandleOrbCollected;
        playerHealth.OnPlayerDied += HandlePlayerDeath;
        monsterAI.OnStateChanged += HandleMonsterStateChanged;
    }

    private void OnDisable()
    {
        playerCollect.OnOrbCollected -= HandleOrbCollected;
        playerHealth.OnPlayerDied -= HandlePlayerDeath;
        monsterAI.OnStateChanged -= HandleMonsterStateChanged;
    }

    private void Update()
    {
        overlayText.enabled = _showCount > 0 || _isEnded;
    }

    private void HandleOrbCollected(int current)
    {
        if (_isEnded)
        {
            return;
        }

        if (current >= orbGoal)
        {
            EndGame("YOU WIN");
        }
        else
        {
            ShowOverlayText($"{current} / {orbGoal}");
        }
    }

    private void HandlePlayerDeath()
    {
        if (_isEnded)
        {
            return;
        }

        EndGame("YOU DIED");
    }

    private void HandleMonsterStateChanged(MonsterAI.State state)
    {
        if (_isEnded)
        {
            return;
        }

        if (state is MonsterAI.State.Chasing)
        {
            ShowOverlayText("RUN");
        }
    }

    private void ShowOverlayText(string text)
    {
        overlayText.text = text;
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        _showCount += 1;
        yield return new WaitForSeconds(1f);
        _showCount -= 1;
    }

    private void EndGame(string text)
    {
        overlayText.text = text;
        _isEnded = true;

        playerController.enabled = false;
        monsterAI.enabled = false;

        StartCoroutine(LoadMainMenu());
    }

    private static IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainMenuScene");
    }
}
