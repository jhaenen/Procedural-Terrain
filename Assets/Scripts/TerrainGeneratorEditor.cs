using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        TerrainGenerator terGen = (TerrainGenerator)target;

        DrawDefaultInspector(); 

        if(GUILayout.Button("Generate Terrain")) {
            terGen.CreateMesh();
            terGen.AttachMesh();
            terGen.UpdateMesh();
        }
    }
}
