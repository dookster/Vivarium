using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MestTest : MonoBehaviour {

    public MeshBuilder meshBuilder;

    public float m_Length = 1;
    public float m_Width = 1;
    public float m_Height = 1;

    public int m_SegmentCount = 10;
    public int m_StepCount;

	void Start ()
    {
        meshBuilder = new MeshBuilder();

        for (int i = 0; i <= m_SegmentCount; i++)
        {
            float z = m_Length * i;
            float v = (1.0f / m_SegmentCount) * i;

            for (int j = 0; j <= m_SegmentCount; j++)
            {
                float x = m_Width * j;
                float u = (1.0f / m_SegmentCount) * j;

                Vector3 offset = new Vector3(x, Random.Range(0.0f, m_Height), z);

                Vector2 uv = new Vector2(u, v);
                bool buildTriangles = i > 0 && j > 0;

                BuildQuadForGrid(meshBuilder, offset, uv, buildTriangles, m_StepCount + 1);
            }
        }

        //Create the mesh:
        MeshFilter filter = GetComponent<MeshFilter>();

        if (filter != null)
        {
            Mesh mesh = meshBuilder.CreateMesh();
            mesh.RecalculateNormals();
            filter.sharedMesh = mesh;
        }
    }

    void BuildQuadForGrid(MeshBuilder meshBuilder, Vector3 position, Vector2 uv, bool buildTriangles, int vertsPerRow)
    {
        meshBuilder.Vertices.Add(position);
        meshBuilder.UVs.Add(uv);

        if (buildTriangles)
        {
            int baseIndex = meshBuilder.Vertices.Count - 1;

            int index0 = baseIndex;
            int index1 = baseIndex - 1;
            int index2 = baseIndex - vertsPerRow;
            int index3 = baseIndex - vertsPerRow - 1;

            meshBuilder.AddTriangle(index0, index2, index1);
            meshBuilder.AddTriangle(index2, index3, index1);
        }
    }

    void BuildQuad(MeshBuilder meshBuilder, Vector3 offset)
    {
        meshBuilder.Vertices.Add(new Vector3(0.0f, 0.0f, 0.0f) + offset);
        meshBuilder.UVs.Add(new Vector2(0.0f, 0.0f));
        meshBuilder.Normals.Add(Vector3.up);

        meshBuilder.Vertices.Add(new Vector3(0.0f, 0.0f, m_Length) + offset);
        meshBuilder.UVs.Add(new Vector2(0.0f, 1.0f));
        meshBuilder.Normals.Add(Vector3.up);

        meshBuilder.Vertices.Add(new Vector3(m_Width, 0.0f, m_Length) + offset);
        meshBuilder.UVs.Add(new Vector2(1.0f, 1.0f));
        meshBuilder.Normals.Add(Vector3.up);

        meshBuilder.Vertices.Add(new Vector3(m_Width, 0.0f, 0.0f) + offset);
        meshBuilder.UVs.Add(new Vector2(1.0f, 0.0f));
        meshBuilder.Normals.Add(Vector3.up);

        int baseIndex = meshBuilder.Vertices.Count - 4;

        meshBuilder.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
        meshBuilder.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
    }

}
