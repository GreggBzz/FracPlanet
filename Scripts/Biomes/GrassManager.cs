using UnityEngine;

public class GrassManager : MonoBehaviour {
    private bool grassMade = false;
    private bool grassEnabled = false;
    // For ~100FPS on a 1060, shoot for < 2500 grass in the FOV using 10 materials.
    // Because of dynamic batching, performance roughly scales with (grassMaterials * grassCount).
    public const int grassCount = 4000;
    public const int grassMaterials = 10;
    public const float grassArea = 30;

    public bool procedualGrassLocations = false;

    private Grass[] grassMesh = new Grass[grassCount];
    private GameObject[] grassBunch = new GameObject[grassCount];
    private Material[] grassMaterial = new Material[grassMaterials];
    private Vector3[] grassPos = new Vector3[grassCount];

    void Start () {
	}
	void Update () {
	}

    public void AddGrass(bool onPlanet = false) {
        if (grassMade) return;

        int materialIndex = 0;

        for (int i = 0; i <= grassMaterials - 1; i++) {
            grassMaterial[i] = GetMaterial();
        }      
        for (int i = 0; i <= grassCount - 1; i++) {
            grassMesh[i] = new Grass();
            grassMesh[i].Generate();
        }
        for (int i = 0; i <= grassCount - 1; i++) {
            grassBunch[i] = new GameObject("aGrass" + i);
            grassBunch[i].AddComponent<MeshFilter>();
            grassBunch[i].AddComponent<MeshRenderer>();

            grassBunch[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            grassBunch[i].GetComponent<Renderer>().receiveShadows = false;
            
            grassBunch[i].GetComponent<MeshFilter>().mesh.vertices = grassMesh[i].GetVerts();
            grassBunch[i].GetComponent<MeshFilter>().mesh.triangles = grassMesh[i].GetTris();
            grassBunch[i].GetComponent<MeshFilter>().mesh.uv = grassMesh[i].GetUv();

            grassBunch[i].GetComponent<Renderer>().material = grassMaterial[materialIndex];
            materialIndex += 1; if (materialIndex >= grassMaterials) { materialIndex = 0; }       

            float xPos = Random.Range(-grassArea, grassArea);
            float zPos = Random.Range(-grassArea, grassArea);
            grassPos[i] = new Vector3(xPos,0F,zPos);

            grassMesh[i].SetNormals(grassBunch[i].GetComponent<MeshFilter>().mesh.normals);
            grassBunch[i].GetComponent<MeshFilter>().mesh.normals = grassMesh[i].GetNormals();
            grassBunch[i].GetComponent<MeshFilter>().mesh.RecalculateBounds();
            grassBunch[i].GetComponent<Renderer>().enabled = false;
        }
        grassMade = true;
        grassEnabled = false;
    }

    private Material GetMaterial() {
        Material aGrassMaterial;
        Texture grassTexture = Resources.Load("BiomeTextures/Grass" + Random.Range(1, 5)) as Texture;
        //Texture grassTexture = Resources.Load("BiomeTextures/grassTest3") as Texture;
        aGrassMaterial = new Material(Shader.Find("Custom/SimpleGrassSine"));
        aGrassMaterial.SetTexture("_MainTex", grassTexture);
        aGrassMaterial.SetTexture("_Illum", grassTexture);
        aGrassMaterial.SetColor("_Color", GetColor());
        aGrassMaterial.renderQueue = 1000;
        aGrassMaterial.SetFloat("_XStrength", Random.Range(.01F, .1F));
        aGrassMaterial.SetFloat("_XDisp", Random.Range(-.7F, .7F));
        aGrassMaterial.SetFloat("_WindFrequency", Random.Range(.09F, .25F));
        return aGrassMaterial;
    }
    private Color32 GetColor() {
        int R = Random.Range(180, 255); int G = Random.Range(180, 255);
        int B = Random.Range(180, 255); int A = Random.Range(180, 255);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void DestroyGrass() {
        for (int i = 0; i <= grassCount - 1; i++) {
            Destroy(grassBunch[i]);
        }
        grassMade = false;
    }

    public void DisableGrass() {
        if (!grassMade) { return; }
        if (!grassEnabled) { return; }
        for (int i = 0; i <= grassCount - 1; i++) {
            grassBunch[i].GetComponent<Renderer>().enabled = false;
        }
        grassEnabled = false;
    }

    private void EnableGrass() {
        if (!grassMade) { return; }
        for (int i = 0; i <= grassCount - 1; i++) {
            grassBunch[i].GetComponent<Renderer>().enabled = true;
        }
        grassEnabled = true;
    }

    public void PlaceGrass(bool onPlanet = false, Vector3[] grassMap = null) {

        if (!grassMade) { return; }
        if (!grassEnabled) { EnableGrass(); }

        RaycastHit hit;
        
        if (onPlanet) {
            for (int i = 0; i <= grassCount - 1; i++) {
                Vector3 dropPoint = new Vector3(grassPos[i].x, 25000, (3500 + grassPos[i].z));
                if (Physics.Raycast(dropPoint, Vector3.down, out hit, 30000)) {
                    grassBunch[i].transform.position = hit.point;
                }
            }
        }
        else {
            for (int i = 0; i <= grassCount - 1; i++) {
                grassBunch[i].transform.position = new Vector3(grassPos[i].x, 0, grassPos[i].z);
            }
        }
    }
}