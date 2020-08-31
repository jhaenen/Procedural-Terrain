using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class TerrainGenerator : MonoBehaviour {
    Mesh mesh;
    Renderer textureRenderer;
    float[,] noiseMap;
    public enum DrawMode { textureMode, colorMode, meshMode };

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    [Header("Terrain Dimensions")]
    public int terrainWidth = 20; //x
    public int terrainHeight = 20; //z
    public bool useChunks = false;
    [Range(0, 6)]
    public int levelOfDetail = 0;

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
    public Material material;
    public Gradient colorGradient;
    public bool coloredMesh = true;

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
        GameObject go = new GameObject("Chunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        go.transform.parent = transform;
        go.GetComponent<Renderer>().material = material;

        mesh = new Mesh();
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.GetComponent<MeshCollider>().sharedMesh = mesh;
        textureRenderer = go.GetComponent<MeshRenderer>();
        textureRenderer.material = material;

        int inc = (useChunks && levelOfDetail != 0) ? levelOfDetail * 2 : 1;
        int xVerts = (terrainWidth - 1) / inc + 1;
        int zVerts = (terrainHeight - 1) / inc + 1;

        vertices = new Vector3[xVerts * zVerts];
    	triangles = new int[(xVerts - 1) * (zVerts - 1) * 6];
        uvs = new Vector2[xVerts * zVerts];

        noiseMap = GenerateNoiseMap();

        for(int x = 0; x < terrainWidth; x += inc) {
            for(int z = 0; z < terrainHeight; z += inc) {
                int xNorm = x / inc;
                int zNorm = z / inc;
                
                vertices[xNorm * zVerts + zNorm] = new Vector3(x, (drawMode == DrawMode.meshMode) ? elevationCurve.Evaluate(noiseMap[x, z]) * elevation : 0, z);
                uvs[xNorm * zVerts + zNorm] = new Vector2(x/(float)terrainWidth, z/(float)terrainHeight);

                if(z != (terrainHeight - 1) && x != terrainWidth - 1) {
                    triangles[(xNorm * (zVerts - 1) + zNorm) * 6] = xNorm * zVerts + zNorm;
                    triangles[(xNorm * (zVerts - 1) + zNorm) * 6 + 1] = xNorm * zVerts + zNorm + 1;
                    triangles[(xNorm * (zVerts - 1) + zNorm) * 6 + 2]  = (xNorm + 1) * zVerts + zNorm + 1;

                    triangles[(xNorm * (zVerts - 1) + zNorm) * 6 + 3] = xNorm * zVerts + zNorm;
                    triangles[(xNorm * (zVerts - 1) + zNorm) * 6 + 4] = (xNorm + 1) * zVerts + zNorm + 1;
                    triangles[(xNorm * (zVerts - 1) + zNorm) * 6 + 5] = (xNorm + 1) * zVerts + zNorm;
                }
            }
        }

        if(drawMode == DrawMode.textureMode) drawNoiseMap(noiseMap);
        else if(drawMode == DrawMode.colorMode || (drawMode == DrawMode.meshMode && coloredMesh)) drawColorMap(noiseMap);
        else textureRenderer.sharedMaterial.mainTexture = null;
    }

    public void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    public void drawNoiseMap(float[,] noiseMap) {
        Texture2D texture = new Texture2D(terrainWidth, terrainHeight);
        Color[] colorMap = new Color[terrainWidth * terrainHeight];

        for(int x = 0; x < terrainWidth; x++) {
            for(int z = 0; z < terrainHeight; z++) {
                colorMap[z + terrainHeight * x] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
    }

    public void drawColorMap(float[,] noiseMap) {
        Texture2D texture = new Texture2D(terrainWidth, terrainHeight);
        Color[] colorMap = new Color[terrainWidth * terrainHeight];

        for(int x = 0; x < terrainWidth; x++) {
            for(int z = 0; z < terrainHeight; z++) {
                colorMap[x + terrainWidth * z] = colorGradient.Evaluate(noiseMap[x, z]);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
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
        if(!useChunks) {
            if(terrainWidth < 1) terrainWidth = 1;
            if(terrainWidth > 255) terrainWidth = 255;
            if(terrainHeight < 1) terrainHeight = 1;
            if(terrainHeight > 255) terrainHeight = 255;
        } else {
            terrainWidth = 241;
            terrainHeight = 241;
        }
        if(noiseScale < 0) noiseScale = 0;
        if(octaves < 0) octaves = 0;
        if(lacunarity < 0) lacunarity = 0;
        if(elevation < 0) elevation = 0;
    }

    private void Update() {
        if(coloredMesh && textureRenderer.sharedMaterial.mainTexture == null) drawColorMap(noiseMap);
    }
}