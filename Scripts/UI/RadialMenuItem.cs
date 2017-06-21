using UnityEngine;
using System;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class RadialMenuItem : MonoBehaviour {

    public string itemTitle;
    public float angle;
    public Mesh mesh;

    public void Generate(Vector3 sPos, int itemNumber, int totalItems) {

        // enough for a simple plane.
        Vector3 itemLoc;
        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[12];

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Radial Menu Item " + itemNumber;

        angle = 360 / totalItems * itemNumber * Mathf.Deg2Rad;
        int j = 0;

        // calculate the location of this item based on it's number.
        itemLoc.z = sPos.z + (float)(.06F * Math.Sin(angle));
        itemLoc.x = sPos.x + (float)(.06F * Math.Cos(angle));
        itemLoc.y = sPos.y;

        // Convert angle back to touchpad friendly radians.
        angle = (Mathf.Atan2(itemLoc.x, itemLoc.z) + Mathf.PI);

        // build our plane, front facing.
        vertices[j].x = (itemLoc.x - .02F);
        vertices[j].z = (itemLoc.z - .02F);
        vertices[j].y = itemLoc.y;
        uv[j].x = 0F;
        uv[j].y = 0F;

        vertices[j + 1].x = (itemLoc.x - .02F);
        vertices[j + 1].z = (itemLoc.z + .02F);
        vertices[j + 1].y = itemLoc.y;
        uv[j + 1].x = 0F;
        uv[j + 1].y = 1.0F;

        vertices[j + 2].x = (itemLoc.x + .02F);
        vertices[j + 2].z = (itemLoc.z + .02F);
        vertices[j + 2].y = itemLoc.y;
        uv[j + 2].x = 1.0F;
        uv[j + 2].y = 1.0F;

        vertices[j + 3].x = (itemLoc.x + .02F);
        vertices[j + 3].z = (itemLoc.z - .02F);
        vertices[j + 3].y = itemLoc.y;
        uv[j + 3].x = 1.0F;
        uv[j + 3].y = 0F;

        triangles[j] = 0;
        triangles[j + 1] = 1;
        triangles[j + 2] = 2;
        triangles[j + 3] = 2;
        triangles[j + 4] = 3;
        triangles[j + 5] = 0;

        // build our plane, back facing.
        vertices[j + 4].x = (itemLoc.x - .02F);
        vertices[j + 4].z = (itemLoc.z - .02F);
        vertices[j + 4].y = itemLoc.y - .001F;
        uv[j + 4].x = 0F;
        uv[j + 4].y = 0F;

        vertices[j + 5].x = (itemLoc.x - .02F);
        vertices[j + 5].z = (itemLoc.z + .02F);
        vertices[j + 5].y = itemLoc.y - .001F;
        uv[j + 5].x = 0F;
        uv[j + 5].y = 1.0F;

        vertices[j + 6].x = (itemLoc.x + .02F);
        vertices[j + 6].z = (itemLoc.z + .02F);
        vertices[j + 6].y = itemLoc.y - .001F;
        uv[j + 6].x = 1.0F;
        uv[j + 6].y = 1.0F;

        vertices[j + 7].x = (itemLoc.x + .02F);
        vertices[j + 7].z = (itemLoc.z - .02F);
        vertices[j + 7].y = itemLoc.y - .001F;
        uv[j + 7].x = 1.0F;
        uv[j + 7].y = 0F;
        
        triangles[j + 6] = 4;
        triangles[j + 7] = 7;
        triangles[j + 8] = 6;
        triangles[j + 9] = 6;
        triangles[j + 10] = 5;
        triangles[j + 11] = 4;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        transform.Translate(sPos);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
