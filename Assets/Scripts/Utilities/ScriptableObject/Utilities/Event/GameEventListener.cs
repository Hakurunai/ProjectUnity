using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    
    [SerializeField] GameEvent Event;
    [SerializeField] SerializableDelegateNoParam Delegate;


    [SerializeField] SerializableDelegateOneParam<int> DelegateBool;

    [SerializeField] UnityEvent UEvent;

    protected void TestDelegate(int p_value = 5)
    {
        Debug.Log($"The parameter value is : {p_value}");
    }


    protected void Awake()
    {
        Delegate.InitDelegate();

        DelegateBool.SetCallBack("TestDelegate", this, this);
    }

    protected void OnEnable()
    {
        Event.RegisterListener(this);

        DelegateBool.Invoke(5);
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