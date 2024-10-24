using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestObservableValue : MonoBehaviour
{
    public FloatObservable valueToObserve;

    public UnityEvent ValueUpdated;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        valueToObserve.InitObservable();
        ValueUpdated.Invoke();
    }
}
