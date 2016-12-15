using UnityEngine;
using System.Collections;
using System;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Geosphere : MonoBehaviour {

    public float pScale;
    private int[] continents = new int[7];
    public Mesh mesh;

    private int numTimesDisplaced = 0;
    private int numTimesExtended = 0;
    private int[] triangles;
    private int[] tempTriangles;
    private int numTris;
    private double radius;
    private Vector3[] vertices = new Vector3[10242];
        

    // keep track of how many new vertices we've added during tesselate
    private int startVertIndex;
    private int newVertIndex;

    // keep track of the rays for which we've caclulated the midpoint, for
    // quick refrence.
    private struct doneMidpoint {
        public bool done;
        public int mvert;
    }
    private doneMidpoint[,] doneMidpoints = new doneMidpoint[10242,10242];

    private System.Random rnd = new System.Random();
    private void Awake() {
    }

    public void Generate(Vector3 pos, Vector3 angle) {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Geosphere";

        // the original verticies are immuatable and will never 
        // be disaplaced, based on a geosphere with 50F radius.

        vertices[0].x = -26.2865543F / pScale; vertices[0].y = 0.0F / pScale; vertices[0].z = -42.53254F / pScale;
        vertices[1].x = 26.2865543F / pScale; vertices[1].y = 0.0F / pScale; vertices[1].z = -42.53254F / pScale;
        vertices[2].x = 0.0F / pScale; vertices[2].y = -42.53254F / pScale; vertices[2].z = -26.2865543F / pScale;
        vertices[3].x = -42.53254F / pScale; vertices[3].y = -26.2865543F / pScale; vertices[3].z = 0.0F / pScale;
        vertices[4].x = 0.0F / pScale; vertices[4].y = -42.53254F / pScale; vertices[4].z = 26.2865543F / pScale;
        vertices[5].x = 42.53254F / pScale; vertices[5].y = -26.2865543F / pScale; vertices[5].z = 0.0F / pScale;
        vertices[6].x = 42.53254F / pScale; vertices[6].y = 26.2865543F / pScale; vertices[6].z = 0.0F / pScale;
        vertices[7].x = 26.2865543F / pScale; vertices[7].y = 0.0F / pScale; vertices[7].z = 42.53254F / pScale;
        vertices[8].x = -26.2865543F / pScale; vertices[8].y = 0.0F / pScale; vertices[8].z = 42.53254F / pScale;
        vertices[9].x = 0.0F / pScale; vertices[9].y = 42.53254F / pScale; vertices[9].z = 26.2865543F / pScale;
        vertices[10].x = 0.0F / pScale; vertices[10].y = 42.53254F / pScale; vertices[10].z = -26.2865543F / pScale;
        vertices[11].x = -42.53254F / pScale; vertices[11].y = 26.2865543F / pScale; vertices[11].z = 0.0F / pScale;
        newVertIndex = 12;

        radius = 50.0F / pScale;

        // 20 triangles for now
        triangles = new int[60];
        triangles[0] = 1; triangles[1] = 2; triangles[2] = 0;
        triangles[3] = 11; triangles[4] = 8; triangles[5] = 9;
        triangles[6] = 4; triangles[7] = 2; triangles[8] = 5;
        triangles[9] = 9; triangles[10] = 6; triangles[11] = 10;
        triangles[12] = 10; triangles[13] = 0; triangles[14] = 11;
        triangles[15] = 7; triangles[16] = 5; triangles[17] = 6;
        triangles[18] = 2; triangles[19] = 3; triangles[20] = 0;
        triangles[21] = 8; triangles[22] = 4; triangles[23] = 7;
        triangles[24] = 5; triangles[25] = 1; triangles[26] = 6;
        triangles[27] = 3; triangles[28] = 4; triangles[29] = 8;
        triangles[30] = 6; triangles[31] = 1; triangles[32] = 10;
        triangles[33] = 11; triangles[34] = 3; triangles[35] = 8;
        triangles[36] = 1; triangles[37] = 0; triangles[38] = 10;
        triangles[39] = 9; triangles[40] = 7; triangles[41] = 6;
        triangles[42] = 0; triangles[43] = 3; triangles[44] = 11;
        triangles[45] = 4; triangles[46] = 5; triangles[47] = 7;
        triangles[48] = 2; triangles[49] = 4; triangles[50] = 3;
        triangles[51] = 10; triangles[52] = 11; triangles[53] = 9;
        triangles[54] = 5; triangles[55] = 2; triangles[56] = 1;
        triangles[57] = 8; triangles[58] = 7; triangles[59] = 9;

        // tesselate 2 times.
        for (int i = 0; i <= 4; i++) {
            tempTriangles = Tesselate(triangles);
            triangles = null;
            triangles = tempTriangles;
            tempTriangles = null;
        }

        // normalize to max radius, apply the texture and assign the vertices and triangles
        mesh.vertices = vertices;
        TexturePlanet();
        mesh.triangles = triangles;

        // center mesh, translate to controller hit position and eurlerangles
        transform.Translate(pos);
        transform.Rotate(angle);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private int[] Tesselate(int[] curTriangles) {

        int[] newTriangles = new int[curTriangles.Length * 4];
        int newTriangleIndex = 0;
        float lastDisplaceMag = 0.0F;

        // process a triangle for midpoints
        startVertIndex = newVertIndex;
        int Num = 0;
        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            int[] newMidpoints = new int[3];
            float[] dispalceMag = new float[3];
            int skip;

            // check our 2d refrence array to see if we've done this midpoint, if we have use the vert that's
            // refrenced, otherwise caclulate it. Also, calculate the displaceMag for furture refrence.

            skip = CheckMidpoint(curTriangles[i], curTriangles[i + 1]);
            dispalceMag[0] = Vector3.Distance(vertices[curTriangles[i]], vertices[curTriangles[i + 1]]);
            if (skip == -1) {
                vertices[newVertIndex] = Midpoint(vertices[curTriangles[i]], vertices[curTriangles[i + 1]]);
                newMidpoints[0] = newVertIndex;
                doneMidpoints[curTriangles[i], curTriangles[i + 1]].done = true;
                doneMidpoints[curTriangles[i], curTriangles[i + 1]].mvert = newVertIndex;
                doneMidpoints[curTriangles[i + 1], curTriangles[i]].done = true;
                doneMidpoints[curTriangles[i + 1], curTriangles[i]].mvert = newVertIndex;
                newVertIndex += 1;
            }
            else {
                newMidpoints[0] = skip;
            }

            skip = CheckMidpoint(curTriangles[i + 1], curTriangles[i + 2]);
            dispalceMag[1] = Vector3.Distance(vertices[curTriangles[i + 1]], vertices[curTriangles[i + 2]]);
            if (skip == -1) {
                vertices[newVertIndex] = Midpoint(vertices[curTriangles[i + 1]], vertices[curTriangles[i + 2]]);
                newMidpoints[1] = newVertIndex;
                doneMidpoints[curTriangles[i + 1], curTriangles[i + 2]].done = true;
                doneMidpoints[curTriangles[i + 1], curTriangles[i + 2]].mvert = newVertIndex;
                doneMidpoints[curTriangles[i + 2], curTriangles[i + 1]].done = true;
                doneMidpoints[curTriangles[i + 2], curTriangles[i + 1]].mvert = newVertIndex;
                newVertIndex += 1;
            }
            else {
                newMidpoints[1] = skip;
            }

            skip = CheckMidpoint(curTriangles[i + 2], curTriangles[i]);
            dispalceMag[1] = Vector3.Distance(vertices[curTriangles[i + 2]], vertices[curTriangles[i]]);
            if (skip == -1) {
                vertices[newVertIndex] = Midpoint(vertices[curTriangles[i + 2]], vertices[curTriangles[i]]);
                newMidpoints[2] = newVertIndex;
                doneMidpoints[curTriangles[i + 2], curTriangles[i]].done = true;
                doneMidpoints[curTriangles[i + 2], curTriangles[i]].mvert = newVertIndex;
                doneMidpoints[curTriangles[i], curTriangles[i + 2]].done = true;
                doneMidpoints[curTriangles[i], curTriangles[i + 2]].mvert = newVertIndex;
                newVertIndex += 1;
            }
            else {
                newMidpoints[2] = skip;
            }

            // displace the new midpoints
            for (int i2 = 0; i2 <= 2; i2++) {
                vertices[newMidpoints[i2]] = DisplaceMidpoint(vertices[newMidpoints[i2]], dispalceMag[i2]);
            }

            // build our new trianlges from the bisected vertices
            // 1
            newTriangles[newTriangleIndex] = newMidpoints[0];
            newTriangles[newTriangleIndex + 1] = curTriangles[i + 1];
            newTriangles[newTriangleIndex + 2] = newMidpoints[1];
            // 2
            newTriangles[newTriangleIndex + 3] = curTriangles[i];
            newTriangles[newTriangleIndex + 4] = newMidpoints[0];
            newTriangles[newTriangleIndex + 5] = newMidpoints[2];
            // 3
            newTriangles[newTriangleIndex + 6] = newMidpoints[0];
            newTriangles[newTriangleIndex + 7] = newMidpoints[1];
            newTriangles[newTriangleIndex + 8] = newMidpoints[2];
            // 4
            newTriangles[newTriangleIndex + 9] = newMidpoints[2];
            newTriangles[newTriangleIndex + 10] = newMidpoints[1];
            newTriangles[newTriangleIndex + 11] = curTriangles[i + 2];

            newTriangleIndex += 12;
        }
        Debug.Log("Number of DisplaceMags calulated: " + Num);
        Debug.Log("Last DisplaceMag: " + lastDisplaceMag);
        return newTriangles;
    }
    
    private Vector3 Midpoint(Vector3 p1, Vector3 p2) {
        float rApprox1 = (float)Math.Sqrt((p1.x * p1.x) + (p1.y * p1.y) + (p1.z * p1.z));
        float rApprox2 = (float)Math.Sqrt((p2.x * p2.x) + (p2.y * p2.y) + (p2.z * p2.z));
        float rApprox = ((rApprox1 + rApprox2) / 2.0F);
        Vector3 aMidpoint = new Vector3((p1.x + p2.x) / 2, (p1.y + p2.y) / 2, (p1.z + p2.z) / 2);
        return ExtendMidpoint(aMidpoint, rApprox);
    }

    private Vector3 ExtendMidpoint(Vector3 p1, float rApprox) {
        // calculate the current length and extend to the radius
        float currentLength = (float)Math.Sqrt((p1.x * p1.x) + (p1.y * p1.y) + (p1.z * p1.z));
        float lengthMult = (rApprox / currentLength);
        Vector3 extendedP1 = p1 * lengthMult;
        return extendedP1;
    }

    private Vector3 DisplaceMidpoint(Vector3 p1, float displaceMag) {
        if (rnd.NextDouble() < .5F) {
            return p1 * (float)((displaceMag * displaceMag * .00001F) * rnd.NextDouble() + 1.0F);
        }
        else {
            return p1 * (1.0F - (float)((displaceMag * displaceMag * .00001F) * rnd.NextDouble())); 
        }
    }

    public void TexturePlanet() {
        Vector2[] uv = new Vector2[newVertIndex];
        float minElev = (float)radius * 100F;
        float maxElev = 0F;
        float[] vertLength = new float[newVertIndex];

        // extend all short vertices to even with the radius to simualte an ocean
        for (int i = 0; i <= vertices.Length - 1; i++) {
            vertLength[i] = (float)Math.Sqrt((vertices[i].x * vertices[i].x) +
                                             (vertices[i].y * vertices[i].y) +
                                             (vertices[i].z * vertices[i].z));
            //if (vertLength[i] < radius) {
            //    float lengthmult = (float)(radius / vertLength[i]);
            //    vertices[i] = vertices[i] * lengthmult;
            //   vertLength[i] = (float)Math.Sqrt((vertices[i].x * vertices[i].x) +
            //                     (vertices[i].y * vertices[i].y) +
            //                     (vertices[i].z * vertices[i].z));
            //}
            // Determind the maximum and minmum elevations to aid in texture UV assignments
            if (vertLength[i] <= minElev) { minElev = vertLength[i]; }
            if (vertLength[i] >= maxElev) { maxElev = vertLength[i]; }
        }
        Debug.Log("Radius:" + radius);
        Debug.Log("maxElev:" + maxElev);
        Debug.Log("minElev:" + minElev);

        for (int i = 0; i <= vertices.Length - 1; i++) {
            uv[i].y = 1 - (vertLength[i] - minElev) / (maxElev - minElev);
            uv[i].x = 1 - (vertLength[i] - minElev) / (maxElev - minElev);
        }
        mesh.uv = uv;
    }

    private int CheckMidpoint(int vIndex1, int vIndex2) {
        // check our 2d array for a previously done midpoint that was between two verts
        if ((doneMidpoints[vIndex1, vIndex2].done) || (doneMidpoints[vIndex2, vIndex1].done)) { 
                return doneMidpoints[vIndex1, vIndex2].mvert;
        }
        return -1;
    }
            
    // use this for initialization
    void Start() {

    }

    // update is called once per frame, rotate the planet
    void Update() {
       transform.Rotate(Vector3.up, 6F * Time.deltaTime);
    }
}
