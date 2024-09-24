using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    Node[,,] grid;
    [Header("Grid parameters")]
    [SerializeField, Range(4, 50)] int gridXSize = 15;
    [SerializeField, Range(1, 30)] int gridYSize = 1;
    [SerializeField, Range(4, 50)] int gridZSize = 15;
    [SerializeField, Min(0.1f)] float cellSize = 1.0f;
    [SerializeField, Min(1)] int nbLayerPerCellSize = 4;
    [SerializeField] GameObject TileModel;

    private void Awake()
    {
        GenerateMap();
    }

    public Node GetNodeAtIndex(int X, int Y, int Z)
    {
        return grid[X, Y, Z];
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
        for (int z = 0; z < gridZSize; ++z)
        {
            for (int y = 0; y < gridYSize; ++y)
            {
                for (int x = 0; x < gridXSize; ++x)
                {
                    //Find all adjacent elements inside the grid
                    {
                        if (!IsIndexOutOfBound(x - 1, y, z)) neighborNodes.Add(grid[x - 1, y, z]);
                        if (!IsIndexOutOfBound(x + 1, y, z)) neighborNodes.Add(grid[x + 1, y, z]);

                        if (!IsIndexOutOfBound(x, y - 1, z)) neighborNodes.Add(grid[x, y - 1, z]);
                        if (!IsIndexOutOfBound(x, y + 1, z)) neighborNodes.Add(grid[x, y + 1, z]);

                        if (!IsIndexOutOfBound(x, y, z - 1)) neighborNodes.Add(grid[x, y, z - 1]);
                        if (!IsIndexOutOfBound(x, y, z + 1)) neighborNodes.Add(grid[x, y, z + 1]);
                    }

                    ray.origin = grid[x, y, z].transform.position;
                    //for each adjacent element in the grid
                    foreach (Node node in neighborNodes)
                    {
                        ray.direction = node.transform.position - ray.origin;
                        if (Physics.Raycast(ray, out hit, cellSize, mask))
                        {

                        }
                    }
                }
            }
        }
    }

    bool IsIndexOutOfBound(int X, int Y, int Z)
    {
        return X >= gridXSize || Y >= gridYSize || Z >= gridZSize ||
                X < 0 || Y < 0 || Z < 0;
    }


    [ContextMenu("GenerateModelLayer")]
    void GenerateModelLayer()
    {
        if (TileModel == null)
        {
            Debug.LogError($"Error on {gameObject.name} : A tile model need to be referenced to generate a model layer.");
            return;
        }

        grid = new Node[gridXSize, gridYSize, gridXSize];
        for (int z = 0; z < gridZSize; ++z)
        {
            for (int y = 0; y < gridYSize; ++y)
            {
                for (int x = 0; x < gridXSize; ++x)
                {
                    //Adding 0.5f cause origin of tile are centered, not in a corner
                    var Go = Instantiate(TileModel,
                                new Vector3(transform.position.x + (x + 0.5f) * cellSize,
                                            transform.position.y + transform.position.y + ((float)y / nbLayerPerCellSize) * cellSize, 
                                            transform.position.z + (z + 0.5f) * cellSize),
                                Quaternion.identity, transform);
                    Go.transform.localScale = new Vector3(cellSize, cellSize/nbLayerPerCellSize, cellSize);
                    grid[x, y, z] = Go.GetComponent<Node>();
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
    [SerializeField, Min(0)] int XindexToCheck;
    [ContextMenuItem("Check position", "CheckIndex")]
    [SerializeField, Min(0)] int YindexToCheck;
    [ContextMenuItem("Check position", "CheckIndex")]
    [SerializeField, Min(0)] int ZindexToCheck;
    
    void CheckIndex()
    {
        if (grid == null)
        {
            Debug.LogError($"Error on {gameObject.name} : Grid need to be populated before trying to access her data.");
            return;
        }
        Renderer R = GetNodeAtIndex(XindexToCheck, YindexToCheck, ZindexToCheck).GetComponentInChildren<Renderer>();
        R.material.color = Color.green;
    }

    private void OnDrawGizmos()
    {
        for (int z = 0; z < gridZSize; ++z)
        {
            for (int y = 0; y < gridYSize; ++y)
            {
                for (int x = 0; x < gridXSize; ++x)
                {
                    Vector3 Pos = new Vector3(transform.position.x + (x+0.5f) * cellSize,
                                                transform.position.y + ((float)y / nbLayerPerCellSize) * cellSize,
                                                transform.position.z + (z + 0.5f) * cellSize);
                    Gizmos.DrawWireCube(Pos, new Vector3(0.66f * cellSize, cellSize / nbLayerPerCellSize, 0.66f * cellSize));

                    UnityEditor.Handles.color = Color.blue;
                    UnityEditor.Handles.Label(Pos, $"x:{x}, y:{y}, z:{z}");
                }
            }
        }        
    }
#endif
}