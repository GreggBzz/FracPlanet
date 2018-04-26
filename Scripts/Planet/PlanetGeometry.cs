using UnityEngine;
using System;

// Setup mesh geometry, tesselate, bisect vectors, extend midpoints etc..
public class PlanetGeometry : MonoBehaviour {

    // is this a full mesh, or a more detailed partial mesh?
    private bool aFullSetup = true;
    
    // keep track of how many new vertices we've added during tesselate.
    // and the parent vertices, the center of each hexagon.
    public int newVertIndex;
    public float diameter;

    public string planetLayer; // terrain, ocean, atmosphere?

    // set to 10242 for 4 rounds and 40962 for 5 rounds.
    private const int vertCount = 40962;
    public int tessRounds = 5;

    // how bumpy is our planet?
    private const float maxRoughness = .000025F;
    private const float minRoughness = .000015F;
    private float roughness;

    private int[] triangles;
    private int[] tempTriangles;
    private int[] vertSeed;
    private int vertSeedCount = 0;

    // scale variables.
    private float radius;
    private float pScale;

    private struct doneMidpoint {
        public int adjVert;
        public int midPoint;
    }

    // A detail class for adding rivers, mountains, plains and platues. 
    private PlanetGeometryDetail detailMaker;
    
    // setup the random generator and seed.
    private System.Random rnd;
    int seed;

    private Vector3[] vertices = new Vector3[vertCount];
    private doneMidpoint[,] doneMidpoints = new doneMidpoint[vertCount, 6];

    public void Generate(string curPlanetLayer, float curDiameter, int curPlanetSeed = 100, bool fullSetup = true) {
        // setup random seed and assign the planet layer.
        if (GameObject.Find("Controller (right)") != null) {
            seed = GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetSeed;
        }
        else {
            seed = 100;
        }

        rnd = new System.Random(seed);

        planetLayer = curPlanetLayer;
        aFullSetup = fullSetup;
        // for public access, store diameter.
        diameter = curDiameter;
        // setup roughness, planet scale, base verts and triangles.
        roughness = ((float)rnd.NextDouble() * (maxRoughness - minRoughness) + minRoughness) / (curDiameter / 2500);
        pScale = 100.0F / curDiameter;
        radius = 50.0F / pScale;

        if (fullSetup) {
            // if we're doing a full, new planet setup, create the initial geometry.
            // otherwise, it gets set by way of the public vertice[], triangle[] and newvertindex 
            // variables.
            vertices = StartVerts(pScale, vertCount);
            triangles = StartTris();
        }

        // reset the midpoints and tesselate in a loop.
        for (int i = 0; i <= tessRounds; i++) {
            ResetMidpoints();
            tempTriangles = Tesselate(triangles);
            triangles = null;
            triangles = tempTriangles;
            tempTriangles = null;
        }
        // If we're doing initial setup, add any interesting features, like valleys's and rivers.
        if ((fullSetup) && curPlanetLayer == "terrain") {
            detailMaker = gameObject.AddComponent<PlanetGeometryDetail>();
            detailMaker.Initialize(triangles, vertices, vertCount, radius, curPlanetSeed);
        }
    }

    public Vector3[] StartVerts(float pScale, int vertCount) {
        // the original vertices are points on an icosahedron (d20) with a 50 unit radius.
        Vector3[] vertices = new Vector3[vertCount];
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
        return vertices;
    }

    public int[] StartTris() {
        // 20 triangles in a 60 int array.
        int[] triangles = new int[60];
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
        return triangles;
    }

    public int[] Tesselate(int[] curTriangles) {
        int[] newTriangles = new int[curTriangles.Length * 4];
        int newTriangleIndex = 0;
        // process a triangle for midpoints
        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            int[] newMidpoints = new int[3];
            float[] dispalceMag = new float[3];
            int skip;
            // check our 2d refrence array to see if we've done this midpoint, if we have use the vert that's
            // refrenced, otherwise caclulate it. Also, calculate the displaceMag for furture refrence.
            skip = CheckMidpoint(curTriangles[i], curTriangles[i + 1]);
            dispalceMag[0] = Vector3.Distance(vertices[curTriangles[i]], vertices[curTriangles[i + 1]]);
            if (skip == -1) {
                vertices[newVertIndex] = CreateMidpoint(vertices[curTriangles[i]], vertices[curTriangles[i + 1]]);
                newMidpoints[0] = newVertIndex;
                MarkMidpointDone(curTriangles[i], curTriangles[i + 1], newVertIndex);
                newVertIndex += 1;
            }
            else { newMidpoints[0] = skip; }
            skip = CheckMidpoint(curTriangles[i + 1], curTriangles[i + 2]);
            dispalceMag[1] = Vector3.Distance(vertices[curTriangles[i + 1]], vertices[curTriangles[i + 2]]);
            if (skip == -1) {
                vertices[newVertIndex] = CreateMidpoint(vertices[curTriangles[i + 1]], vertices[curTriangles[i + 2]]);
                newMidpoints[1] = newVertIndex;
                MarkMidpointDone(curTriangles[i + 1], curTriangles[i + 2], newVertIndex);
                newVertIndex += 1;
            }
            else { newMidpoints[1] = skip; }
            skip = CheckMidpoint(curTriangles[i + 2], curTriangles[i]);
            dispalceMag[2] = Vector3.Distance(vertices[curTriangles[i + 2]], vertices[curTriangles[i]]);
            if (skip == -1) {
                vertices[newVertIndex] = CreateMidpoint(vertices[curTriangles[i + 2]], vertices[curTriangles[i]]);
                newMidpoints[2] = newVertIndex;
                MarkMidpointDone(curTriangles[i + 2], curTriangles[i], newVertIndex);
                newVertIndex += 1;
            }
            else { newMidpoints[2] = skip; }
            // displace the new midpoints
            for (int i2 = 0; i2 <= 2; i2++) {
                vertices[newMidpoints[i2]] = DisplaceMidpoint(vertices[newMidpoints[i2]], dispalceMag[i2]);
            }
            // build our new trianlges from the bisected vertices
            newTriangles[newTriangleIndex] = newMidpoints[0]; // 1
            newTriangles[newTriangleIndex + 1] = curTriangles[i + 1];
            newTriangles[newTriangleIndex + 2] = newMidpoints[1];
            newTriangles[newTriangleIndex + 3] = curTriangles[i]; // 2
            newTriangles[newTriangleIndex + 4] = newMidpoints[0];
            newTriangles[newTriangleIndex + 5] = newMidpoints[2];
            newTriangles[newTriangleIndex + 6] = newMidpoints[0]; // 3
            newTriangles[newTriangleIndex + 7] = newMidpoints[1];
            newTriangles[newTriangleIndex + 8] = newMidpoints[2];
            newTriangles[newTriangleIndex + 9] = newMidpoints[2]; // 4
            newTriangles[newTriangleIndex + 10] = newMidpoints[1];
            newTriangles[newTriangleIndex + 11] = curTriangles[i + 2];
            newTriangleIndex += 12;
        }
        return newTriangles;
    }

    private int CheckMidpoint(int v1, int v2) {
        // check to see if we've done this midpoint before.
        for (int i = 0; i <= 5; i++) {
            if (doneMidpoints[v1, i].adjVert == v2) {
                return doneMidpoints[v1, i].midPoint;
            }
        }
        for (int i = 0; i <= 5; i++) {
            if (doneMidpoints[v2, i].adjVert == v1) {
                return doneMidpoints[v2, i].midPoint;
            }
        }
        return -1;
    }

    private Vector3 DisplaceMidpoint(Vector3 p1, float displaceMag) {
        // randomly displace a midpoint up or down 50/50.
        if (planetLayer != "terrain") return p1 * 1.0F;
        if (!aFullSetup) {
            rnd = new System.Random(vertSeed[vertSeedCount]);
            vertSeedCount += 1;
        }
        if (rnd.NextDouble() < .5F) {
            return p1 * (float)((displaceMag * roughness * .9) * rnd.NextDouble() + 1.0F);
        }
        else {
            return p1 * (1.0F - (float)((displaceMag * roughness * .9) * rnd.NextDouble()));
        }
    }

    private Vector3 ExtendMidpoint(Vector3 p1, float rApprox) {
        // calculate the current length and extend to the radius, only.
        float currentLength = (float)Math.Sqrt((p1.x * p1.x) + (p1.y * p1.y) + (p1.z * p1.z));
        float lengthMult = (rApprox / currentLength);
        Vector3 extendedP1 = p1 * lengthMult;
        return extendedP1;
    }

    private Vector3 CreateMidpoint(Vector3 p1, Vector3 p2) {
        // create a midpoint.
        float rApprox;
        if (planetLayer != "terrain") {
            rApprox = (float)radius;
        }
        else {
            float rApprox1 = (float)Math.Sqrt((p1.x * p1.x) + (p1.y * p1.y) + (p1.z * p1.z));
            float rApprox2 = (float)Math.Sqrt((p2.x * p2.x) + (p2.y * p2.y) + (p2.z * p2.z));
            rApprox = ((rApprox1 + rApprox2) / 2.0F);
        }
        Vector3 aMidpoint = new Vector3((p1.x + p2.x) / 2, (p1.y + p2.y) / 2, (p1.z + p2.z) / 2);
        return ExtendMidpoint(aMidpoint, rApprox);
    }

    private void MarkMidpointDone(int v1, int v2, int mp) {
        // mark a midpoint as done so we don't do it twice.
        for (int i = 0; i <= 5; i++) {
            if (doneMidpoints[v1, i].adjVert == -1) {
                doneMidpoints[v1, i].adjVert = v2;
                doneMidpoints[v1, i].midPoint = mp;
                break;
            }
        }
        for (int i = 0; i <= 5; i++) {
            if (doneMidpoints[v2, i].adjVert == -1) {
                doneMidpoints[v2, i].adjVert = v1;
                doneMidpoints[v2, i].midPoint = mp;
                return;
            }
        }
    }

    private void ResetMidpoints() {
        // reset the refrence array for each round of tesselation.
        for (int i = 0; i <= vertCount - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                doneMidpoints[i, i2].adjVert = -1;
            }
        }
    }

    public int GetVertIndex() {
        return newVertIndex;
    }

    public int[] GetTriangles() {
        return triangles;
    }

    public Vector3[] GetVerts() {
        return vertices;
    }

    public void SetVerts(Vector3[] newVerts) {
        vertices = newVerts;
    }

    public void SetTris(int[] newTris) {
        triangles = newTris;
    }

    public void SetVertSeeds(int[] vertSeeds) {
        vertSeed = vertSeeds;
    }

}
