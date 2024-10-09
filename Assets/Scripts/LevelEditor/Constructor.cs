using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Constructor : MonoBehaviour
{
    [Header("Tile Info -> need to be moved")]
    [SerializeField, Range(0.2f,4f)] float tileSize = 1f;
    [SerializeField, Range(0.1f,1.0f)] float yTileOffset = 0.25f;
    [SerializeField, Range(0,30)] int yCurrentLevel = 0;

    [Header("Renderer and preview datas")]
    [SerializeField] GameObject gridRenderer;
    [SerializeField] GameObject currentConstruction;
    GameObject constructionViewer;

    [SerializeField] Material PreviewMat;

    //Find ground location data
    Plane gridBlocker = new Plane(Vector3.down, 0f);
    Camera cam;
    Vector3 screenPos;
    Vector3 worldPos;
    private void Awake()
    {
        cam = Camera.main;
        gridRenderer = Instantiate(gridRenderer);
    }

    private void Start()
    {
        SetConstructionViewer();
    }

    void Update()
    {
        PositionGridRenderer();
        constructionViewer.transform.position = worldPos;

        if (Input.GetMouseButtonDown(0) && currentConstruction != null)
        {
            //TODO : Check underneath before instantiation

            var obj = Instantiate(currentConstruction, worldPos, Quaternion.identity);
        }
    }

    void PositionGridRenderer()
    {
        screenPos = Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (gridBlocker.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
        }

        worldPos.x = Mathf.Round(worldPos.x / tileSize) * tileSize;
        worldPos.z = Mathf.Round(worldPos.z / tileSize) * tileSize;
        worldPos.y = yCurrentLevel * yTileOffset;
        gridRenderer.transform.position = worldPos;
    }

    //Get a visible preview with a different shader
    void SetConstructionViewer()
    {        
        constructionViewer = Instantiate(currentConstruction, Vector3.zero, Quaternion.identity);
        if (PreviewMat != null)
        {
            //we need to allocate a new array, cause modification of the current one are not taking into account
            Renderer renderer = constructionViewer.GetComponentInChildren<Renderer>();
            Material[] allMats = new Material[renderer.materials.Length];
            for (int i = 0; i < allMats.Length; ++i)
                allMats[i] = PreviewMat;
            renderer.materials = allMats;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(worldPos, $"x:{worldPos.x}, y:{worldPos.y}, z:{worldPos.z}");
    }
#endif
}