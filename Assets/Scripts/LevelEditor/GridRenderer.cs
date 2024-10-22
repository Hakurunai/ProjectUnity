using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GridRenderer : MonoBehaviour
{
    Material _mat;

    private void Awake()
    {
        _mat = GetComponent<Renderer>().material;        
    }

    private void Update()
    {
        //Update position of the object to the shader
        _mat.SetVector("_ObjPosition", transform.position);
    }

    public void UpdateSize(float p_RendererScale, float p_CellSize)
    {
        if (p_RendererScale <= 0f || p_CellSize <= 0f)
        {
            throw new System.Exception("A gridRenderer or his cellSize cannot be less or equal to 0");
        }
        float previousScale = transform.localScale.x;

        //TODO : need to finish this part
        //Renderer size
        transform.localScale = Vector3.one * p_RendererScale;

        //Tex Tiling
        _mat.mainTextureScale = Vector2.one * (p_RendererScale / p_CellSize);

        Debug.Log($"R = {p_RendererScale}, C = {p_CellSize}, T = {Vector2.one * (p_RendererScale / p_CellSize)}");
    }
}
