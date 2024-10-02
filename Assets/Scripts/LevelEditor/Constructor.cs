using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : MonoBehaviour
{
    [SerializeField, Range(1,4)] int tileSize = 1;
    [SerializeField, Range(0.1f,1.0f)] float yTileOffset = 0.25f;
    [SerializeField, Range(0,30)] int yCurrentLevel = 0;

    [SerializeField] GameObject gridRenderer;
    [SerializeField] GameObject currentConstruction;

    Plane gridBlocker = new Plane(Vector3.down, 0f);

    Camera cam;
    private void Awake()
    {
        cam = Camera.main;
        gridRenderer = Instantiate(gridRenderer);
    }

    Vector3 screenPos;
    Vector3 worldPos;
    void Update()
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
}