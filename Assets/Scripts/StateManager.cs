using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }
    public State CurrentState { get; set; }
    public static event Action<State> OnStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateState(State newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}