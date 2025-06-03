using UnityEngine;

public class ToggleRenderer : MonoBehaviour
{
    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void EnableRenderers()
    {
        foreach (var r in _renderers)
        {
            r.enabled = true;
        }
    }

    public void DisableRenderers()
    {
        foreach (var r in _renderers)
        {
            r.enabled = false;
        }
    }
}
