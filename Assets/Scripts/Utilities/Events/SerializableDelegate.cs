using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
}

[Serializable]
public class SerializableDelegateNoParam : SerializableDelegateBase
{
    private Action _cachedDelegate;
    
    public override void InitDelegate()
    {
        if (!CheckMethodeSearchingInformations()) return;

        MethodInfo methodInfo;
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        methodInfo = _methodOwner.GetType().GetMethod(_methodName, flags);
        if (methodInfo == null)
        {
            Debug.LogError($"Method '{_methodName}' not found on target '{_methodOwner}'.");
            return;
        }
        _cachedDelegate = (Action)Delegate.CreateDelegate(typeof(Action), _methodOwner, methodInfo);
    }

    public void Invoke()
    {
        _cachedDelegate?.Invoke();
    }
}

[Serializable]
public class SerializableDelegateOneParam<T> : SerializableDelegateBase
{
    private Action<T> _cachedDelegate;

    public void SetCallBack(Action<T> p_CallBack)
        { _cachedDelegate = p_CallBack; }

    public override void InitDelegate()
    {
        if (!CheckMethodeSearchingInformations()) return;

        MethodInfo methodInfo;
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        methodInfo = _methodOwner.GetType().GetMethod(_methodName, flags, null, new Type[] { typeof(T) }, null);
        if (methodInfo == null)
        {
            Debug.LogError($"Method '{_methodName}' not found on target '{_methodOwner}'.");
            return;
        }
        _cachedDelegate = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), _methodOwner, methodInfo);
    }

    public void Invoke(T p_Value)
    {
        _cachedDelegate.Invoke(p_Value);
    }
}