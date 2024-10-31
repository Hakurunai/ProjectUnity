using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] GameEvent Event;
    [SerializeField] SerializableDelegateNoParam Delegate;
    protected void Awake()
    {
        Delegate.InitDelegate();
    }

    protected void OnEnable()
    {
        Event.RegisterListener(this);

    }

    protected void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public virtual void OnEventRaised()
    {
        Delegate?.Invoke();
    }
}

public class GameEventListener<T> : GameEventListener
{
    [SerializeField] SerializableDelegateOneParam<T> Delegate;

    public override void OnEventRaised()
    {
        //Delegate?.Invoke();
    }
}