using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 20;
    public int zSize = 20;

    void Start() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape() { 
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
    	triangles = new int[xSize * zSize * 2 * 3];

        for(int x = 0; x <= xSize; x++) {
            for(int z = 0; z <= zSize; z++) {
                vertices[x * (zSize + 1) + z] = new Vector3(x, 0, z);
                if(z != zSize && x != xSize) {
                    triangles[(x * zSize + z) * 6] = x * (zSize + 1) + z;
                    triangles[(x * zSize + z) * 6 + 1] = x * (zSize + 1) + z + 1;
                    triangles[(x * zSize + z) * 6 + 2]  = (x + 1) * (zSize + 1) + z + 1;

                    triangles[(x * zSize + z) * 6 + 3] = x * (zSize + 1) + z;
                    triangles[(x * zSize + z) * 6 + 4] = (x + 1) * (zSize + 1) + z + 1;
                    triangles[(x * zSize + z) * 6 + 5] = (x + 1) * (zSize + 1) + z;
                }
            }
        }
    }

    void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos() {
        if(vertices == null) return;

        for(int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], .1f);
        } 
    }
}
