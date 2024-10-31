using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Event/GameEvent")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> _listeners = new List<GameEventListener>();

    public void Raise()
    {
        for (int i = _listeners.Count - 1; i >= 0; --i)
        {
            _listeners[i].OnEventRaised();
        }
    }

    public void RegisterListener(GameEventListener p_Listener)
    {
        _listeners.Add(p_Listener);
    }

    public void UnregisterListener(GameEventListener p_Listener)
    {
        _listeners.Remove(p_Listener);
    }
}