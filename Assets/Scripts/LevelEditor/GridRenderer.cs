using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GridRenderer : MonoBehaviour
{
    Material mat;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        //Update position of the object to the shader
        mat.SetVector("_ObjPosition", transform.position);
    }

    void UpdateSize()
    {

    }
}
