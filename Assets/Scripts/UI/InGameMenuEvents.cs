using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameMenuEvents : MonoBehaviour
{
    UIDocument _document;
    Button _buttonConstructMode;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        _buttonConstructMode = _document.rootVisualElement.Q("ConstructionMode") as Button;
    }

    private void OnEnable()
    {
        _buttonConstructMode.RegisterCallback<ClickEvent>(OnButtonConstructionClicked);
    }

    private void OnDisable()
    {
        _buttonConstructMode.UnregisterCallback<ClickEvent>(OnButtonConstructionClicked);
    }

    void OnButtonConstructionClicked(ClickEvent p_event)
    {
        Debug.Log("Yay");
    }
}
