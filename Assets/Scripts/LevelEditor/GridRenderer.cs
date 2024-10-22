using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GridRenderer : MonoBehaviour
{
    Material _mat;

    private void Start()
    {
        _mat = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        //Update position of the object to the shader
        _mat.SetVector("_ObjPosition", transform.position);
    }

    void UpdateSize()
    {

    }
}
