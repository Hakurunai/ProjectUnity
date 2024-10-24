using System;
using UnityEngine;

[System.Serializable]
public class FloatObservable
{
    [SerializeField] private FloatReference Observer;
    [SerializeField] private float _value;

    //This is a reminder that we need to initialize this type of variable for them
    //to work properly. We could call this method or set directly Value 
    public void InitObservable()
    {
        Value = _value;
    }

    public void InitObservable(float p_Value)
    {
        Value = p_Value;
    }

    public float Value
    {
        get { return _value; }
        set 
        { 
            _value = value;
            Observer.Value = value;
        }
    }
}