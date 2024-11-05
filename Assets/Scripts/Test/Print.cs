using UnityEngine;
using UnityEngine.Events;

public class Print : MonoBehaviour
{
    public void PrintMessage()
    {
        Debug.Log(typeof(ScriptableObject));
    }

    public void PrintMessage2(string p_message)
    {
        Debug.Log(p_message);
    }

    public void PrintMessage3(string p_message, int p_number)
    {
        Debug.Log("Multiple parameters method : " + p_message + p_number);
    }


    public void PrintMessageVector(Vector3 p_message)
    {
        Debug.Log(p_message);
    }

    public void PrintMessageFloat(double p_message)
    {
        Debug.Log(p_message);
    }
}