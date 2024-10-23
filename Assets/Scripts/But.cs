using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class But : MonoBehaviour
{

    public void Pressed(bool pressed)
    {
        Debug.Log($"Yay : {pressed}");
    }
}
