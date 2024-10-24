using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestObservableValue : MonoBehaviour
{
    public FloatObservable valueToObserve;

    public UnityEvent ValueUpdated;

    public SerializableDelegateNoParam SerializableDelegate;

    public int PerfMeasureNbIteration = 100000;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        valueToObserve.InitObservable();
        ValueUpdated.Invoke();


        SerializableDelegate.InitDelegate();
        SerializableDelegate.Invoke();


        //float startTime;
        //float endTime;
        //valueToObserve.InitObservable();

        //startTime = Time.realtimeSinceStartup;

        //for (int i = 0; i < PerfMeasureNbIteration; ++i)
        //{
        //    ValueUpdated.Invoke();
        //}

        //endTime = Time.realtimeSinceStartup;
        //Debug.Log("Unity_Event execution time: " + (endTime - startTime) + " seconds");


        //SerializableDelegate.InitDelegate();

        //startTime = Time.realtimeSinceStartup;

        //for (int i = 0; i < PerfMeasureNbIteration; ++i)
        //{
        //    SerializableDelegate.Invoke();
        //}

        //endTime = Time.realtimeSinceStartup;
        //Debug.Log("Serializable_Delegate execution time: " + (endTime - startTime) + " seconds");


    }
}
