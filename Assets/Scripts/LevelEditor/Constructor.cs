#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.InputSystem;

enum EConstructionMode
{
    OnTerrain = 7,
    OnFoundation = 4,
    Nothing
}

public class Constructor : MonoBehaviour
{
    [Header("Tile Info -> need to be moved")]
    [SerializeField, Range(0.2f, 4f)] float _tileSize = 1f;
    [SerializeField, Range(0.1f, 1.0f)] float _yTileOffset = 0.25f;
    [SerializeField, Range(0, 30)] int _yCurrentLevel = 0;

    [Header("Renderer datas")]
    [SerializeField] float _gridRendererSize = 1f;
    GridRenderer _gridRenderer;

    [Header("Construction")]
    [SerializeField] Material _previewMat;
    [SerializeField] int _constructionViewerLayerMask;
    [SerializeField] GameObject _currentConstruction;
    GameObject _constructionViewer;
    [SerializeField] float _maxConstructionRange = 50f;
    [SerializeField] LayerMask _constructorTargetlayerMasks;

    [Header("Input")]
    PlayerInputActions _inputActionAsset;
    InputAction _buildAction;

    //Find ground location data
    Plane _gridBlocker = new Plane(Vector3.down, 0f);
    Camera _cam;
    Vector3 _screenPos;
    Vector3 _worldPos;
    private void Awake()
    {
        _cam = Camera.main;
        _gridRenderer = GetComponentInChildren<GridRenderer>(true);

        //Inputs
        _inputActionAsset = new PlayerInputActions();
        _buildAction = _inputActionAsset.Player.Build;
    }

    private void Start()
    {
        SetConstructionViewer();
        _gridRenderer.UpdateSize(_gridRendererSize, _tileSize);
    }

    private void OnEnable()
    {
        _gridRenderer.gameObject.SetActive(true);
        
        //Inputs
        _buildAction.Enable();
        _buildAction.started += BuildAction;
    }

    private void BuildAction(InputAction.CallbackContext obj)
    {
        if (obj.started)
        {
            BuildCurrentConstruction();
        }
    }

    private void OnDisable()
    {
        _gridRenderer.gameObject.SetActive(false);

        //Inputs
        _buildAction.Disable();
    }

    void Update()
    {
        ComputeWorldPosFromCamera();
        transform.position = _worldPos;
    }

    void ComputeWorldPosFromCamera()
    {
        _screenPos = Input.mousePosition;
        Ray ray = _cam.ScreenPointToRay(_screenPos);
        RaycastHit hit;
        EConstructionMode constructionMode = EConstructionMode.Nothing;

        if (Physics.Raycast(ray, out hit, _maxConstructionRange, _constructorTargetlayerMasks))
        {
            constructionMode = (EConstructionMode)hit.transform.gameObject.layer;
        }

        switch(constructionMode)
        {
            case EConstructionMode.OnFoundation: break;

            case EConstructionMode.OnTerrain: 
                _worldPos = hit.point;
                return;

            case EConstructionMode.Nothing: return;

            default: break;
        }

        _worldPos.x = Mathf.Round(_worldPos.x / _tileSize) * _tileSize;
        _worldPos.z = Mathf.Round(_worldPos.z / _tileSize) * _tileSize;
        _worldPos.y = _yCurrentLevel * _yTileOffset;
    }

    //Get a visible preview with a different shader
    void SetConstructionViewer()
    {        
        _constructionViewer = Instantiate(_currentConstruction, Vector3.zero, Quaternion.identity, transform);
        _constructionViewer.name = "ConstructionViewer";
        _constructionViewer.gameObject.layer = _constructionViewerLayerMask;
        foreach (Transform child in _constructionViewer.transform)
        {
            child.gameObject.layer = _constructionViewerLayerMask;
        }

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

    void BuildCurrentConstruction()
    {
        //TODO : Check underneath before instantiation
        var obj = Instantiate(_currentConstruction, _worldPos, _constructionViewer.transform.rotation);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(_worldPos, $"x:{_worldPos.x}, y:{_worldPos.y}, z:{_worldPos.z}");
    }
#endif
}