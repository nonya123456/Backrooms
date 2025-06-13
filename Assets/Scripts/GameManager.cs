using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI overlayText;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerCollect playerCollect;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MonsterAI monsterAI;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private AudioSource audioSource;

    [Header("Game Settings")]
    [SerializeField] private int minMapLength = 8;
    [SerializeField] private int maxMapLength = 12;
    [SerializeField] private int minWaypointCount = 5;
    [SerializeField] private int maxWaypointCount = 8;
    [SerializeField] private int orbGoal = 10;
    private bool _isEnded;
    private int _showCount;

    [Header("Audio")]
    [SerializeField] private AudioClip textShowClip;
    [SerializeField] private AudioClip monsterFoundClip;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private IEnumerator Start()
    {
        SetMapConfig();
        mapGenerator.GenerateMap();
        yield return null;
        mapGenerator.BuildNavMesh();
        ShowOverlayText($"0/{orbGoal}");
        ResolvePlayerPosition();
    }

    private void ResolvePlayerPosition()
    {
        playerTransform.position = new Vector3(playerTransform.position.x, 0.5f, playerTransform.position.z);
    }

    private void SetMapConfig()
    {
        var seed = Random.Range(0, 10000);
        var width = Random.Range(minMapLength, maxMapLength + 1);
        var height = Random.Range(minMapLength, maxMapLength + 1);
        var waypointCount = Random.Range(minWaypointCount, maxWaypointCount + 1);

        mapGenerator.ResetConfig(seed, width, height, orbGoal, waypointCount);
    }

    private void OnEnable()
    {
        playerCollect.OnOrbCollected += HandleOrbCollected;
        playerHealth.OnPlayerDied += HandlePlayerDeath;
        monsterAI.OnStateChanged += HandleMonsterStateChanged;
        monsterAI.OnPlayerFound += PlayMonsterFoundClip;
    }

    private void OnDisable()
    {
        playerCollect.OnOrbCollected -= HandleOrbCollected;
        playerHealth.OnPlayerDied -= HandlePlayerDeath;
        monsterAI.OnStateChanged -= HandleMonsterStateChanged;
        monsterAI.OnPlayerFound -= PlayMonsterFoundClip;
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
            ShowOverlayText($"{current}/{orbGoal}");
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
        audioSource.PlayOneShot(textShowClip);
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

    private void PlayMonsterFoundClip()
    {
        audioSource.PlayOneShot(monsterFoundClip);
    }
}
