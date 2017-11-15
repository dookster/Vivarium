using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlantData))]
public class PlantDataEditor : Editor
{

    public override void OnInspectorGUI()
    {


        DrawDefaultInspector();
    }

}
