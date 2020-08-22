using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class TerrainGenerator : MonoBehaviour {
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int terrainWidth = 20; //x
    public int terrainHeight = 20; //z
    public float noiseScale = 3;

    void Start() {
        CreateMesh();
        AttachMesh();
        UpdateMesh();
    }

    float[,] GenerateNoiseMap() {
        float [,] noiseMap = new float[terrainWidth + 1, terrainHeight + 1];

        float tmpScale = noiseScale;
        if(tmpScale <= 0) tmpScale = 0.001f;

        for(int x = 0; x <= terrainWidth; x++) {
            for(int z = 0; z <= terrainHeight; z++) {
                noiseMap[x,z] = Mathf.PerlinNoise(x / tmpScale, z / tmpScale);
            }
        }

        return noiseMap;
    }

    public void CreateMesh() { 
        vertices = new Vector3[(terrainWidth + 1) * (terrainHeight + 1)];
    	triangles = new int[terrainWidth * terrainHeight * 2 * 3];

        float[,] noiseMap = GenerateNoiseMap();

        for(int x = 0; x <= terrainWidth; x++) {
            for(int z = 0; z <= terrainHeight; z++) {
                vertices[x * (terrainHeight + 1) + z] = new Vector3(x, noiseMap[x, z], z);
                if(z != terrainHeight && x != terrainWidth) {
                    triangles[(x * terrainHeight + z) * 6] = x * (terrainHeight + 1) + z;
                    triangles[(x * terrainHeight + z) * 6 + 1] = x * (terrainHeight + 1) + z + 1;
                    triangles[(x * terrainHeight + z) * 6 + 2]  = (x + 1) * (terrainHeight + 1) + z + 1;

                    triangles[(x * terrainHeight + z) * 6 + 3] = x * (terrainHeight + 1) + z;
                    triangles[(x * terrainHeight + z) * 6 + 4] = (x + 1) * (terrainHeight + 1) + z + 1;
                    triangles[(x * terrainHeight + z) * 6 + 5] = (x + 1) * (terrainHeight + 1) + z;
                }
            }
        }
    }

    public void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    private void OnDrawGizmos() {
        if(vertices == null) return;

        for(int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], .1f);
        } 
    }

    public void AttachMesh() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}