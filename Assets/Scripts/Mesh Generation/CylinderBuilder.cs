using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderBuilder : MonoBehaviour {

    public int m_RadialSegmentCount = 10;
    public int m_HeightSegmentCount = 3;
    //public float m_Radius = 1f;
    //public float m_Height = 1f;
    //public float m_BendAngle = 90;
    //public Vector3 startOffset;

    //public AnimationCurve xPosCurve;
    //public AnimationCurve zPosCurve;

    public AnimationCurve radiusCurve;

    public BezierSpline spline;

    public MeshBuilder meshBuilder;

	public void GenerateMeshAlongSpline (BezierSpline spline)
    {
        //meshBuilder = new MeshBuilder();

        Vector3 centrePos = Vector3.zero;

        Vector3 lastSplineDir = Vector3.negativeInfinity;
        Vector3 up = Vector3.up;

        for (int i = 0; i <= m_HeightSegmentCount; i++)
        {
      
            float t = (float)i / m_HeightSegmentCount;

            centrePos = spline.GetPoint(t);

            Vector3 splineDir = spline.GetDirection(t);
            
            // Flip 'up' if we make a sharp turn on the x/z plane (when the spline curves back over itself) otherwise it gets twisted
            Vector3 lastFlat = Vector3.ProjectOnPlane(lastSplineDir, Vector3.up);
            Vector3 curFlat = Vector3.ProjectOnPlane(splineDir, Vector3.up);
            float dotProd = Vector3.Dot(lastFlat, curFlat);
            if(dotProd < 0)
            {
                up = -up;
                DebugExtension.DebugWireSphere(centrePos, Color.red, 1, 60, false);
            }

            // Rotation of ring (up is forward...)
            Quaternion rotation = YLookRotation(splineDir, up);

            // If ring is flat on plane, twist it a bit and flip up for next ring
            bool adjustVertRot = false;
            if (i > 0 && Vector3.Angle(splineDir, Vector3.ProjectOnPlane(splineDir, Vector3.right)) < 1f)
            {
                DebugExtension.DebugWireSphere(centrePos, Color.blue, 0.5f, 60, false);
                adjustVertRot = true;
            }

            // Build ring
            BuildRing(meshBuilder, m_RadialSegmentCount, centrePos, radiusCurve.Evaluate(t), t, i > 0, rotation, adjustVertRot);

            if (adjustVertRot) up = -up;
            lastSplineDir = splineDir;
        }

        //BuildCap(meshBuilder, Vector3.zero, true);
        //BuildCap(meshBuilder, Vector3.up * m_Height, false);

        //Create the mesh:
        MeshFilter filter = GetComponent<MeshFilter>();

        if (filter != null)
        {
            Mesh mesh = meshBuilder.CreateMesh();
            //mesh.RecalculateNormals();
            filter.sharedMesh = mesh;
        }
    }

    public static float RoundToTwo(float f)
    {
        return Mathf.Round(f * 10f) / 10f;
    }

    Quaternion YLookRotation(Vector3 dir, Vector3 up)
    {

        Quaternion upToForward = Quaternion.Euler(90.0f, 0f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(dir, up);

        return forwardToTarget * upToForward;
    }

    float lastYRot = -1000;

    void BuildRing(MeshBuilder meshBuilder, int segmentCount, Vector3 centre, float radius, float v, bool buildTriangles, Quaternion rotation, bool adjustVertRot = false)
    {

        float angleInc = (Mathf.PI * 2.0f) / segmentCount;

        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = angleInc * i;

            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.z = Mathf.Sin(angle);

            //Debug.Log("Rot " + rotation.eulerAngles);

            if (lastYRot != -1000)
            {
                
                if (rotation.eulerAngles.y == 0)
                {
                    DebugExtension.DebugWireSphere(centre, Color.red, radius, 60, false);
                    //rotation = Quaternion.Euler(rotation.x, lastYRot, rotation.z);
                }
            }
            if (adjustVertRot)
            {
                //unitPosition = Quaternion.Euler(0, -45, 0) * unitPosition;
            }

            lastYRot = rotation.eulerAngles.y;

            unitPosition = rotation * unitPosition;

            Vector3 vertexPosition = centre + unitPosition * radius;

            meshBuilder.Vertices.Add(vertexPosition);
            meshBuilder.Normals.Add(unitPosition);
            meshBuilder.UVs.Add(new Vector2((float)i / segmentCount, v));

            if (i > 0 && buildTriangles)
            {
                int baseIndex = meshBuilder.Vertices.Count - 1;

                int vertsPrRow = segmentCount + 1;

                int index0 = baseIndex;
                int index1 = baseIndex - 1;
                int index2 = baseIndex - vertsPrRow;
                int index3 = baseIndex - vertsPrRow - 1;

                meshBuilder.AddTriangle(index0, index2, index1);
                meshBuilder.AddTriangle(index2, index3, index1);

            }
        }
    }


    //void BuildCap(MeshBuilder meshBuilder, Vector3 centre, bool reverseDirection)
    //{
    //    Vector3 normal = reverseDirection ? Vector3.down : Vector3.up;

    //    //one vertex in the center:
    //    meshBuilder.Vertices.Add(centre);
    //    meshBuilder.Normals.Add(normal);
    //    meshBuilder.UVs.Add(new Vector2(0.5f, 0.5f));

    //    int centreVertexIndex = meshBuilder.Vertices.Count - 1;

    //    //vertices around the edge:
    //    float angleInc = (Mathf.PI * 2.0f) / m_RadialSegmentCount;

    //    for (int i = 0; i <= m_RadialSegmentCount; i++)
    //    {
    //        float angle = angleInc * i;

    //        Vector3 unitPosition = Vector3.zero;
    //        unitPosition.x = Mathf.Cos(angle);
    //        unitPosition.z = Mathf.Sin(angle);

    //        meshBuilder.Vertices.Add(centre + unitPosition * m_Radius);
    //        meshBuilder.Normals.Add(normal);

    //        Vector2 uv = new Vector2(unitPosition.x + 1.0f, unitPosition.z + 1.0f) * 0.5f;
    //        meshBuilder.UVs.Add(uv);

    //        //build a triangle:
    //        if (i > 0)
    //        {
    //            int baseIndex = meshBuilder.Vertices.Count - 1;

    //            if (reverseDirection)
    //                meshBuilder.AddTriangle(centreVertexIndex, baseIndex - 1,
    //                    baseIndex);
    //            else
    //                meshBuilder.AddTriangle(centreVertexIndex, baseIndex,
    //                    baseIndex - 1);
    //        }
    //    }
    //}

}
