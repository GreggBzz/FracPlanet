using UnityEngine;

public class ObjectManager : MonoBehaviour {
    private bool objectMade = false;
    private bool objectEnabled = false;
    // For ~100FPS on a 1060, shoot for < 2500 object in the FOV using 10 materials.
    // Because of dynamic batching, performance roughly scales with (objectMaterials * objectCount).
    public int objectCurCount;
    public const int objectMaxCount = 4000;
    public const int objectMaterials = 10;
    public const float objectArea = 30;

    public bool globalobjectLocations = false;
    public bool[] planetRefVets = new bool[40962];

    // Specific objecst.
    private Grass[] grassMesh = new Grass[objectMaxCount];



    private GameObject[] objectBunch = new GameObject[objectMaxCount];
    private Material[] objectMaterial = new Material[objectMaterials];
    public Vector3[] objectPos = new Vector3[objectMaxCount];

    void Start() {
    }
    void Update() {
    }

    public void Addobject(bool onPlanet = false) {
        if (objectMade) return;

        int materialIndex = 0;

        for (int i = 0; i <= objectMaterials - 1; i++) {
            objectMaterial[i] = GetMaterial();
        }
        for (int i = 0; i <= objectMaxCount - 1; i++) {
            grassMesh[i] = new Grass();
            grassMesh[i].Generate();
        }
        for (int i = 0; i <= objectMaxCount - 1; i++) {
            objectBunch[i] = new GameObject("aobject" + i);
            objectBunch[i].AddComponent<MeshFilter>();
            objectBunch[i].AddComponent<MeshRenderer>();

            objectBunch[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            objectBunch[i].GetComponent<Renderer>().receiveShadows = false;

            objectBunch[i].GetComponent<MeshFilter>().mesh.vertices = grassMesh[i].GetVerts();
            objectBunch[i].GetComponent<MeshFilter>().mesh.triangles = grassMesh[i].GetTris();
            objectBunch[i].GetComponent<MeshFilter>().mesh.uv = grassMesh[i].GetUv();

            objectBunch[i].GetComponent<Renderer>().material = objectMaterial[materialIndex];
            materialIndex += 1; if (materialIndex >= objectMaterials) { materialIndex = 0; }

            if (!onPlanet) {
                float xPos = Random.Range(-objectArea, objectArea);
                float zPos = Random.Range(-objectArea, objectArea);
                objectPos[i] = new Vector3(xPos, 0F, zPos);
            }

            grassMesh[i].SetNormals(objectBunch[i].GetComponent<MeshFilter>().mesh.normals);
            objectBunch[i].GetComponent<MeshFilter>().mesh.normals = grassMesh[i].GetNormals();
            objectBunch[i].GetComponent<MeshFilter>().mesh.RecalculateBounds();
            objectBunch[i].GetComponent<Renderer>().enabled = true;
        }
        objectMade = true;
        objectEnabled = false;
    }

    private Material GetMaterial() {
        Material aobjectMaterial;
        Texture objectTexture = Resources.Load("BiomeTextures/object" + Random.Range(1, 5)) as Texture;
        //Texture objectTexture = Resources.Load("BiomeTextures/objectTest3") as Texture;
        aobjectMaterial = new Material(Shader.Find("Custom/SimpleobjectSine"));
        aobjectMaterial.SetTexture("_MainTex", objectTexture);
        aobjectMaterial.SetTexture("_Illum", objectTexture);
        aobjectMaterial.SetColor("_Color", GetColor());
        aobjectMaterial.renderQueue = 1000;
        aobjectMaterial.SetFloat("_XStrength", Random.Range(.01F, .1F));
        aobjectMaterial.SetFloat("_XDisp", Random.Range(-.7F, .7F));
        aobjectMaterial.SetFloat("_WindFrequency", Random.Range(.09F, .25F));
        return aobjectMaterial;
    }
    private Color32 GetColor() {
        int R = Random.Range(180, 255); int G = Random.Range(180, 255);
        int B = Random.Range(180, 255); int A = Random.Range(180, 255);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void Destroyobject() {
        for (int i = 0; i <= objectMaxCount - 1; i++) {
            Destroy(objectBunch[i]);
        }
        objectMade = false;
    }

    public void Disableobject() {
        if (!objectMade) { return; }
        if (!objectEnabled) { return; }
        for (int i = 0; i <= objectMaxCount - 1; i++) {
            objectBunch[i].GetComponent<Renderer>().enabled = false;
        }
        objectEnabled = false;
    }

    public void Enableobject() {
        if (!objectMade) { return; }
        for (int i = 0; i <= objectMaxCount - 1; i++) {
            objectBunch[i].GetComponent<Renderer>().enabled = true;
        }
        objectEnabled = true;
    }

    public void Placeobject(bool onPlanet = false, Vector3[] objectMap = null) {

        if (!objectMade) { return; }
        if (!objectEnabled) { Enableobject(); }

        RaycastHit hit;

        if (onPlanet) {
            for (int i = 0; i <= objectMaxCount - 1; i++) {
                Vector3 dropPoint = new Vector3(objectPos[i].x, 25000, (objectPos[i].z));
                if (Physics.Raycast(dropPoint, Vector3.down, out hit, 30000)) {
                    objectBunch[i].transform.position = hit.point;
                    objectBunch[i].GetComponent<Renderer>().enabled = true;
                }
            }
        }
        else {
            for (int i = 0; i <= objectMaxCount - 1; i++) {
                objectBunch[i].transform.position = new Vector3(objectPos[i].x, 0, objectPos[i].z);
            }
        }
        for (int i = objectCurCount; i <= objectMaxCount - 1; i++) {
            objectBunch[i].GetComponent<Renderer>().enabled = false;
        }
    }
}
