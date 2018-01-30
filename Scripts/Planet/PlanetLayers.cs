using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetLayers : MonoBehaviour {
    // create each layer and texutre it using the texture, geometry and cloud
    // helper classes.

    // terrain, ocean, atmosphere?
    public string planetLayer;
    public bool rotate = true;

    public Mesh mesh;
    public Vector3 center = new Vector3(0, 0, 0);

    // manage textures and special layers.
    private PlanetTexture textureManager;
    private PlanetCloudsAndStars cloudManager;

    // mesh geometry setup is done in another class.
    private PlanetGeometry meshGeometry;

    public void GenerateFull(string curPlanetLayer, float curDiameter, int curPlanetSeed = 100) {
        textureManager = gameObject.AddComponent<PlanetTexture>();
        cloudManager = gameObject.AddComponent<PlanetCloudsAndStars>();
        meshGeometry = gameObject.AddComponent<PlanetGeometry>();
        planetLayer = curPlanetLayer;

        MeshCollider planetCollider = gameObject.AddComponent<MeshCollider>();
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Geosphere";

        meshGeometry.Generate(curPlanetLayer, curDiameter, curPlanetSeed);
        mesh.vertices = meshGeometry.GetVerts();

        switch (curPlanetLayer) {
            case "":
                break;
            case "ocean":
                mesh.uv = textureManager.Texture(meshGeometry.GetVertIndex(), meshGeometry.GetVerts(), meshGeometry.GetTriangles());
                mesh.triangles = meshGeometry.GetTriangles();
                recalc(false);
                break;
            case "atmosphere":
                mesh.uv = textureManager.Texture(meshGeometry.GetVertIndex(), meshGeometry.GetVerts(), meshGeometry.GetTriangles());
                mesh.triangles = meshGeometry.GetTriangles();
                recalc(false);
                break;
            case "cloud":
                // we need to modify the cloud mesh to avoid the texture seam problem.
                mesh.vertices = cloudManager.TextureCloudMesh(meshGeometry.GetVerts(), meshGeometry.GetTriangles());
                mesh.uv = cloudManager.newUv;
                mesh.triangles = cloudManager.newTriangles;
                cloudManager.newTriangles = null; cloudManager.newUv = null;
                recalc(false);
                break;
            case "terrain":
                mesh.uv = textureManager.Texture(meshGeometry.GetVertIndex(), meshGeometry.GetVerts(), meshGeometry.GetTriangles());
                mesh.uv4 = textureManager.AssignSplatElev(meshGeometry.GetVertIndex(), meshGeometry.GetVerts());
                mesh.triangles = meshGeometry.GetTriangles();
                planetCollider.sharedMesh = mesh;
                recalc();
                break;
            default:
                break;
        }
        // clean up
        meshGeometry = null;
    }

    private void recalc(bool bounds = true) {
        if (bounds) { mesh.RecalculateBounds(); }
        mesh.RecalculateNormals();
    }

    public float GetMaxElevation() {
        return textureManager.maxElev;
    }

    void Update() {
        // rotate the layers when we view the planet from space.
        if (rotate) {
            if (planetLayer == "cloud") {
                transform.Rotate(Vector3.up, Time.deltaTime * .45F);
            }
            else if (planetLayer != "atmosphere") {
                transform.Rotate(Vector3.up, Time.deltaTime * 1F);
            }
        }
    }
}
