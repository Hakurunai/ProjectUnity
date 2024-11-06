using System;
using System.Reflection;
using UnityEngine;

//[Serializable]
//public class SerializableDelegate
//{
//    [SerializeField] UnityEngine.Object Target;  // Target GameObject or ScriptableObject
//    [SerializeField] Component Component;        // Selected component (if GameObject)
//    [SerializeField] string MethodName;          // Name of the method to be invoked
//    [SerializeField] object[] Parameters;                  // Parameters for the method (if applicable)

//    private Delegate _cachedDelegate;

//    public void InitDelegate()
//    {
//        if (Target == null || string.IsNullOrEmpty(MethodName))
//        {
//            Debug.LogError($"Target field and/or MethodName provided is null.");
//            return;
//        }

//        UnityEngine.Object targetObject = Target is ScriptableObject ? Target : Component;
//        CacheMethodFromObject(targetObject);
//    }

//    // Cache the method to improve performance at runtime.
//    // Basically, we want to avoid calling DynamicInvoke([]params) and prefer Invoke(p1, p2...) instead
//    // For that, we need to determine, create and store the correct delegate type
//    private void CacheMethodFromObject(UnityEngine.Object targetObject)
//    {
//        MethodInfo methodInfo = targetObject.GetType().GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//        if (methodInfo == null)
//        {
//            Debug.LogError($"Method '{MethodName}' not found on target '{Target}'.");
//            return;
//        }

//        Type[] parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();


//        // Create the correct delegate type based on parameters, we assume that return is always void
//        Type delegateType;

//        if (parameterTypes.Length == 0)
//        {
//            delegateType = typeof(Action);
//        }
//        else
//        {
//            delegateType = Expression.GetActionType(parameterTypes);
//        }

//        // Create the strongly-typed delegate
//        _cachedDelegate = Delegate.CreateDelegate(delegateType, targetObject, methodInfo);
//    }

//    public void Invoke()
//    {
//        // Use the correct invoke method based on the number of parameters
//        if (_cachedDelegate == null)
//        {
//            Debug.LogError("Cached delegate is null. Ensure InitDelegate has been called.");
//            return;
//        }

//        // Cast and invoke the delegate based on the number of parameters
//        switch (Parameters.Length)
//        {
//            case 0:
//                ((Action)_cachedDelegate).Invoke();
//                break;
//            case 1:
//                ((Action<object>)_cachedDelegate).Invoke(Parameters[0]);
//                break;
//            case 2:
//                ((Action<object, object>)_cachedDelegate).Invoke(Parameters[0], Parameters[1]);
//                break;
//            case 3:
//                ((Action<object, object, object>)_cachedDelegate).Invoke(Parameters[0], Parameters[1], Parameters[2]);
//                break;
//            case 4:
//                ((Action<object, object, object, object>)_cachedDelegate).Invoke(Parameters[0], Parameters[1], Parameters[2], Parameters[3]);
//                break;
//            default:
//                Debug.LogError("Too many parameters. DynamicInvoke fallback required.");
//                _cachedDelegate.DynamicInvoke(Parameters);
//                break;
//        }
//    }
//}

[Serializable]
public abstract class SerializableDelegateBase
{
    [SerializeField, Tooltip("Work with GameObject and ScriptableObject")] protected UnityEngine.Object _targetSelector;
    [SerializeField] protected UnityEngine.Object _methodOwner; //this value is setted via the drawer of the class
    [SerializeField] protected string _methodName; //this value is setted via the drawer of the class

    public abstract void InitDelegate();

    protected bool CheckMethodeSearchingInformations()
    {
        if (_targetSelector == null || string.IsNullOrEmpty(_methodName) || _methodOwner == null)
        {
            Debug.LogError($"Target field and/or MethodName is incorrect.");
            return false;
        }
        return true;
    }


    public static MethodInfo GenerateMethodInfo(UnityEngine.Object p_MethodOwner, in string p_MethodName, SerializableDelegateBase p_Delegate)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Type[] argumentTypes = p_Delegate.GetType().IsGenericType ?
            p_Delegate.GetType().GetGenericArguments() : Type.EmptyTypes;

        MethodInfo method = p_MethodOwner.GetType().GetMethod(p_MethodName, flags, null, argumentTypes, null);
        Debug.Assert(method != null, $"Method '{p_MethodName}' not found on target '{p_MethodOwner}'.");

        return method;
    }

    private static Type GenerateGenericDelegateType(SerializableDelegateBase p_Delegate)
    {
        Type[] genericTypes = p_Delegate.GetType().GetGenericArguments();
        return typeof(Action<>).MakeGenericType(genericTypes);
    }

    public static Action<T1> GenerateDelegate<T1>(MethodInfo p_MethodInfo, UnityEngine.Object p_MethodOwner, SerializableDelegateBase p_Delegate)
    {
        Type delegateType = GenerateGenericDelegateType(p_Delegate);
        return (Action<T1>)Delegate.CreateDelegate(delegateType, p_MethodOwner, p_MethodInfo);
    }

    public static Action<T1,T2> GenerateDelegate<T1, T2>(MethodInfo p_MethodInfo, UnityEngine.Object p_MethodOwner, SerializableDelegateBase p_Delegate)
    {
        Type delegateType = GenerateGenericDelegateType(p_Delegate);
        return (Action<T1,T2>)Delegate.CreateDelegate(delegateType, p_MethodOwner, p_MethodInfo);
    }

    public static Action<T1, T2, T3> GenerateDelegate<T1, T2, T3>(MethodInfo p_MethodInfo, UnityEngine.Object p_MethodOwner, SerializableDelegateBase p_Delegate)
    {
        Type delegateType = GenerateGenericDelegateType(p_Delegate);
        return (Action<T1, T2, T3>)Delegate.CreateDelegate(delegateType, p_MethodOwner, p_MethodInfo);
    }

    public static Action<T1, T2, T3, T4> GenerateDelegate<T1, T2, T3, T4>(MethodInfo p_MethodInfo, UnityEngine.Object p_MethodOwner, SerializableDelegateBase p_Delegate)
    {
        Type delegateType = GenerateGenericDelegateType(p_Delegate);
        return (Action<T1, T2, T3, T4>)Delegate.CreateDelegate(delegateType, p_MethodOwner, p_MethodInfo);
    }
}

[Serializable]
public class SerializableDelegateNoParam : SerializableDelegateBase
{
    private Action _cachedDelegate;
    
    public override void InitDelegate()
    {
        if (!CheckMethodeSearchingInformations()) return;

        MethodInfo methodInfo = SerializableDelegateBase.GenerateMethodInfo(_methodOwner, _methodName, this);
        _cachedDelegate = (Action)Delegate.CreateDelegate(typeof(Action), _methodOwner, methodInfo);
    }

    public void Invoke()
    {
        if (_cachedDelegate.Target == null)
        {
            Debug.LogError($"Trying to launch an action from a null object. Owner null : {_methodOwner}, method name : {_methodName}");
            return;
        }
        _cachedDelegate?.Invoke();
    }
}

[Serializable]
public class SerializableDelegateOneParam<T> : SerializableDelegateBase
{
    private Action<T> _cachedDelegate;

    public void SetCallBack(Action<T> p_CallBack)
    { _cachedDelegate = p_CallBack; }

    public void SetCallBack(string p_MethodName, UnityEngine.Object p_target, UnityEngine.Object p_methodOwner)
    {
        _targetSelector = p_target;
        _methodOwner = p_methodOwner;
        _methodName = p_MethodName;

        InitDelegate();
    }

    public override void InitDelegate()
    {
        if (!CheckMethodeSearchingInformations()) return;

        MethodInfo methodInfo = SerializableDelegateBase.GenerateMethodInfo(_methodOwner, _methodName, this);
        Debug.Assert(methodInfo != null, $"Method '{_methodName}' not found on target '{_methodOwner}'.");

        _cachedDelegate = SerializableDelegateBase.GenerateDelegate<T>(methodInfo, _methodOwner, this);
    }

    public void Invoke(T p_Value)
    {
        if (_cachedDelegate.Target == null)
        {
            Debug.LogError($"Trying to launch an action from a null object. Owner null : {_methodOwner}, method name : {_methodName}");
            return;
        }
        _cachedDelegate.Invoke(p_Value);
    }
}