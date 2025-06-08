using System.Collections;
using UnityEngine;

public class MonsterEffect : MonoBehaviour
{
    [SerializeField] private AnimationCurve shrinkCurve;
    [SerializeField] private float shrinkDuration = 0.4f;
    private Renderer[] _renderers;

    private static readonly int ShrinkCenter = Shader.PropertyToID("_Shrink_Center");
    private static readonly int ShrinkMultiplier = Shader.PropertyToID("_Shrink_Multiplier");

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void Teleport()
    {
        StopAllCoroutines();
        StartCoroutine(TeleportCoroutine());
    }

    public void Show()
    {
        SetShrink(Vector3.zero, 1f);

        foreach (var r in _renderers)
        {
            r.enabled = true;
        }
    }

    public void Hide()
    {
        foreach (var r in _renderers)
        {
            r.enabled = false;
        }
    }

    private IEnumerator TeleportCoroutine()
    {
        var center = GetShrinkCenter();
        var elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            var multiplier = shrinkCurve.Evaluate(elapsedTime / shrinkDuration);
            SetShrink(center, multiplier);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Hide();
    }

    private Vector3 GetShrinkCenter()
    {
        var vertical = transform.up * 2.0f;
        var horizontal = transform.right * Random.Range(-5f, 5f);
        return transform.position + vertical + horizontal;
    }

    private void SetShrink(Vector3 center, float multiplier)
    {
        foreach (var r in _renderers)
        {
            r.material.SetVector(ShrinkCenter, center);
            r.material.SetFloat(ShrinkMultiplier, multiplier);
        }
    }
}
