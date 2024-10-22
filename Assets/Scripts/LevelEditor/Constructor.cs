using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Constructor : MonoBehaviour
{
    [Header("Tile Info -> need to be moved")]
    [SerializeField, Range(0.2f,4f)] float _tileSize = 1f;
    [SerializeField, Range(0.1f,1.0f)] float _yTileOffset = 0.25f;
    [SerializeField, Range(0,30)] int _yCurrentLevel = 0;

    [Header("Renderer and preview datas")]
    [SerializeField] GameObject _gridRenderer;
    [SerializeField] GameObject _currentConstruction;
    GameObject _constructionViewer;

    [SerializeField] Material _previewMat;

    //Find ground location data
    Plane _gridBlocker = new Plane(Vector3.down, 0f);
    Camera _cam;
    Vector3 _screenPos;
    Vector3 _worldPos;
    private void Awake()
    {
        _cam = Camera.main;
        _gridRenderer = Instantiate(_gridRenderer);
    }

    private void Start()
    {
        SetConstructionViewer();
    }

    void Update()
    {
        PositionGridRenderer();
        _constructionViewer.transform.position = _worldPos;

        if (Input.GetMouseButtonDown(0) && _currentConstruction != null)
        {
            //TODO : Check underneath before instantiation

            var obj = Instantiate(_currentConstruction, _worldPos, Quaternion.identity);
        }
    }

    void PositionGridRenderer()
    {
        _screenPos = Input.mousePosition;
        Ray ray = _cam.ScreenPointToRay(_screenPos);

        if (_gridBlocker.Raycast(ray, out float distance))
        {
            _worldPos = ray.GetPoint(distance);
        }

        _worldPos.x = Mathf.Round(_worldPos.x / _tileSize) * _tileSize;
        _worldPos.z = Mathf.Round(_worldPos.z / _tileSize) * _tileSize;
        _worldPos.y = _yCurrentLevel * _yTileOffset;
        _gridRenderer.transform.position = _worldPos;
    }

    //Get a visible preview with a different shader
    void SetConstructionViewer()
    {        
        _constructionViewer = Instantiate(_currentConstruction, Vector3.zero, Quaternion.identity);
        if (_previewMat != null)
        {
            //we need to allocate a new array, cause modification of the current one are not taking into account
            Renderer renderer = _constructionViewer.GetComponentInChildren<Renderer>();
            Material[] allMats = new Material[renderer.materials.Length];
            for (int i = 0; i < allMats.Length; ++i)
                allMats[i] = _previewMat;
            renderer.materials = allMats;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(_worldPos, $"x:{_worldPos.x}, y:{_worldPos.y}, z:{_worldPos.z}");
    }
#endif
}