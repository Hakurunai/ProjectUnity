using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] GameEvent Event;
    [SerializeField] SerializableDelegateNoParam Delegate;


    [SerializeField] SerializableDelegateOneParam<bool> DelegateBool;

    protected void TestDelegate(bool p_value = true)
    {
        Debug.Log($"The parameter value is : {p_value}");
    }


    protected void Awake()
    {
        Delegate.InitDelegate();

        DelegateBool.SetCallBack(TestDelegate);
    }

    protected void OnEnable()
    {
        Event.RegisterListener(this);

        DelegateBool.Invoke(false);
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