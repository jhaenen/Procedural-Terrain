using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class TerrainGenerator : MonoBehaviour {
    Mesh mesh;
    Renderer textureRenderer;
    public Texture2D curText;
    public enum DrawMode { textureMode, colorMode, meshMode };

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    [Header("Terrain Dimensions")]
    public int terrainWidth = 20; //x
    public int terrainHeight = 20; //z

    [Header("Terrain Control")]
    public float noiseScale = 3;
    public int octaves = 1;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2;
    public int seed = 0;
    public Vector2 sampleOffset = new Vector2(0, 0);
    public float elevation = 1;
    public AnimationCurve elevationCurve;

    [Header("Color Settings")]
    public Gradient colorGradient;
    public bool colorMesh = true;

    [Header("Other Settings")]
    public DrawMode drawMode;
    public bool enableVertexGizmos = false;
    public bool autoUpdate = false;

    void Start() {
        CreateMesh();
        UpdateMesh();
    }

    float[,] GenerateNoiseMap() {
        float [,] noiseMap = new float[terrainWidth, terrainHeight];

        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++) {
            octaveOffsets[i] = new Vector2(rng.Next(-100000, 100000) + sampleOffset.x, rng.Next(-100000, 100000) + sampleOffset.y);
        }

        float tmpScale = noiseScale;
        if(tmpScale <= 0) tmpScale = 0.001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for(int x = 0; x < terrainWidth; x++) {
            for(int z = 0; z < terrainHeight; z++) {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for(int i = 0; i < octaves; i++) {
                    float sampleX = (x - terrainWidth / 2) / tmpScale * frequency + octaveOffsets[i].x;
                    float sampleZ = (z - terrainHeight / 2) / tmpScale * frequency + octaveOffsets[i].y;

                    noiseHeight += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude * 2 - 1;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if(noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                noiseMap[x,z] = noiseHeight;
            }
        }

        for(int x = 0; x < terrainWidth; x++) {
            for(int z = 0; z < terrainHeight; z++) {
                noiseMap[x,z] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, z]);
            }
        }

        return noiseMap;
    }

    public void CreateMesh() { 
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        textureRenderer = GetComponent<MeshRenderer>();

        vertices = new Vector3[terrainWidth * terrainHeight];
    	triangles = new int[(terrainWidth - 1) * (terrainHeight -1) * 6];
        uvs = new Vector2[terrainWidth * terrainHeight];

        float[,] noiseMap = GenerateNoiseMap();

        for(int x = 0; x < terrainWidth; x++) {
            for(int z = 0; z < terrainHeight; z++) {
                vertices[x * terrainHeight + z] = new Vector3(x, (drawMode == DrawMode.meshMode) ? elevationCurve.Evaluate(noiseMap[x, z]) * elevation : 0, z);
                uvs[x * terrainHeight + z] = new Vector2(x/(float)terrainWidth, z/(float)terrainHeight);

                if(z != (terrainHeight - 1) && x != terrainWidth - 1) {
                    triangles[(x * (terrainHeight - 1) + z) * 6] = x * terrainHeight + z;
                    triangles[(x * (terrainHeight - 1) + z) * 6 + 1] = x * terrainHeight + z + 1;
                    triangles[(x * (terrainHeight - 1) + z) * 6 + 2]  = (x + 1) * terrainHeight + z + 1;

                    triangles[(x * (terrainHeight - 1) + z) * 6 + 3] = x * terrainHeight + z;
                    triangles[(x * (terrainHeight - 1) + z) * 6 + 4] = (x + 1) * terrainHeight + z + 1;
                    triangles[(x * (terrainHeight - 1) + z) * 6 + 5] = (x + 1) * terrainHeight + z;
                }
            }
        }

        if(drawMode == DrawMode.textureMode) drawNoiseMap(noiseMap);
        else if(drawMode == DrawMode.colorMode || (drawMode == DrawMode.meshMode && colorMesh)) drawColorMap(noiseMap);
        //else textureRenderer.sharedMaterial.mainTexture = null;
    }

    public void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    public void drawNoiseMap(float[,] noiseMap) {
        curText = new Texture2D(terrainWidth, terrainHeight);
        Color[] colorMap = new Color[terrainWidth * terrainHeight];

        for(int x = 0; x < terrainWidth; x++) {
            for(int z = 0; z < terrainHeight; z++) {
                colorMap[z + terrainHeight * x] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);
            }
        }

        curText.SetPixels(colorMap);
        curText.Apply();

        textureRenderer.sharedMaterial.mainTexture = curText;
    }

    public void drawColorMap(float[,] noiseMap) {
        curText = new Texture2D(terrainWidth, terrainHeight);
        Color[] colorMap = new Color[terrainWidth * terrainHeight];

        for(int x = 0; x < terrainWidth; x++) {
            for(int z = 0; z < terrainHeight; z++) {
                colorMap[x + terrainWidth * z] = colorGradient.Evaluate(noiseMap[x, z]);
            }
        }

        curText.filterMode = FilterMode.Point;
        curText.SetPixels(colorMap);
        curText.Apply();

        textureRenderer.sharedMaterial.mainTexture = curText;
    }

    private void OnDrawGizmos() {
        if(enableVertexGizmos) {
            if(vertices == null) return;

            for(int i = 0; i < vertices.Length; i++) {
                Gizmos.DrawSphere(vertices[i], .1f);
            } 
        }
    }

    private void OnValidate() {
        if(terrainWidth < 1) terrainWidth = 1;
        if(terrainWidth > 255) terrainWidth = 255;
        if(terrainHeight < 1) terrainHeight = 1;
        if(terrainHeight > 255) terrainHeight = 255;
        if(noiseScale < 0) noiseScale = 0;
        if(octaves < 0) octaves = 0;
        if(lacunarity < 0) lacunarity = 0;
        if(elevation < 0) elevation = 0;
    }
}