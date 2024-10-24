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
public class SerializableDelegateNoParam
{
    [SerializeField] UnityEngine.Object _target;
    [SerializeField] Component _component;
    [SerializeField] string _methodName;

    private Action _cachedDelegate;

    public void InitDelegate()
    {
        if (_target == null || string.IsNullOrEmpty(_methodName))
        {
            Debug.LogError($"Target field and/or MethodName provided is null.");
            return;
        }
        MethodInfo methodInfo;

        if (_target is ScriptableObject scriptableObject)
        {
            methodInfo = scriptableObject.GetType().GetMethod(_methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                Debug.LogError($"Method '{_methodName}' not found on target '{_target}'.");
                return;
            }
            _cachedDelegate = (Action)Delegate.CreateDelegate(typeof(Action), scriptableObject, methodInfo);
        }
        else if (_component != null)
        {
            methodInfo = _component.GetType().GetMethod(_methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                Debug.LogError($"Method '{_methodName}' not found on target '{_target}'.");
                return;
            }
            _cachedDelegate = (Action)Delegate.CreateDelegate(typeof(Action), _component, methodInfo);
        }
    }

    public void Invoke()
    {
        _cachedDelegate?.Invoke();
    }
}