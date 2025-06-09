using System;
using UnityEngine;

public class PlayerCollect : MonoBehaviour
{
    private int _current;

    public Action<int> OnOrbCollected;

    public void Collect()
    {
        _current += 1;
        OnOrbCollected?.Invoke(_current);
    }
}
