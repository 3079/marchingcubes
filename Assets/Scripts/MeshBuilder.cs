using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder : MonoBehaviour
{
    public class CubeGrid
    {
        public Cube[,,] grid;
        public int[,,] map;
        public bool[,,] valueMap;
        public float size;
        public CubeGrid(int[,,] map, bool[,,] valueMap, float size)
        {
            this.map = map;
            this.valueMap = valueMap;
            this.size = size;
            
            int countX = map.GetLength(0);
            int countY = map.GetLength(1);
            int countZ = map.GetLength(2);
            float mapWidth = size * countX;
            float mapHeight = size * countY;
            float mapDepth = size * countZ;

            ControlNode[,,] controlNodes = new ControlNode[countX, countY, countZ];

            grid = new Cube[countX - 1, countY - 1, countZ - 1];
            for (int n = 0; n < (countX - 1) * (countY - 1) * (countZ -1); ++n)
            {
                int x = n / ((countY - 1) * (countZ - 1));
                int r = n % ((countY - 1) * (countZ - 1));
                int y = r / (countZ - 1);
                int z = r % (countZ - 1);
                
                ControlNode[] cNodes = new ControlNode[8];
                        
                for (int i = 0; i < 2; ++i)
                {
                    for (int j = 0; j < 2; ++j)
                    {
                        for (int k = 0; k < 2; ++k)
                        {
                            Vector3 pos = new Vector3(-mapWidth / 2f + (x + i) * size + size / 2f,
                                -mapHeight / 2f + (y + j) * size + size / 2f, mapDepth / 2f - (z + k) * size - size / 2f);
                            ControlNode controlNode;
                            if ((x > 0 && i == 0) || (y > 0 && j == 0) || (z > 0 && k == 0)) 
                            {
                                controlNode = controlNodes[x + i, y + j, z + k];
                            }
                            else
                            {
                                controlNode = new ControlNode(pos, map[x + i, y + j, z + k], valueMap[x + i, y + j, z + k], size);
                                controlNodes[x + i, y + j, z + k] = controlNode;
                            }
                                    
                            int coordMask = (k << 2) + (j << 1) + i;
                            int vertId = (coordMask >> 2) << 2;
                            int t = (coordMask >> 1) % 2;
                            vertId |= t << 1;
                            vertId |= coordMask % 2 == 0 ? t : ~t & 0x00000001;
                                    
                            cNodes[vertId] = controlNode;
                        }
                    }
                }

                grid[x, y, z] = new Cube(cNodes);
            }
        }
    }
    
    public class Node
    {
        public Vector3 pos;
        public int index;
    
        public Node(Vector3 pos, int index)
        {
            this.pos = pos;
            this.index = index;
        }
    }
    
    public class ControlNode
    {
        public Vector3 pos;
        public int value;
        public bool isSet;
        // public Node right, above, forward;
    
        public ControlNode(Vector3 pos, int value, bool isSet, float size)
        {
            this.pos = pos;
            this.value = value;
            this.isSet = isSet;
            
            // right = new Node(pos + Vector3.right * size / 2f);
            // above = new Node(pos + Vector3.up * size / 2f);
            // forward = new Node(pos + Vector3.back * size / 2f);
        }
    }
    
    public class Cube
    {
        public Cube(ControlNode[] cNodes)
        {
            controlNodes = cNodes;

            RecalculateVariant();
        }
        
        public void RecalculateVariant()
        {
            variant = 0;
            for (int i = 0; i < 8; ++i)
            {
                variant |= (controlNodes[i].isSet ? 1 : 0) << i;
            }
        }
        
        public ControlNode[] controlNodes;
        public int variant;
    }

    public CubeGrid cubeGrid;
    [SerializeField] private bool drawGizmos = false;
    [SerializeField] private bool interpolateVertices = false;
    [SerializeField] private bool invertTriangles = false;
    private int _iterator = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private MeshFilter _meshFilter;
    private MeshCollider _collider;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _collider = GetComponent<MeshCollider>();
    }

    private void Update()
    {
        if (drawGizmos && Input.GetMouseButtonDown(0))
        {
            _iterator = (_iterator + 1) % cubeGrid.grid.GetLength(1);
        }
    }
    
    public void BuildMesh(int[,,] map, bool[,,] values, int threshold, float size)
    {
        vertices.Clear();
        triangles.Clear();
        if(_meshFilter.mesh) _meshFilter.mesh.Clear();
        
        cubeGrid = new CubeGrid(map, values, size);

        foreach (var cube in cubeGrid.grid)
        {
            TriangulateCube(cube, threshold);
        }
        
        _meshFilter.mesh = new Mesh();
        _meshFilter.mesh.vertices = vertices.ToArray();
        _meshFilter.mesh.triangles = triangles.ToArray();
        _meshFilter.mesh.RecalculateNormals();
        _collider.sharedMesh = _meshFilter.sharedMesh;
    }

    public void RebuildMesh(int[,,] map, bool[,,] values, int threshold, float size)
    {
        vertices.Clear();
        triangles.Clear();
        
        if (map != cubeGrid.map || size.Equals(cubeGrid.size))
            cubeGrid = new CubeGrid(map, values, size);

        if (values != cubeGrid.valueMap)
        {
            cubeGrid.valueMap = values;
            foreach (var cube in cubeGrid.grid)
            {
                cube.RecalculateVariant();
            }
        }
        
        foreach (var cube in cubeGrid.grid)
        {
            TriangulateCube(cube, threshold);
        }
        
        _meshFilter.mesh.Clear();
        _meshFilter.mesh.vertices = vertices.ToArray();
        _meshFilter.mesh.triangles = triangles.ToArray();
        _meshFilter.mesh.RecalculateNormals();
        _collider.sharedMesh = _meshFilter.mesh;
    }

    private void TriangulateCube(Cube cube, int threshold)
    {
        int[] edges = TriangulationTable.triangulation[cube.variant];

        for (int i = 0; edges[i] != -1; i += 3)
        {
            int a0 = TriangulationTable.cornerIndexAFromEdge[edges[i]];
            int b0 = TriangulationTable.cornerIndexBFromEdge[edges[i]];
            Node v0 = new Node(InterpolateVertices(cube.controlNodes[a0], cube.controlNodes[b0], threshold), vertices.Count);
            vertices.Add(v0.pos);
            
            int a1 = TriangulationTable.cornerIndexAFromEdge[edges[i+1]];
            int b1 = TriangulationTable.cornerIndexBFromEdge[edges[i+1]];
            Node v1 = new Node(InterpolateVertices(cube.controlNodes[a1], cube.controlNodes[b1], threshold), vertices.Count);
            vertices.Add(v1.pos);
            
            int a2 = TriangulationTable.cornerIndexAFromEdge[edges[i+2]];
            int b2 = TriangulationTable.cornerIndexBFromEdge[edges[i+2]];
            Node v2 = new Node(InterpolateVertices(cube.controlNodes[a2], cube.controlNodes[b2], threshold), vertices.Count);
            vertices.Add(v2.pos);

            if (invertTriangles)
            {
                triangles.Add(v2.index);
                triangles.Add(v1.index);
                triangles.Add(v0.index);
            }
            else
            {
                triangles.Add(v0.index);
                triangles.Add(v1.index);
                triangles.Add(v2.index);
            }
        }
    }

    private Vector3 InterpolateVertices(ControlNode v1, ControlNode v2, int threshold)
    {
        if (interpolateVertices && v2.value - v1.value != 0)
        {
            return v1.pos + (v2.pos - v1.pos) * (threshold - v1.value) / (v2.value - v1.value);
        }
        return (v1.pos + v2.pos) / 2f;
    }
    
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        
            for (int x = 0; x < cubeGrid.grid.GetLength(0); ++x)
            {
                for (int z = 0; z < cubeGrid.grid.GetLength(2); ++z)
                {
                    Cube cube = cubeGrid.grid[x, _iterator, z];
                    
                    for (int i = 0; i < 2; ++i)
                    {
                        for (int j = 0; j < 2; ++j)
                        {
                            for (int k = 0; k < 2; ++k)
                            {
                                    ControlNode cNode = cube.controlNodes[(k << 2) + (j << 1) + i];
                                    Gizmos.color =
                                        cNode.isSet ? Color.black : Color.white;
                                    Gizmos.DrawCube(cNode.pos, Vector3.one * 0.4f);
                            }
                        }
                    }
                }
            }
    }
}
