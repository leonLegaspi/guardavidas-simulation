using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    private Dictionary<string, IState> _states = new();
    private IState _currentState;
    private string _currentStateName;

    [Tooltip("Activar para ver los cambios de estado en la consola")]
    public bool debugLog = false;

    public void CreateState(string name, IState state)
    {
        if (!_states.ContainsKey(name))
            _states.Add(name, state);
    }

    public void UpdateFSM()
    {
        _currentState?.OnUpdate();
    }

    public void ChangeState(string name)
    {
        if (_currentStateName == name) return;

        if (!_states.ContainsKey(name))
        {
            Debug.LogWarning($"[FSM] Estado '{name}' no existe en {gameObject.name}");
            return;
        }

        if (debugLog)
            Debug.Log($"[FSM] {gameObject.name}: {_currentStateName} → {name}");

        _currentState?.OnExit();
        _currentState = _states[name];
        _currentStateName = name;
        _currentState.OnEnter();
    }

    public string GetCurrentState() => _currentStateName;
}

public interface IState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}