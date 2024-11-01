using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(SerializableDelegateNoParam), true)] // 'true' applies to derived classes
public class SerializableDelegateDrawer : PropertyDrawer
{
    public VisualTreeAsset m_InspectorXML;
    
    public SerializableDelegateDrawer()
    {
        // Load your UXML asset. Ensure to use the correct path.
        m_InspectorXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Utilities/Events/UI Documents/SerializableDelegate.uxml");
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = m_InspectorXML.CloneTree();

        
        var delegateName = root.Q<Label>("DelegateName");
        var target = root.Q<ObjectField>("Target");
        var errorBox = root.Q<VisualElement>("TargetErrorBox");


        delegateName.text = property.displayName;
        target.BindProperty(property.FindPropertyRelative("_targetSelector"));


        target.RegisterValueChangedCallback(evt =>
        {
            if (target.value != null)
            {
                Debug.Log("A value is assigned");
                errorBox.style.display = DisplayStyle.None;
            }
            else
            {
                Debug.Log("Something is wrong");
                errorBox.style.display = DisplayStyle.Flex;
            }
        });


        // Get a reference to the default Inspector Foldout control.
        VisualElement InspectorFoldout = root.Q("Default_Inspector");
        // Create a property field for the default inspector representation
        var propertyField = new PropertyField(property);
        InspectorFoldout.Add(propertyField);        

        return root;
    }


    private bool IsTargetTypeValide(UnityEngine.Object p_NewValue)
    {
        var gameObjTest = p_NewValue as GameObject;
        var scriptableTest = p_NewValue as ScriptableObject;

        if (!(gameObjTest != null || scriptableTest != null))
        {
            Debug.LogWarning($"{p_NewValue.GetType()} is an invalid type ! Please assign a GameObject" +
            $" from the scene or a ScriptableObject.");
            return false;
        }

        MonoBehaviour mono = p_NewValue as MonoBehaviour;
        MonoScript monoScript = MonoScript.FromMonoBehaviour(mono);

        if (PrefabUtility.IsPartOfPrefabAsset(p_NewValue) || !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(monoScript)))
        {
            Debug.LogWarning($"{p_NewValue.GetType()} is an invalid type ! Please assign a GameObject" +
            $" from the scene or a ScriptableObject.");
            return false;
        }
        return true;
    }
    

    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    // Start drawing properties
    //    EditorGUI.BeginProperty(position, label, property);
    //    var indent = EditorGUI.indentLevel;
    //    EditorGUI.indentLevel = 1;

    //    // Get the target object and method name fields
    //    SerializedProperty targetSelectorProperty = property.FindPropertyRelative("_targetSelector");
    //    SerializedProperty methodOwnerProperty = property.FindPropertyRelative("_methodOwner");
    //    SerializedProperty methodNameProperty = property.FindPropertyRelative("_methodName");

    //    Color LineBlack = new Color(0.102f, 0.102f, 0.102f);
    //    Color BGDarkGrey = new Color(0.207f, 0.207f, 0.207f);
    //    Color BGLightGrey = new Color(0.274f, 0.274f, 0.274f);

    //    float yOffset = 0f;
    //    float yLargeSeparator = 6f;
    //    float yLittleSeparator = 3f;

    //    // Draw the label (variable name)
    //    Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
    //    yOffset += labelRect.height;
    //    EditorGUI.DrawRect(labelRect, BGDarkGrey);
    //    EditorGUI.LabelField(labelRect, label);

    //    // Create a box around the property similar to UnityEvents
    //    Rect boxRect = new Rect(labelRect.x, yOffset, labelRect.width, GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight * 2);
    //    EditorGUI.DrawRect(boxRect, BGLightGrey); // Background color of the box

    //    EditorGUI.DrawRect(new Rect(labelRect.x, labelRect.y, position.width, 1), LineBlack); // Top line
    //    EditorGUI.DrawRect(new Rect(labelRect.x, labelRect.y + +EditorGUIUtility.singleLineHeight, position.width, 1), LineBlack); // VariableNameSeparatorLine
    //    EditorGUI.DrawRect(new Rect(boxRect.x, boxRect.y + boxRect.height - 1, boxRect.width, 1), LineBlack); // Bottom line
    //    EditorGUI.DrawRect(new Rect(labelRect.x, labelRect.y, 1, labelRect.height + boxRect.height), LineBlack); // Left line
    //    EditorGUI.DrawRect(new Rect(labelRect.x + labelRect.width - 1, labelRect.y, 1, labelRect.height + boxRect.height), LineBlack); // Right line


    //    // Draw the target field
    //    yOffset += yLargeSeparator;
    //    Rect targetRect = new Rect(boxRect.x, yOffset, boxRect.width, EditorGUIUtility.singleLineHeight);
    //    EditorGUI.PropertyField(targetRect, targetSelectorProperty);
    //    UnityEngine.Object targetObject = targetSelectorProperty.objectReferenceValue;

    //    if (targetObject == null)
    //    {
    //        EndPropertyDrawing(indent);
    //        return;
    //    }
    //    // If target is a GameObject, show a component dropdown
    //    if (targetObject is GameObject)
    //    {
    //        GameObject gameObject = (GameObject)targetObject;
    //        Component[] components = gameObject.GetComponents<Component>();
    //        string[] componentNames = components.Select(c => c.GetType().Name).ToArray();

    //        // Find the currently selected component index
    //        Component selectedComponent = methodOwnerProperty.objectReferenceValue as Component;
    //        int selectedIndex = selectedComponent != null ? Array.IndexOf(components, selectedComponent) : 0;

    //        // Draw component dropdown
    //        yOffset += EditorGUIUtility.singleLineHeight + yLittleSeparator;
    //        Rect componentRect = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
    //        selectedIndex = EditorGUI.Popup(componentRect, "Component", selectedIndex, componentNames);

    //        if (selectedIndex >= 0 && selectedIndex < components.Length)
    //        {
    //            yOffset += EditorGUIUtility.singleLineHeight + yLittleSeparator;
    //            // If a component is selected, show method dropdown
    //            Component component = components[selectedIndex];
    //            DrawMethodDropdown(position, yOffset, component.GetType(), methodNameProperty);

    //            // Update the owner property field with the selected component
    //            methodOwnerProperty.objectReferenceValue = components[selectedIndex];
    //        }
    //    }
    //    // If target is a ScriptableObject, show method dropdown directly
    //    else if (targetObject is ScriptableObject)
    //    {
    //        yOffset += EditorGUIUtility.singleLineHeight + yLittleSeparator;

    //        ScriptableObject scriptableObject = (ScriptableObject)targetObject;
    //        DrawMethodDropdown(position, yOffset, scriptableObject.GetType(), methodNameProperty);

    //        methodOwnerProperty.objectReferenceValue = scriptableObject;
    //    }
    //    EndPropertyDrawing(indent);
    //}

    private void DrawMethodDropdown(Rect position, float yOffset, Type type, SerializedProperty methodNameProperty)
    {
        // Get all valid methods (void return type, no parameters)
        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                   .Where(m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void))
                                   .ToArray();

        string[] methodNames = methods.Select(m => m.Name).ToArray();

        // Find the currently selected method
        string currentMethodName = methodNameProperty.stringValue;
        int selectedIndex = Array.IndexOf(methodNames, currentMethodName);
        if (selectedIndex == -1) selectedIndex = 0; // Default to the first method if not found

        // Draw the method dropdown
        Rect methodRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
        selectedIndex = EditorGUI.Popup(methodRect, "Method", selectedIndex, methodNames);

        // Update the methodName property with the selected method
        if (selectedIndex >= 0 && selectedIndex < methodNames.Length)
        {
            methodNameProperty.stringValue = methodNames[selectedIndex];
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty targetProperty = property.FindPropertyRelative("_targetSelector");
        UnityEngine.Object targetObject = targetProperty.objectReferenceValue;
        
        if (targetObject is GameObject)
        {
            return EditorGUIUtility.singleLineHeight * 4 + 36; // Height for label, target, component, and method dropdowns, plus padding
        }
        else if (targetObject is ScriptableObject)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 32; // Height for label and method dropdowns, plus padding
        }
        else
        {
            return EditorGUIUtility.singleLineHeight * 2 + 30; // Default height if no target
        }
    }
    private void EndPropertyDrawing(int p_indentLevel)
    {
        EditorGUI.indentLevel = p_indentLevel;
        EditorGUI.EndProperty();
    }
}