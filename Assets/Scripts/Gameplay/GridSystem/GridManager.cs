using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    Node[,,] _grid;
    [Header("Grid parameters")]
    [SerializeField, Range(4, 50)] int _gridXSize = 15;
    [SerializeField, Range(1, 30)] int _gridYSize = 1;
    [SerializeField, Range(4, 50)] int _gridZSize = 15;
    [SerializeField, Min(0.1f)] float _cellSize = 1.0f;
    [SerializeField, Min(1)] int _nbLayerPerCellSize = 4;
    [SerializeField] GameObject _tileModel;

    private void Awake()
    {
        GenerateMap();
    }

    public Node GetNodeAtIndex(int X, int Y, int Z)
    {
        return _grid[X, Y, Z];
    }

    [ContextMenu("Generate Map")]
    void GenerateMap()
    {
        GenerateModelLayer();
        GenerateGraph();
    }

    void GenerateGraph()
    {
        //caching some data
        int mask = LayerMask.GetMask("Tiles");
        RaycastHit hit;
        Ray ray = new Ray();

        List<Node> neighborNodes = new List<Node>();
        neighborNodes.Capacity = 4;

        //for each node
        for (int z = 0; z < _gridZSize; ++z)
        {
            for (int y = 0; y < _gridYSize; ++y)
            {
                for (int x = 0; x < _gridXSize; ++x)
                {
                    //Find all adjacent elements inside the grid
                    {
                        if (!IsIndexOutOfBound(x - 1, y, z)) neighborNodes.Add(_grid[x - 1, y, z]);
                        if (!IsIndexOutOfBound(x + 1, y, z)) neighborNodes.Add(_grid[x + 1, y, z]);

                        if (!IsIndexOutOfBound(x, y - 1, z)) neighborNodes.Add(_grid[x, y - 1, z]);
                        if (!IsIndexOutOfBound(x, y + 1, z)) neighborNodes.Add(_grid[x, y + 1, z]);

                        if (!IsIndexOutOfBound(x, y, z - 1)) neighborNodes.Add(_grid[x, y, z - 1]);
                        if (!IsIndexOutOfBound(x, y, z + 1)) neighborNodes.Add(_grid[x, y, z + 1]);
                    }

                    ray.origin = _grid[x, y, z].transform.position;
                    //for each adjacent element in the grid
                    foreach (Node node in neighborNodes)
                    {
                        ray.direction = node.transform.position - ray.origin;
                        if (Physics.Raycast(ray, out hit, _cellSize, mask))
                        {

                        }
                    }
                }
            }
        }
    }

    bool IsIndexOutOfBound(int p_X, int p_Y, int p_Z)
    {
        return p_X >= _gridXSize || p_Y >= _gridYSize || p_Z >= _gridZSize ||
                p_X < 0 || p_Y < 0 || p_Z < 0;
    }


    [ContextMenu("GenerateModelLayer")]
    void GenerateModelLayer()
    {
        if (_tileModel == null)
        {
            Debug.LogError($"Error on {gameObject.name} : A tile model need to be referenced to generate a model layer.");
            return;
        }

        _grid = new Node[_gridXSize, _gridYSize, _gridXSize];
        for (int z = 0; z < _gridZSize; ++z)
        {
            for (int y = 0; y < _gridYSize; ++y)
            {
                for (int x = 0; x < _gridXSize; ++x)
                {
                    //Adding 0.5f cause origin of tile are centered, not in a corner
                    var Go = Instantiate(_tileModel,
                                new Vector3(transform.position.x + (x + 0.5f) * _cellSize,
                                            transform.position.y + transform.position.y + ((float)y / _nbLayerPerCellSize) * _cellSize, 
                                            transform.position.z + (z + 0.5f) * _cellSize),
                                Quaternion.identity, transform);
                    Go.transform.localScale = new Vector3(_cellSize, _cellSize/_nbLayerPerCellSize, _cellSize);
                    _grid[x, y, z] = Go.GetComponent<Node>();
                }
            }
        }
    }

    [ContextMenu("ClearModelLayer")]
    void ClearModelLayer()
    {
        List<Transform> allTransform = new List<Transform>();
        //Two steps needed, one to get all references, one to delete cause of Immediate Destruction
        //GetComponentsInChildren search inside ourself, then inside our childrens....
        foreach (Transform child in transform)
        {
            allTransform.Add(child);
        }
        foreach (Transform child in allTransform)
        {
            DestroyImmediate(child.gameObject); //Cannot use simple Destroy in edit mode
        }
    }

#if UNITY_EDITOR

    [Header("Check Position")]
    [ContextMenuItem("Check position", "CheckIndex")]
    [SerializeField, Min(0)] int _XindexToCheck;
    [ContextMenuItem("Check position", "CheckIndex")]
    [SerializeField, Min(0)] int _YindexToCheck;
    [ContextMenuItem("Check position", "CheckIndex")]
    [SerializeField, Min(0)] int _ZindexToCheck;
    
    void CheckIndex()
    {
        if (_grid == null)
        {
            Debug.LogError($"Error on {gameObject.name} : Grid need to be populated before trying to access her data.");
            return;
        }
        Renderer R = GetNodeAtIndex(_XindexToCheck, _YindexToCheck, _ZindexToCheck).GetComponentInChildren<Renderer>();
        R.material.color = Color.green;
    }

    private void OnDrawGizmos()
    {
        return;
        //for (int z = 0; z < _gridZSize; ++z)
        //{
        //    for (int y = 0; y < _gridYSize; ++y)
        //    {
        //        for (int x = 0; x < _gridXSize; ++x)
        //        {
        //            Vector3 Pos = new Vector3(transform.position.x + (x+0.5f) * _cellSize,
        //                                        transform.position.y + ((float)y / _nbLayerPerCellSize) * _cellSize,
        //                                        transform.position.z + (z + 0.5f) * _cellSize);
        //            Gizmos.DrawWireCube(Pos, new Vector3(0.66f * _cellSize, _cellSize / _nbLayerPerCellSize, 0.66f * _cellSize));

        //            UnityEditor.Handles.color = Color.blue;
        //            UnityEditor.Handles.Label(Pos, $"x:{x}, y:{y}, z:{z}");
        //        }
        //    }
        //}        
    }
#endif
}