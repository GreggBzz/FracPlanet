using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : MonoBehaviour {
    private GameObject[] planet = new GameObject[200];
    private GameObject[] ocean = new GameObject[200];

    public GameObject planetOutline; // public for user(wand) manipulated transforms.

    public float distScale = 200F;
    public float planetCircumference = 50F;
    public string curPlanetType = "";
    private PlanetMesh[] planets = new PlanetMesh[200];
    private PlanetMesh[] oceans = new PlanetMesh[200];
    private int planetMeshCount = 0;
    private int oceanMeshCount = 0;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void DestroyPlanets() {
        for (int i = 0; i <= planetMeshCount; i++) {
            Destroy(planet[i]);
        }
        for (int i = 0; i <= oceanMeshCount; i++) {
            Destroy(ocean[i]);
        }
    }

    public void DestroyPlanetOutline() {
        Destroy(planetOutline);
    }

    public void AddPlanet(Transform controllerTransform, float distScale, float planetScale, bool ocean) {
        // Setup textures.
        Texture curTypeTexture = Resources.Load("PlanetTextures/txr" + curPlanetType) as Texture;
        Material planetSurfaceMaterial = new Material(Shader.Find("Standard"));
        planetSurfaceMaterial.SetTexture("_MainTex", curTypeTexture);
    
        // Construction.
        planet[planetMeshCount] = new GameObject("aPlanet[" + planetMeshCount + "]");
        planet[planetMeshCount].AddComponent<MeshFilter>();
        planet[planetMeshCount].AddComponent<MeshRenderer>();
        planet[planetMeshCount].AddComponent<PlanetMesh>();
        planet[planetMeshCount].GetComponent<PlanetMesh>().circumference = planetCircumference;
        planet[planetMeshCount].GetComponent<Renderer>().material = planetSurfaceMaterial;
        planets[planetMeshCount] = planet[planetMeshCount].GetComponent<PlanetMesh>();
        planets[planetMeshCount].ocean = false;
        planets[planetMeshCount].Generate();
        // Adjust position.
        planets[planetMeshCount].transform.position = controllerTransform.position + controllerTransform.forward * distScale;
        planets[planetMeshCount].transform.eulerAngles = controllerTransform.eulerAngles;
        if (ocean) { AddPlanetOcean(controllerTransform, distScale, planetScale); }
        planetMeshCount += 1;
    }

    private void AddPlanetOcean(Transform controllerTransform, float distScale, float planetScale) {
        // Setup textures.
        Texture curTypeTexture = Resources.Load("PlanetTextures/txrOcean" + curPlanetType) as Texture;
        Material oceanSurfaceMaterial = new Material(Shader.Find("Standard"));
        oceanSurfaceMaterial.SetTexture("_MainTex", curTypeTexture);
        // Construction.
        ocean[oceanMeshCount] = new GameObject("aPlanetOcean[" + oceanMeshCount + "]");
        ocean[oceanMeshCount].AddComponent<MeshFilter>();
        ocean[oceanMeshCount].AddComponent<MeshRenderer>();
        ocean[oceanMeshCount].AddComponent<PlanetMesh>();
        ocean[oceanMeshCount].GetComponent<PlanetMesh>().circumference = planetCircumference;
        ocean[oceanMeshCount].GetComponent<Renderer>().material = oceanSurfaceMaterial;
        oceans[oceanMeshCount] = ocean[oceanMeshCount].GetComponent<PlanetMesh>();
        planets[planetMeshCount].ocean = true;
        oceans[oceanMeshCount].Generate();
        // Adjust position.
        oceans[oceanMeshCount].transform.position = controllerTransform.position + controllerTransform.forward * distScale;
        oceans[oceanMeshCount].transform.eulerAngles = controllerTransform.eulerAngles;
        oceanMeshCount += 1;
    }
    
    public void AddPlanetOutline() {
        Texture planetOutlineTexture = Resources.Load("PlanetTextures/txrPlanetOutline") as Texture;
        Material planetOutlineMaterial = new Material(Shader.Find("Transparent/Diffuse"));
        planetOutlineMaterial.SetTexture("_MainTex", planetOutlineTexture);
        planetOutline = new GameObject("Planet Outline");
        planetOutline = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planetOutline.GetComponent<Renderer>().material = planetOutlineMaterial;
        planetOutline.transform.localScale = new Vector3(planetCircumference, planetCircumference, planetCircumference);
        planetOutline.SetActive(true);
    }
    public void UpdatePlanetOutline(Transform controllerTransform) {
        if (planetOutline != null) {
            planetOutline.transform.position = controllerTransform.position + controllerTransform.forward * distScale;
            planetOutline.transform.localScale = new Vector3(planetCircumference, planetCircumference, planetCircumference);
        }
    }
}
