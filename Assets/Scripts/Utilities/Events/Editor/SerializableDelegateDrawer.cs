using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Collections;

[CustomPropertyDrawer(typeof(SerializableDelegateNoParam), true)] // 'true' applies to derived classes
public class SerializableDelegateDrawer : PropertyDrawer
{
    enum SelectionState
    {
        None = 0,
        TargetIsAScriptableObject,
        TargetIsAGameObject,
        UnusableType
    }

    public VisualTreeAsset m_InspectorXML;
    VisualElement root;
    VisualElement targetSection;

    Label delegateNameLabel;

    ObjectField targetSelector;
    UnityEngine.Object lastTargetValidValue;
    HelpBox targetSelectorHelp;

    DropdownField methodSelector;

    SelectionState selectionState = SelectionState.None;

    SerializedProperty methodOwner;


    public SerializableDelegateDrawer()
    {
        // Load your UXML asset. Ensure to use the correct path.
        m_InspectorXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Utilities/Events/UI Documents/SerializableDelegate.uxml");
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        root = m_InspectorXML.CloneTree();

        GetUxmlFields();
        BindProperties(property);
        RegisterCallback(property);
        SetDatas(property);

        DrawDefaultInspector(property);

        return root;
    }

    private void GetSerialiezedProperty(SerializedProperty p_property)
    {
        methodOwner = p_property.FindPropertyRelative("_methodOwner");
    }

    private void DrawDefaultInspector(SerializedProperty p_property)
    {
        // Get a reference to the default Inspector Foldout control.
        VisualElement InspectorFoldout = root.Q("Default_Inspector");
        // Create a property field for the default inspector representation
        var propertyField = new PropertyField(p_property);
        InspectorFoldout.Add(propertyField);
    }

    private void GetUxmlFields()
    {
        delegateNameLabel = root.Q<Label>("DelegateName");
        targetSelector = root.Q<ObjectField>("Target");
        targetSection = root.Q<VisualElement>("TargetSection");

        methodSelector = root.Q<DropdownField>("MethodSelector");
    }

    private void BindProperties(SerializedProperty p_property)
    {
        BindProperty(p_property, targetSelector, "_targetSelector");

        GetSerialiezedProperty(p_property);
    }



    private void BindProperty(SerializedProperty p_property, IBindable p_uiView, string p_propertyName)
    {
        var targetProperty = p_property.FindPropertyRelative(p_propertyName);
        if (targetProperty != null)
        {
            p_uiView.BindProperty(targetProperty);
        }
    }

    private void SetDatas(SerializedProperty p_property)
    {
        delegateNameLabel.text = p_property.displayName;

        //The bindings seems to no be totally correct at that point, so we can get the data through the SerializedProperty to avoid a null ref
        var targetSelec = p_property.FindPropertyRelative("_targetSelector");
        lastTargetValidValue = targetSelec.objectReferenceValue;
        if (targetSelec.objectReferenceValue == null)
            selectionState = SelectionState.None;
        else
            selectionState = targetSelec.objectReferenceValue is GameObject ? SelectionState.TargetIsAGameObject :
                             targetSelec.objectReferenceValue is ScriptableObject ? SelectionState.TargetIsAScriptableObject : SelectionState.UnusableType;

        ShowCompoAndMethodSelectors();
    }

    private void PopulateMethodDropDown()
    {
        Debug.Assert(targetSelector.value != null, $"Target selector {targetSelector} from property drawer of SerializableDelegate is null");
        switch(selectionState)
        {
            case SelectionState.TargetIsAGameObject:
                GenerateComponentsMethodDropDown();
                break;

            case SelectionState.TargetIsAScriptableObject:
                ScriptableObject scriptObj = (ScriptableObject)targetSelector.value;
                //TODO : GenerateScriptableMethodDropDown ?
                return;

            default: Debug.LogError($"Selection state {selectionState} is not implemented in PopulateMethodDropDown");
                return;
        }
    }

    private void GenerateComponentsMethodDropDown()
    {
        GameObject gameObject = (GameObject)targetSelector.value;
        Component[] components = gameObject.GetComponents<Component>();

        PopulateMethodSelectorChoices(components);
        //TODO : Set the current value as the value of the Selector
    }

    private void PopulateMethodSelectorChoices(Component[] p_components)
    {
        List<string> uniqueMethodsName = new List<string>();
        Dictionary<string, int> componentNameCounts = new Dictionary<string, int>();

        Type[] componentTypes = new Type[p_components.Length];
        for (int i = 0; i < p_components.Length; ++i)
        {
            componentTypes[i] = p_components[i].GetType();
        }

        string[] componentNames = GenerateComponentNames(p_components);

        for (int i = 0; i < p_components.Length; i++)
        {
            string[] methodNames = GenerateMethodNameFromType(componentTypes[i]);
            foreach (string method in methodNames)
            {
                uniqueMethodsName.Add($"{componentNames[i]}/{method}");
            }
        }
        methodSelector.choices = uniqueMethodsName;
    }

    //Arg : all components from the SAME GameObject
    private string[] GenerateComponentNames(Component[] p_components)
    {
        int size = p_components.Length;
        string[] componentNames = new string[size];
        Dictionary<string, int> componentNameCounts = new Dictionary<string, int>();

        for (int i = 0; i < size; ++i)
        {
            componentNames[i] = GenerateComponentName(p_components[i].GetType().Name, componentNameCounts);
        }
        return componentNames;
    }


    //Getting all the name of the methods through MethodInfo
    public static string[] GenerateMethodNameFromType(Type p_type)
    {
        MethodInfo[] methodInfos = p_type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                        .Where(m => m.ReturnType == typeof(void)
                                                    && AreParametersSerializableByUnity(m.GetParameters()))
                                        .ToArray();
        int size = methodInfos.Length;
        string[] methodNames = new string[size];
        for (int i = 0; i < size; ++i)
        {
            methodNames[i] = methodInfos[i].Name;
        }
        return methodNames;
    }

    public static string GenerateComponentName(in string p_componentDefaultName, Dictionary<string, int> p_componentNameCounts)
    {
        string componentName = p_componentDefaultName;

        //we will treat duplicate by adding a number to each of them
        if (!p_componentNameCounts.ContainsKey(componentName))
        {
            p_componentNameCounts[componentName] = 0; //first one is 0
        }
        else
        {
            p_componentNameCounts[componentName]++;
            componentName = $"{componentName}({p_componentNameCounts[componentName]})";
        }
        return componentName;
    }

    private static bool AreParametersSerializableByUnity(ParameterInfo[] parameters)
    {
        Type paramType = null;
        foreach (var param in parameters)
        {
            paramType = param.ParameterType;
            if (!IsSerializableByUnity(paramType))
            {
                return false;
            }
        }
        return true;
    }

    //caching those types for all instance of this drawer
    private static readonly HashSet<Type> builtInSerializableStruct = new HashSet<Type>
    {
        typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Vector2Int), typeof(Vector3Int), typeof(Matrix4x4), typeof(Quaternion),
        typeof(Rect), typeof(RectInt), typeof(Bounds), typeof(BoundsInt), typeof(Color), typeof(Color32), typeof(LayerMask)
    };

    public static bool IsSerializableByUnity(Type type)
    {
        // Check if it's a primitive, enum, or a Unity Object
        if (type.IsPrimitive || type.IsEnum || typeof(UnityEngine.Object).IsAssignableFrom(type))
            return true;
        
        if (builtInSerializableStruct.Contains(type))
            return true;

        // If it's an array : check if the element type of the array is serializable
        if (type.IsArray)
            return IsSerializableByUnity(type.GetElementType());

        if (typeof(IList).IsAssignableFrom(type) && type.IsGenericType)
        {
            // same thing for List<T>
            if (type.GetGenericTypeDefinition() == typeof(List<>))
                return IsSerializableByUnity(type.GetGenericArguments()[0]);
#if false
            // Optionally handle other generic collections like Dictionary<K,V>
            if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                return IsSerializableByUnity(keyType) && IsSerializableByUnity(valueType);
            }
#endif
        }
        // Check if it’s a custom class/struct marked with [Serializable] attribute
        if (type.IsClass || type.IsValueType)
            return Attribute.IsDefined(type, typeof(SerializableAttribute));

        return false;
    }

    private void ShowCompoAndMethodSelectors()
    {
        switch(selectionState)
        {
            case SelectionState.None:
                HideVisualElement(new VisualElement[] { methodSelector });
                break;

            case SelectionState.TargetIsAGameObject:
                goto case SelectionState.TargetIsAScriptableObject; //fall through

            case SelectionState.TargetIsAScriptableObject:
                ShowVisualElement(new VisualElement[] { methodSelector });
                break;

            case SelectionState.UnusableType:
                HideVisualElement(new VisualElement[] { methodSelector });
                break;

            default:
                throw new NotImplementedException();
        }
    }

    #region Callbacks
    private void RegisterCallback(SerializedProperty p_property)
    {
        targetSelector.RegisterValueChangedCallback(TargetSelectorValueChangedCallback);
        MethodSelectorValueChangedCallback(p_property);        
    }

    private void MethodSelectorValueChangedCallback(SerializedProperty p_property)
    {
        methodSelector.RegisterValueChangedCallback(evt =>
        {
            GameObject gameObject = (GameObject)targetSelector.value;
            Component[] components = gameObject.GetComponents<Component>();

            //TODO : Horrible switch here, need to refactor this after -> currently i need this to work properly
            switch(selectionState)
            {
                case SelectionState.TargetIsAGameObject :
                    // Find the selected object, evt.newValue format is Component/MethodName
                    // Component(1)/MethodName if a duplicate exist, number can increase
                    string[] splitName = evt.newValue.Split('/');
                    string componentName = splitName[0];
                    string methodName = splitName[1];


                    Debug.Assert(splitName.Length == 2, "Selected value did not respect the format : 'Component/MethodName'");


                    int selectedIndex = methodSelector.choices.IndexOf(evt.newValue);

                    //if (selectedIndex >= 0 && selectedIndex < components.Count())
                    //{
                    //    methodOwner.objectReferenceValue = components[selectedIndex];
                    //    p_property.serializedObject.ApplyModifiedProperties();
                    //}
                    break;
            }
            
        });
    }

    private void TargetSelectorValueChangedCallback(ChangeEvent<UnityEngine.Object> evt)
    {
        // Ensure HelpBox is created only once
        if (targetSelectorHelp == null)
        {
            targetSelectorHelp = new HelpBox("A target must be set.", HelpBoxMessageType.Warning);
            targetSection.Add(targetSelectorHelp);
        }

        IsTargetTypeValide(evt.newValue);

        switch(selectionState)
        {
            //We can group this two states together
            case SelectionState.TargetIsAGameObject : 
                goto case SelectionState.TargetIsAScriptableObject; //fall through

            case SelectionState.TargetIsAScriptableObject:
                HideVisualElement(new VisualElement[] { targetSelectorHelp });
                PopulateMethodDropDown();

                lastTargetValidValue = evt.newValue;
                break;

            case SelectionState.None:
                ShowVisualElement(new VisualElement[] { targetSelectorHelp });
                lastTargetValidValue = null;                
                break;

            case SelectionState.UnusableType:
                Debug.LogWarning($"{evt.newValue.GetType()} is an invalid type! Please assign a GameObject from the scene or a ScriptableObject. " +
                $"Reverting to the last previous valid value.");
                targetSelector.value = lastTargetValidValue;
                break;
        }

        ShowCompoAndMethodSelectors();
    }
    #endregion Callbacks

    private void HideVisualElement(VisualElement[] p_elements)
    {
        foreach (VisualElement element in p_elements)
        {
            element.visible = false;
            element.style.display = DisplayStyle.None;
        }
    }

    private void ShowVisualElement(VisualElement[] p_elements)
    {
        foreach (VisualElement element in p_elements)
        {
            element.visible = true;
            element.style.display = DisplayStyle.Flex;
        }
    }

    private void IsTargetTypeValide(UnityEngine.Object p_NewValue)
    {
        if (p_NewValue == null)
        {
            selectionState = SelectionState.None;
            return;
        }

        if (p_NewValue is GameObject)
        {
            selectionState = SelectionState.TargetIsAGameObject;
        }
        else if (p_NewValue is ScriptableObject)
        {
            selectionState = SelectionState.TargetIsAScriptableObject;
        }
        else
            selectionState = SelectionState.UnusableType;
    }
}