using System;
using UnityEngine;

public class PlayerCollect : MonoBehaviour
{
    [SerializeField] private int goal = 5;

    private int _current;

    public Action<int> OnCurrentChanged;
    public Action OnGoalReached;

    public void Collect()
    {
        _current += 1;
        OnCurrentChanged?.Invoke(_current);

        if (_current >= goal)
        {
            OnGoalReached?.Invoke();
        }
    }
}
