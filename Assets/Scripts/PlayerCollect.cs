using System;
using UnityEngine;

public class PlayerCollect : MonoBehaviour
{
    private int _current;

    public Action<int> OnCurrentChanged;

    public void Collect()
    {
        _current += 1;
        Debug.Log($"Collected {_current} orbs");
        OnCurrentChanged?.Invoke(_current);
    }
}
