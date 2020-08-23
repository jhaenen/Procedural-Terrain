using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        TerrainGenerator terGen = (TerrainGenerator)target;
        
        if(DrawDefaultInspector()) {
            if(terGen.autoUpdate) {
                terGen.CreateMesh();
                terGen.UpdateMesh();
            }
        } 

        if(GUILayout.Button("Generate Terrain")) {
            terGen.CreateMesh();
            terGen.UpdateMesh();
        }
    }
}
