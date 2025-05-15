using UnityEngine;

public static class MeshUtils {

    public static readonly Mesh QuadMesh = new Mesh() {
        vertices = new Vector3[] { 
            new(-1.0f, -1.0f, 0.0f), 
            new(-1.0f, 1.0f, 0.0f), 
            new(1.0f, -1.0f, 0.0f), 
            new(1.0f, 1.0f, 0.0f)
            },
        triangles = new int[] {
            0, 3, 2,
            0, 1, 3
        },
        uv = new Vector2[] {
            new(0.0f, 0.0f),
            new(0.0f, 1.0f),
            new(1.0f, 0.0f),
            new(1.0f, 1.0f)
        }
    };

}