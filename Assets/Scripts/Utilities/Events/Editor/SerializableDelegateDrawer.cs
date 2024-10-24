using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableDelegateNoParam), true)] // 'true' applies to derived classes
public class SerializableDelegateDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Start drawing properties
        EditorGUI.BeginProperty(position, label, property);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 1;

        // Get the target object and method name fields
        SerializedProperty targetProperty = property.FindPropertyRelative("_target");
        SerializedProperty componentProperty = property.FindPropertyRelative("_component");
        SerializedProperty methodNameProperty = property.FindPropertyRelative("_methodName");

        Color LineBlack = new Color(0.102f, 0.102f, 0.102f);
        Color BGDarkGrey = new Color(0.207f, 0.207f, 0.207f);
        Color BGLightGrey = new Color(0.274f, 0.274f, 0.274f);

        float yOffset = 0f;
        float yLargeSeparator = 6f;
        float yLittleSeparator = 3f;

        // Draw the label (variable name)
        Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        yOffset += labelRect.height;
        EditorGUI.DrawRect(labelRect, BGDarkGrey);
        EditorGUI.LabelField(labelRect, label);

        // Create a box around the property similar to UnityEvents
        Rect boxRect = new Rect(labelRect.x, yOffset, labelRect.width, GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight * 2);
        EditorGUI.DrawRect(boxRect, BGLightGrey); // Background color of the box

        EditorGUI.DrawRect(new Rect(labelRect.x, labelRect.y, position.width, 1), LineBlack); // Top line
        EditorGUI.DrawRect(new Rect(labelRect.x, labelRect.y + +EditorGUIUtility.singleLineHeight, position.width, 1), LineBlack); // VariableNameSeparatorLine
        EditorGUI.DrawRect(new Rect(boxRect.x, boxRect.y + boxRect.height - 1, boxRect.width, 1), LineBlack); // Bottom line
        EditorGUI.DrawRect(new Rect(labelRect.x, labelRect.y, 1, labelRect.height + boxRect.height), LineBlack); // Left line
        EditorGUI.DrawRect(new Rect(labelRect.x + labelRect.width - 1, labelRect.y, 1, labelRect.height + boxRect.height), LineBlack); // Right line


        // Draw the target field
        yOffset += yLargeSeparator;
        Rect targetRect = new Rect(boxRect.x, yOffset, boxRect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(targetRect, targetProperty);

        UnityEngine.Object targetObject = targetProperty.objectReferenceValue;
        if (targetObject == null) goto EndPropertyDrawing;        
        // If target is a GameObject, show a component dropdown
        if (targetObject is GameObject)
        {
            GameObject gameObject = (GameObject)targetObject;
            Component[] components = gameObject.GetComponents<Component>();
            string[] componentNames = components.Select(c => c.GetType().Name).ToArray();

            // Find the currently selected component index
            Component selectedComponent = componentProperty.objectReferenceValue as Component;
            int selectedIndex = selectedComponent != null ? Array.IndexOf(components, selectedComponent) : 0;

            // Draw component dropdown
            yOffset += EditorGUIUtility.singleLineHeight + yLittleSeparator;
            Rect componentRect = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
            selectedIndex = EditorGUI.Popup(componentRect, "Component", selectedIndex, componentNames);

            // Update the component field with the selected component
            if (selectedIndex >= 0 && selectedIndex < components.Length)
            {
                componentProperty.objectReferenceValue = components[selectedIndex];
            }

            yOffset += EditorGUIUtility.singleLineHeight + yLittleSeparator;
            // If a component is selected, show method dropdown
            if (selectedIndex >= 0 && selectedIndex < components.Length)
            {
                Component component = components[selectedIndex];
                DrawMethodDropdown(position, yOffset, component.GetType(), methodNameProperty);
            }
        }
        // If target is a ScriptableObject, show method dropdown directly
        else if (targetObject is ScriptableObject)
        {
            yOffset += EditorGUIUtility.singleLineHeight + yLittleSeparator;

            ScriptableObject scriptableObject = (ScriptableObject)targetObject;
            DrawMethodDropdown(position, yOffset, scriptableObject.GetType(), methodNameProperty);

            // Clear the component field since we're working with a ScriptableObject
            componentProperty.objectReferenceValue = null;
        }

    EndPropertyDrawing:
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

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
        // Two lines for target and method selection, plus one for component if it's a GameObject
        SerializedProperty targetProperty = property.FindPropertyRelative("_target");
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
}