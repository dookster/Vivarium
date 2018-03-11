using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBase : MonoBehaviour {

    public BezierSpline CUSTOMSPLINE;

    public PlantData data;

    public bool animateGrowth = true;

    private BezierSpline trunkSpline;
    private List<BezierSpline> branchSplines = new List<BezierSpline>();
    private MeshBuilder meshBuilder;

    void Start ()
    {
        
	}

    private void OnEnable()
    {
        meshBuilder = new MeshBuilder();


        Renderer ren = GetComponent<Renderer>();
        ren.material.color = data.color.Evaluate(Random.Range(0f, 1f));

        Generate();
    }

    void Update ()
    {
		
	}
    
    public void Generate()
    {
        // Trunk
        Vector3 foot = Vector3.zero;
        //Vector3 top = foot + (Vector3.up * data.GetTotalHeight());

        if(CUSTOMSPLINE == null)
        {
            trunkSpline = gameObject.AddComponent<BezierSpline>();
            //trunkSpline.Setup(BezierControlPointMode.Free, foot, foot + Vector3.up, top, top + Vector3.up + TEMPLeanDir);
            trunkSpline.Setup(BezierControlPointMode.Free, foot);
            //trunkSpline.AddCurveType(CurveType.Wave, Quaternion.identity, data.size * data.height);
            trunkSpline.AddCurveType(FlipCoin() ? CurveType.Straight : CurveType.Wave, Quaternion.Euler(0, Random.Range(0f, 360f), 0), data.size * data.height);
            trunkSpline.AddCurveType(FlipCoin() ? CurveType.Straight : CurveType.Wave, Quaternion.Euler(0, Random.Range(0f, 360f), 0), data.size * data.height);
            trunkSpline.AddCurveType(FlipCoin() ? CurveType.Straight : CurveType.Wave, Quaternion.Euler(0, Random.Range(0f, 360f), 0), data.size * data.height);
            trunkSpline.AddCurveType(FlipCoin() ? CurveType.Straight : CurveType.Wave, Quaternion.Euler(0, Random.Range(0f, 360f), 0), data.size * data.height);

            if (animateGrowth)
            {
                StartCoroutine(MakeTrunk(trunkSpline));
            }
            else
            {
                GenerateMeshAlongSpline(trunkSpline, 1f, data.TrunkHeightSegmentCount, data.TrunkRadialSegmentCount, data.TrunkRadiusCurve, data.TrunkSize * data.size);

                // Branches
                int numBranches = Random.Range(data.minBranches, data.maxBranches + 1);
                float branchAngle = Random.Range(0f, 360f);
                for (int n = numBranches; n > 0; n--)
                {
                    AddBranch((1f / numBranches) * n, Quaternion.Euler(0, branchAngle, 90));

                    if (data.twinBranch)
                    {
                        AddBranch((1f / numBranches) * n, Quaternion.Euler(0, branchAngle - 180, 90));
                    }
                    if (!data.sameAxisBranches)
                    {
                        branchAngle = Random.Range(0f, 360f);
                    }
                }

                // ADD LEAVES ON TRUNK
                int numLeaves = Random.Range(data.minLeaves, data.maxLeaves * 3);
                for (int n = numLeaves; n > 0; n--)
                {
                    Vector3 pos = trunkSpline.GetPoint((1f / numLeaves) * n);
                    GameObject go = Instantiate(data.TMPLeafPrefab);
                    go.transform.position = transform.TransformPoint(pos + new Vector3(FlipCoin() ? 0.2f : -0.2f, 0, 0));// * localBranchSize * data.TrunkSize * data.size;
                    go.transform.rotation *= Quaternion.Euler(0, branchAngle, 0);
                    go.transform.SetParent(transform, true);
                }
            }

        }
        else
        {
            GenerateMeshAlongSpline(CUSTOMSPLINE, 1f, data.TrunkHeightSegmentCount, data.TrunkRadialSegmentCount, data.TrunkRadiusCurve, data.TrunkSize * data.size);
        }
    }

    public bool FlipCoin()
    {
        return Random.Range(0, 2) == 0;
    }

    float growStep = 0.1f;

    public IEnumerator MakeTrunk(BezierSpline spline)
    {
        yield return new WaitForSeconds(1f);
        float growAmount = 0f;
        while(growAmount <= 1f)
        {
            growAmount += growStep * Time.deltaTime;
            meshBuilder.Clear();
            GenerateMeshAlongSpline(trunkSpline, growAmount, data.TrunkHeightSegmentCount, data.TrunkRadialSegmentCount, data.TrunkRadiusCurve, data.TrunkSize * data.size);
            yield return 0;
        }
    }

    private void AddBranch(float height, Quaternion rotation)
    {
        Vector3 branchBase = trunkSpline.GetPoint(height);
        BezierSpline spline = gameObject.AddComponent<BezierSpline>();
        float localBranchSize = (data.TrunkRadiusCurve.Evaluate(height) * data.TrunkSize)/ (data.TrunkRadiusCurve.Evaluate(0) * data.TrunkSize );
        localBranchSize *= data.BranchSize;
        spline.Setup(BezierControlPointMode.Free, branchBase);
        if (FlipCoin())
        {
            spline.AddCurveType(CurveType.Straight, rotation, localBranchSize * data.size);
            spline.AddCurveType(FlipCoin() ? CurveType.Hard90 : CurveType.Wave, rotation, localBranchSize * data.size);
        }
        else
        {
            spline.AddCurveType(FlipCoin() ? CurveType.Hard90 : CurveType.Wave, rotation, localBranchSize * data.size);
            spline.AddCurveType(CurveType.Straight, rotation, localBranchSize * data.size);
        }

        // ADD LEAVES
        int numLeaves = Random.Range(data.minLeaves, data.maxLeaves + 1);
        for (int n = numLeaves; n > 0; n--)
        {
            Vector3 pos = spline.GetPoint((1f / numLeaves) * n);
            GameObject go = Instantiate(data.TMPLeafPrefab);
            go.transform.position = transform.TransformPoint(pos + new Vector3(0, FlipCoin() ? 0.2f : -0.2f, 0));// * localBranchSize * data.TrunkSize * data.size;
            go.transform.rotation *= rotation;
            go.transform.SetParent(transform, true);
        }

        branchSplines.Add(spline);
        GenerateMeshAlongSpline(spline, 1, data.TrunkHeightSegmentCount, data.BranchRadialSegmentCount, data.BranchRadiusCurve, localBranchSize * data.TrunkSize * data.size);
    }

    public void GenerateMeshAlongSpline(BezierSpline spline, float splineLength, int heightSegmentCount, int radialSegmentCount, AnimationCurve radiusCurve, float radiusMod)
    {
        //meshBuilder = new MeshBuilder();

        Vector3 centrePos = Vector3.zero;

        Vector3 lastSplineDir = Vector3.zero;
        Vector3 up = Vector3.forward;

        for (int i = 0; i <= heightSegmentCount; i++)
        {
            bool adjustUpRot = false;
            bool adjustForwardRot = false;
            float t = (float)i / heightSegmentCount;

            centrePos = spline.GetPoint(t * splineLength);

            Vector3 splineDir = spline.GetDirection(t * splineLength);

            Vector3 curFlatUp = Vector3.ProjectOnPlane(splineDir, Vector3.up).normalized;
            Vector3 curFlatRight = Vector3.ProjectOnPlane(splineDir, Vector3.right).normalized;
            Vector3 curFlatForward = Vector3.ProjectOnPlane(splineDir, Vector3.forward).normalized;

            Vector3 lastFlatUp = Vector3.ProjectOnPlane(lastSplineDir, Vector3.up).normalized;
            Vector3 lastFlatRight = Vector3.ProjectOnPlane(lastSplineDir, Vector3.right).normalized;
            Vector3 lastFlatForward = Vector3.ProjectOnPlane(lastSplineDir, Vector3.forward).normalized;

            float dotProdUp = Vector3.Dot(lastFlatUp, curFlatUp);
            float dotProdRight = Vector3.Dot(lastFlatRight, curFlatRight);
            float dotProdForward = Vector3.Dot(lastFlatForward, curFlatForward);

            if (dotProdUp < 0)
            {
                DebugExtension.DebugArrow(transform.TransformPoint(centrePos), curFlatUp, Color.cyan, 600, false);
            }
            if (dotProdRight < 0)
            {
                DebugExtension.DebugArrow(transform.TransformPoint(centrePos), curFlatRight, Color.magenta, 600, false);
            }
            if (dotProdForward < 0)
            {
                DebugExtension.DebugArrow(transform.TransformPoint(centrePos), curFlatForward, Color.yellow, 600, false);
            }

            // IF there are sharp turns adjust the 'up' direction to avoid twists
            if (dotProdUp < 0)
            {
                up = -up;
                DebugExtension.DebugWireSphere(transform.TransformPoint(centrePos), Color.cyan, 0.22f, 600, false);
                adjustUpRot = true;
            }
            if (dotProdForward < 0)
            {
                up = Quaternion.Euler(0, 90, 0) * up;
                DebugExtension.DebugWireSphere(transform.TransformPoint(centrePos), Color.yellow, 0.2f, 600, false);
                adjustForwardRot = true;
            }
         
            // Rotation of ring (up is forward...)
            Quaternion rotation = YLookRotation(splineDir, up);
     
            // Build ring
            BuildRing(meshBuilder, radialSegmentCount, centrePos, radiusCurve.Evaluate(t) * radiusMod, t, i > 0, rotation, adjustUpRot, adjustForwardRot);

            if (adjustUpRot) up = -up;
            if (adjustForwardRot) up = Quaternion.Euler(0, -90, 0) * up;
            lastSplineDir = splineDir;
        }

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

    float lastRandomMod = -1f;

    void BuildRing(MeshBuilder meshBuilder, int segmentCount, Vector3 centre, float radius, float v, bool buildTriangles, Quaternion rotation, bool adjustVertRot = false, bool adjustForwardRot = false)
    {

        float angleInc = (Mathf.PI * 2.0f) / segmentCount;

        float firstRandom = Random.Range(0.8f, 1.2f);

        for (int i = 0; i <= segmentCount; i++)
        {
            float angle = angleInc * i;

            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.z = Mathf.Sin(angle);
            
            if (adjustVertRot)
            {
                unitPosition = Quaternion.Euler(0, 180, 0) * unitPosition;
            }
            if (adjustForwardRot)
            {
                unitPosition = Quaternion.Euler(0, 90, 0) * unitPosition;
            }

            unitPosition = rotation * unitPosition;

            float randomWobble = (i == 0 || i == segmentCount) ? firstRandom : Random.Range(0.8f, 1.2f);

            Vector3 vertexPosition = centre + unitPosition * radius * randomWobble;

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


}
