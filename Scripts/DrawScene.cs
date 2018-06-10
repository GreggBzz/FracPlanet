using UnityEngine;
// Draw or destroy world objects, excluding controller menus.
public class DrawScene : MonoBehaviour {
    // the WandController object for the right controller.
    private WandController wand;
    private MainLight aMainLight;
    public string onWhichPlanet = "";
    // teleporting stuff.
    private bool teleporting, teleportHome;
    private RaycastHit hit, hit2;
    private bool rotationMatched;
    private TelePortParabola teleportArc;
    // screen fader.
    private ScreenFader screenFade;
    private DrawScene aScene;
    private SkyBoxManager skybox;
    private CameraEffects cameraEffect;
    private float timer = 0;
    private float timerMax = 0;
    // planet related stuff.
    private PlanetManager planetManager;
    private ScanBox scanBox;
    // 1000 queued up seeds for our planets.
    private int[] seedQueue = new int[1500];
    private int seedQueueIndex = 750;
    public int planetSeed;
    public bool havePlanet = false; // are we rendering a planet?
    System.Random rnd;

    void Start() {
        planetManager = gameObject.AddComponent<PlanetManager>();
        if (GameObject.Find("Controller (right)") != null) {
            wand = GameObject.Find("Controller (right)").GetComponent<WandController>();
            teleportArc = GameObject.Find("Controller (right)").GetComponent<TelePortParabola>();
        }
        if (GameObject.Find("ScreenFader") != null) {
            screenFade = GameObject.Find("ScreenFader").GetComponent<ScreenFader>();
        }
        if (GameObject.Find("Main Light") != null) {
            aMainLight = GameObject.Find("Main Light").GetComponent<MainLight>();
        }
        scanBox = gameObject.AddComponent<ScanBox>();
        skybox = gameObject.AddComponent<SkyBoxManager>();
        cameraEffect = gameObject.AddComponent<CameraEffects>();
        screenFade.fadeTime = .1F;
        screenFade.enabled = true;
        rnd = new System.Random(42); // what's the meaning of life, eh?
        // queue up 1000 random planet seeds.
        for (int i = 0; i <= seedQueue.Length - 1; i++) {
            seedQueue[i] = rnd.Next(0, 32000);
        }
        rotationMatched = false;
    }

    private void Update() {
        if (wand == null) { return; }
        if (onWhichPlanet.Contains("Planet")) {
            scanBox.HideScanBox();
            aMainLight.Disable();
            skybox.setSkyOnPlanet(planetManager.curPlanetType, planetManager.curPlanetSeed, planetManager.planetDiameter);
            UpdatePlanetObjects();
            return;
        }
        if ((havePlanet) && (!onWhichPlanet.Contains("Planet"))) {
            UpdateScanBox();
            aMainLight.Enable();
        }
        if ((!havePlanet) && (wand.radialMenu.curMenuType != ("Planet Menu - Child"))) {
            HideScanBox();
            aMainLight.Enable();
        }
        if ((!havePlanet) && (wand.radialMenu.curMenuType == ("Planet Menu - Child"))) {
            planetManager.UpdatePotentialPlanet(seedQueue[seedQueueIndex]);
            UpdateScanBox(true);
            aMainLight.Enable();
        }
        skybox.setSkyOffPlanet();
        teleportArc.teleDistance = 4500F;
    }

    public void MatchTerrainRotation() { // force the rotation of the planet detail terrain to match, drop planet objects (once per teleport).
        if (rotationMatched) { return; }

        int layerMask = LayerMask.GetMask("Default", "Rocks"); // hit only rocks and terrain.

        if (GameObject.Find("aPlanetTopTerrainCollide")) {
            GameObject.Find("aPlanetTopTerrainCollide").transform.rotation = GameObject.Find("aPlanet").transform.rotation;
            GameObject.Find("aPlanetTopTerrainCollide").transform.localRotation = GameObject.Find("aPlanet").transform.localRotation;
            // drop the player from the center top onto the terrain, to ensure we're always above and the planet transform
            // and rotation's happen below us.
            Vector3 dropPoint = new Vector3(0, 30000, 3500);
            if (Physics.Raycast(dropPoint, Vector3.down, out hit2, 30000, layerMask)) {
                wand.transform.parent.position = hit2.point;
            }
            planetManager.placePlanetObjects();
        }
        if (GameObject.Find("aPlanetBottomTerrain")) {
            GameObject.Find("aPlanetBottomTerrain").transform.rotation = GameObject.Find("aPlanet").transform.rotation;
            GameObject.Find("aPlanetBottomTerrain").transform.localRotation = GameObject.Find("aPlanet").transform.localRotation;
        }
        rotationMatched = true;
    }

    private void UpdatePlanetObjects() {
        // update all the planet objects. 
        GameObject.Find("aPlanet").GetComponent<RocksManager>().PlaceAndEnableRocks();
        GameObject.Find("aPlanet").GetComponent<GrassManager>().PlaceAndEnableGrass();
        GameObject.Find("aPlanet").GetComponent<FogManager>().PlaceAndEnableFog();
    }


    public void TeleportFade() {
        // control the teleport fader, called every update from wandcontroller.
        if (!teleporting) return;
        screenFade.fadeIn = false;
        if (!Waited(.3F)) return;
        DoTeleport(teleportHome);
        screenFade.fadeIn = true;
        teleporting = false;
        teleportHome = false;
        return;
    }

    public void Teleport(bool toHome = false) {
        // kick off a teleport sequence, set teleporting to true.
        teleporting = true;
        if (toHome) { teleportHome = true; return; }
    }

    private void DoTeleport(bool toHome) {
        bool drawArc = false;
        if (onWhichPlanet.Contains("Planet")) {
            drawArc = true;
        }
        string[] planetParts = { "aPlanetCloud", "aPlanet" };
        rotationMatched = false;
        if (toHome) {
            wand.transform.parent.eulerAngles = new Vector3(0F, 0F, 0F);
            wand.transform.parent.position = new Vector3(0F, 0F, 0F);
            // Create the full, low detail ocean and terrain, delete the detailed bits.
            if (GameObject.Find("aPlanet")) { planetManager.ManageOcean(false); planetManager.ManageTerrain(false); }
            // reset terrain and clouds to upright.
            foreach (string planetPart in planetParts) {
                if (GameObject.Find(planetPart)) {
                    GameObject.Find(planetPart).transform.eulerAngles = new Vector3(0, 0, 0);
                }
            }
            if (GameObject.Find("aPlanet") != null) {
                GameObject.Find("aPlanet").GetComponent<MeshRenderer>().enabled = true;
                if (GameObject.Find("aPlanet").GetComponent<GrassManager>() != null) {
                    GameObject.Find("aPlanet").GetComponent<GrassManager>().DisableGrass();
                }
                if (GameObject.Find("aPlanet").GetComponent<RocksManager>() != null) {
                    GameObject.Find("aPlanet").GetComponent<RocksManager>().DisableRocks();
                }
                if (GameObject.Find("aPlanet").GetComponent<FogManager>() != null) {
                    GameObject.Find("aPlanet").GetComponent<FogManager>().DisableFog();
                }
            }
            onWhichPlanet = "";
            return;
        }
        hit = teleportArc.UpdateTeleportLine(drawArc);
        //GameObject.Find("TerraPlanetFog1").GetComponent<Renderer>().enabled = false;
        if (hit.point == new Vector3()) { return; }
        if (hit.transform.gameObject.name.Contains("Planet")) {
            // create seperate high detail top ocean and low detail bottom ocean (once).
            planetManager.ManageOcean(true);
            // if we hit a planet, first rotate the point on the planet we hit to the top.
            // set the hit point (fromPoint) relative to 0,0,0 by subtracting the planet's position.
            Vector3 toPoint = Vector3.up;
            Vector3 fromPoint = hit.point - hit.transform.gameObject.transform.position;
            // correct the height of the toPoint by dropping a ray and taking it's hit point.
            Vector3 dropPoint = new Vector3(0, 30000, 3500);
            if (Physics.Raycast(dropPoint, Vector3.down, out hit2, 20000)) {
                toPoint = hit2.point - hit.transform.gameObject.transform.position;
            }
            // forumulate a quaternion rotation using FromToRotation.
            Quaternion rotateToTop = Quaternion.FromToRotation(fromPoint, toPoint);
            // roate each part of the planet using the caclulated rotation
            foreach (string planetPart in planetParts) {
                if (GameObject.Find(planetPart)) {
                    GameObject.Find(planetPart).transform.localRotation = rotateToTop * GameObject.Find(planetPart).transform.localRotation;
                }
            }
            // see MatchTerrainRotation() to see the player position transform, after the terrain has rotated.
            // we drop the player with that. It's called once a frame.
            // tesselate the near terrain.
            planetManager.ManageTerrain(true);
            if (GameObject.Find("aPlanet")) {
                GameObject.Find("aPlanet").GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else {
            wand.transform.parent.eulerAngles = new Vector3(0F, 0F, 0F);
            wand.transform.parent.position = hit.point;
            if (GameObject.Find("aPlanet")) {
                planetManager.ManageOcean(false);
                planetManager.ManageTerrain(false);
            }
        }
        onWhichPlanet = hit.transform.gameObject.name;
        PausePlanet();
    }


    private bool Waited(float seconds) {
        timerMax = seconds;
        timer += Time.deltaTime;
        if (timer >= timerMax) {
            timer = 0; timerMax = 0;
            return true;
        }
        return false;
    }

    public void AddPlanet() {
        planetManager.AddPlanet();
        havePlanet = true;
    }

    public void PausePlanet() {
        // "pause" this planet if we've teleported to it.
        // "unpause" all the other planets.
        planetManager.PausePlanet(onWhichPlanet);
    }

    public void DestroyPlanets() {
        planetManager.DestroyPlanet();
        scanBox.HideScanBox();
        skybox.setSkyOffPlanet();
        onWhichPlanet = "";
        havePlanet = false;
    }

    public void SetPlanetType(string planetType) {
        planetManager.curPlanetType = planetType;
    }

    public void AddPlanetOutline() {
        planetManager.AddPlanetOutline();
    }

    public void DestroyPlanetOutline() {
        planetManager.DestroyPlanetOutline();
    }

    public void UpdateScanBox(bool unknowns = false) {
        string scanBoxText;
        if (unknowns) {
            scanBoxText = planetManager.planetMetaData.getData(true);
        }
        else {
            scanBoxText = planetManager.planetMetaData.getData();
        }
        scanBox.DisplayScanBox(scanBoxText + " Planet Seed: " + seedQueueIndex);
    }

    public void HideScanBox() {
        scanBox.HideScanBox();
    }

    public void UpdatePlanets() {
        if (wand == null) { return; }
        if (havePlanet) { return; }
        string switch_string = wand.radialMenu.whatIsSelected;
        switch (switch_string) {
            case "Next":
                seedQueueIndex += 1;
                if (seedQueueIndex > seedQueue.Length - 1) { seedQueueIndex = 0; }
                planetSeed = seedQueue[seedQueueIndex];
                rnd = new System.Random(planetSeed);
                planetManager.planetDiameter = rnd.Next(1500, 3000);
                wand.radialMenu.whatIsSelected = "";
                break;
            case "Previous":
                seedQueueIndex -= 1;
                if (seedQueueIndex < 0) { seedQueueIndex = seedQueue.Length - 1; }
                planetSeed = seedQueue[seedQueueIndex];
                rnd = new System.Random(planetSeed);
                planetManager.planetDiameter = rnd.Next(1500, 3000);
                wand.radialMenu.whatIsSelected = "";
                break;
            default:
                break;
        }
    }
}