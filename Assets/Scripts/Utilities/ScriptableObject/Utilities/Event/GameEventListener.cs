using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public GameEvent Event;
    public SerializableDelegateNoParam Delegate;
    public UnityEvent Response;


    private void Awake()
    {
        Delegate.InitDelegate();
    }

    private void OnEnable()
    {
        Event.RegisterListener(this);

    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        Delegate?.Invoke();
        Response.Invoke();
    }
}