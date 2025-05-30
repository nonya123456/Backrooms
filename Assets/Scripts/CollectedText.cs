using System.Collections;
using TMPro;
using UnityEngine;

public class CollectedText : MonoBehaviour
{
    [SerializeField] private PlayerCollect playerCollect;
    [SerializeField] private TextMeshProUGUI text;
    private int _showCount;

    private void Awake()
    {
        text.enabled = false;
        playerCollect.OnCurrentChanged += OnCurrentChanged;
    }

    private void OnDestroy()
    {
        playerCollect.OnCurrentChanged -= OnCurrentChanged;
    }

    private void Update()
    {
        text.enabled = _showCount > 0;
    }

    private void OnCurrentChanged(int current)
    {
        text.text = $"{current}";
        StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        _showCount += 1;
        yield return new WaitForSeconds(1f);
        _showCount -= 1;
    }
}
