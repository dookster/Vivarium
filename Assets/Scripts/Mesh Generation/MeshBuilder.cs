using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder  {

    /// <summary>
    /// The vertex positions of the mesh.
    /// </summary>
    public List<Vector3> Vertices { get { return m_Vertices; } }
    private List<Vector3> m_Vertices = new List<Vector3>();

    /// <summary>
    /// The vertex normals of the mesh.
    /// </summary>
    public List<Vector3> Normals { get { return m_Normals; } }
    private List<Vector3> m_Normals = new List<Vector3>();

    /// <summary>
    /// The UV coordinates of the mesh.
    /// </summary>
    public List<Vector2> UVs { get { return m_UVs; } }
    private List<Vector2> m_UVs = new List<Vector2>();

    //indices for the triangles:
    private List<int> m_Indices = new List<int>();

    public void AddTriangle(int index0, int index1, int index2)
    {
        m_Indices.Add(index0);
        m_Indices.Add(index1);
        m_Indices.Add(index2);
    }

    public Mesh CreateMesh()
    {
        //Create an instance of the Unity Mesh class:
        Mesh mesh = new Mesh();

        //add our vertex and triangle values to the new mesh:
        mesh.vertices = m_Vertices.ToArray();
        mesh.triangles = m_Indices.ToArray();

        //Normals are optional. Only use them if we have the correct amount:
        if (m_Normals.Count == m_Vertices.Count)
            mesh.normals = m_Normals.ToArray();

        //UVs are optional. Only use them if we have the correct amount:
        if (m_UVs.Count == m_Vertices.Count)
            mesh.uv = m_UVs.ToArray();

        //have the mesh recalculate its bounding box (required for proper rendering):
        mesh.RecalculateBounds();

        return mesh;
    }

}
