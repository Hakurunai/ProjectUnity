using System;
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

    public event Action OnEvent;

    public delegate void CSharpDelegate();
    public CSharpDelegate MyCustomDelegate;

    void Init()
    {
        valueToObserve.InitObservable();
        ValueUpdated.Invoke();

        SerializableDelegate.InitDelegate();
        SerializableDelegate.Invoke();

        //CheckPerf();        
    }

    void CheckPerf()
    {
        double startTime;
        double endTimeUnityEvent;
        double endTimeSerializableDelegate;
        double endTimeCSharpDelegate;
        double endTimeCSharpEvent;
        valueToObserve.InitObservable();

        startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < PerfMeasureNbIteration; ++i)
        {
            ValueUpdated.Invoke();
        }

        endTimeUnityEvent = Time.realtimeSinceStartup - startTime;
        Debug.Log("Unity_Event execution time: " + endTimeUnityEvent + " seconds");


        SerializableDelegate.InitDelegate();

        startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < PerfMeasureNbIteration; ++i)
        {
            SerializableDelegate.Invoke();
        }

        endTimeSerializableDelegate = Time.realtimeSinceStartup - startTime;
        Debug.Log("Serializable_Delegate execution time: " + endTimeSerializableDelegate + " seconds");


        startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < PerfMeasureNbIteration; ++i)
        {
            MyCustomDelegate?.Invoke();
        }

        endTimeCSharpDelegate = Time.realtimeSinceStartup - startTime;
        Debug.Log("CSharp_Delegate execution time: " + endTimeCSharpDelegate + " seconds");



        startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < PerfMeasureNbIteration; ++i)
        {
            OnEvent?.Invoke();
        }

        endTimeCSharpEvent = Time.realtimeSinceStartup - startTime;
        Debug.Log("CSharp_Event execution time: " + endTimeCSharpEvent + " seconds");


        double resVsUnityEvent = endTimeUnityEvent / endTimeSerializableDelegate;
        double resVsCSharpEvent = endTimeSerializableDelegate / endTimeCSharpEvent;
        double resVsCSharpDelegate = endTimeSerializableDelegate / endTimeCSharpDelegate;
        double resCSharpDelegateVsUnityEvent = endTimeUnityEvent / endTimeCSharpDelegate;

        Debug.Log($"Serializable_Delegate take {resVsUnityEvent} less time to execute than UNITY_Event.");
        Debug.Log($"Serializable_Delegate take {resVsCSharpEvent} more time to execute than CSharp_EVENT.");
        Debug.Log($"Serializable_Delegate take {resVsCSharpDelegate} more time to execute than CSharp_DELEGATE.");
        Debug.Log($"Comparison : UNITY_Event take {resCSharpDelegateVsUnityEvent} more time to execute than CSharp_DELEGATE.");
    }
}