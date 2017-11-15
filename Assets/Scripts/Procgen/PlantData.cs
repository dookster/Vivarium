using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object with data describing a single plant. Only has values and very basic methods combining them.
/// </summary>
[CreateAssetMenu(fileName = "New Plant Data")]
public class PlantData : ScriptableObject
{
   
    public enum Type { Tree, Bush, Grass, Fern, Moss }

    public GameObject TMPLeafPrefab;

    public string plantName = "Plant";
    public Type type = Type.Tree;
    [Tooltip("(almost) all values are multiplied by this")]
    public float size = 1f;

    [Tooltip("This * size = trunk height in units")]
    public float height = 2f;

    [Header("Trunk")]
    public float TrunkSize = 1f;
    public AnimationCurve TrunkRadiusCurve;
    public int TrunkHeightSegmentCount = 10;
    public int TrunkRadialSegmentCount = 8;
    public Material material;
    public Gradient color;
    public List<CurveType> curves;

    [Tooltip("Relative to total height")]
    [Range(0f, 1f)]
    public float trunkHeight;
    [Header("Branch")]
    [Tooltip("Branch shape")]
    public float BranchSize = 0.5f;
    public AnimationCurve BranchRadiusCurve;
    public int BranchHeightSegmentCount = 10;
    public int BranchRadialSegmentCount = 8;

    public int minLeaves;
    public int maxLeaves;

    public int minBranches;
    public int maxBranches;
    public bool twinBranch; // make a two at a time opposite sides of trunk
    public bool sameAxisBranches; // don't rotate around the trunk axis

    public float GetTotalHeight()
    {
        return size * height;
    }

    public float GetTrunkThickness(float t)
    {
        return TrunkRadiusCurve.Evaluate(t) * TrunkSize;
    }


}
