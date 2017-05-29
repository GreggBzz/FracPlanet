using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetMesh : MonoBehaviour {
  
    // terrain, ocean, atmosphere?
    public string planetLayer;
    public bool rotate = true;

    public Mesh mesh;
    public Vector3 center = new Vector3(0, 0, 0);

    // manage textures and special layers.
    private PlanetTexture textureManager;
    private PlanetOcean oceanManager;
    private PlanetCloud cloudManager;
    
    // mesh geometry setup is done in another class.
    private PlanetFullMesh fullMesh;

    public void GenerateFull(string curPlanetLayer, float curDiameter, int curPlanetSeed = 100) {
        textureManager = gameObject.AddComponent<PlanetTexture>();
        oceanManager = gameObject.AddComponent<PlanetOcean>();
        cloudManager = gameObject.AddComponent<PlanetCloud>();
        fullMesh = gameObject.AddComponent<PlanetFullMesh>();
        planetLayer = curPlanetLayer;

        MeshCollider planetCollider = gameObject.AddComponent<MeshCollider>();
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Geosphere";

        fullMesh.Generate(curPlanetLayer, curDiameter, curPlanetSeed);
        mesh.vertices = fullMesh.GetVerts();

        switch (curPlanetLayer) {
            case "":
                break;
            case "ocean":
                mesh.uv = textureManager.Texture(fullMesh.GetVertIndex(), fullMesh.GetParentVertIndex(), 
                                                 fullMesh.GetVerts(), fullMesh.GetTriangles());
                oceanManager.InitializeWaves(fullMesh.GetVertIndex());
                mesh.triangles = fullMesh.GetTriangles();
                recalc(false);
                break;
            case "atmosphere":
                mesh.uv = textureManager.Texture(fullMesh.GetVertIndex(), fullMesh.GetParentVertIndex(),
                                                 fullMesh.GetVerts(), fullMesh.GetTriangles());
                mesh.triangles = fullMesh.GetTriangles();
                recalc(false);
                break;
            case "cloud":
                // we need to modify the cloud mesh a bit to avoid the texture seam problem.
                mesh.vertices = cloudManager.TextureCloudMesh(fullMesh.GetVerts(), fullMesh.GetTriangles());
                mesh.uv = cloudManager.newUv;
                mesh.triangles = cloudManager.newTriangles;
                cloudManager.newTriangles = null; cloudManager.newUv = null;
                recalc(false);
                break;
            case "terrain":
                mesh.uv = textureManager.Texture(fullMesh.GetVertIndex(), fullMesh.GetParentVertIndex(),
                                                 fullMesh.GetVerts(), fullMesh.GetTriangles());
                mesh.uv4 = textureManager.AssignSplatElev(fullMesh.GetVertIndex(), fullMesh.GetParentVertIndex(),
                                                          fullMesh.GetVerts());
                mesh.triangles = fullMesh.GetTriangles();
                planetCollider.sharedMesh = mesh;
                recalc();
                break;
            default:
                break;
        }
        // clean up.
        fullMesh = null;
    }

    private void recalc(bool bounds = true) {
        if (bounds) { mesh.RecalculateBounds(); }
        mesh.RecalculateNormals();
    }

    public float GetMaxElevation() {
        return textureManager.maxElev;
    }

    void Update() {
        /// make waves every other frame.
        if (planetLayer == "ocean" && oceanManager.skipframe) {
            mesh.vertices = oceanManager.MakeWaves(mesh.vertices, center);
            recalc(false);
        }
        oceanManager.skipframe = !oceanManager.skipframe;

        if (planetLayer == "ocean") {
            transform.Rotate(Vector3.up, (Time.deltaTime / 15) * (oceanManager.tideStrength - 1.5F));
        }
        // if we teleport, stop rotating.
        if (rotate) {
            if (planetLayer == "cloud") {
                transform.Rotate(Vector3.up, Time.deltaTime * .45F);
            }
            else {
                transform.Rotate(Vector3.up, Time.deltaTime * 1F);
            }
        }
    }
}
