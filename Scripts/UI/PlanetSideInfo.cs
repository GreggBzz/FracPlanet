using System;
using UnityEngine;

public class PlanetSideInfo : MonoBehaviour {

    private GameObject localScan;
    private GameObject wand;
    private GameObject localScanText;
    private GameObject planet;
    private GameObject theSun;
    private DrawScene aScene;
    private string detectedObject;
    private float waterline;

    private bool wandFound;
    private bool planetFound;

    private Vector3 targetPos;
    private Vector3 playerCoordinates; // the location on the planet.
    private Quaternion targetRotation;

    private RadialMenuManager radialMenu;
    private float smoothFactor = 20f;
    public bool infoDisplayed = false;

    // temperture calculation on the fly.
    private float tempPositionModifier = .5f;
    private float tempSunModifier = .5f;
    private int sunZenithAngle = 0;

    private System.Random rnd;

    void Start() {
        if (GameObject.Find("PlanetSideInfo") != null) {
            localScan = GameObject.Find("PlanetSideInfo");
        }

        Font LocalScanFont = Resources.Load("UI/Fonts/BroshK") as Font;
        localScanText = new GameObject("PlantSideInfoOverlay");
        localScanText.AddComponent<TextMesh>();
        localScanText.GetComponent<TextMesh>().text = "";
        localScanText.GetComponent<TextMesh>().font = LocalScanFont;
        localScanText.GetComponent<Renderer>().material = LocalScanFont.material;
        localScanText.GetComponent<TextMesh>().fontSize = 45;
        localScanText.GetComponent<TextMesh>().characterSize = .005f;
        localScanText.GetComponent<TextMesh>().lineSpacing = .8f;
        localScanText.GetComponent<TextMesh>().anchor = TextAnchor.UpperCenter;
        localScanText.GetComponent<Renderer>().enabled = false;
        localScanText.GetComponent<MeshRenderer>().allowOcclusionWhenDynamic = false;
    }

    void Update() {
        if (!wandFound) {
            if (GameObject.Find("Controller (right)") != null) {
                wand = GameObject.Find("Controller (right)");
                radialMenu = wand.GetComponent<RadialMenuManager>();
                aScene = wand.GetComponent<DrawScene>();
                wandFound = true;
            }
            else {
                return;
            }
            if (GameObject.Find("Sun") != null) {
                theSun = GameObject.Find("Sun");
            }
        }

        if (aScene.onWhichPlanet == "") {
            Dismiss();
            return;
        }

        if (radialMenu.whatIsSelected == "Info") {
            if (!planetFound) {
                if (GameObject.Find("aPlanet") != null) {
                    planet = GameObject.Find("aPlanet");
                    waterline = planet.GetComponent<PlanetGeometryDetail>().waterLine;
                    planetFound = true;
                }
                else {
                    planetFound = false;
                }
            }
            localScan.GetComponent<MeshRenderer>().enabled = !infoDisplayed;
            localScanText.GetComponent<Renderer>().enabled = !infoDisplayed;
            infoDisplayed = !infoDisplayed;
            if (radialMenu.whatWasSelected == "Teleport") {
                radialMenu.whatIsSelected = "Teleport";
            } else {
                radialMenu.whatIsSelected = "";
            }
            
        }

        if (!infoDisplayed) { return; }

        targetPos = wand.transform.position + (wand.transform.right * -.30f);
        targetRotation = wand.transform.rotation * Quaternion.Euler(90, 0, 5);

        localScan.transform.position = Vector3.Lerp(localScan.transform.position, targetPos, Time.deltaTime * smoothFactor);
        localScan.transform.rotation = Quaternion.Slerp(localScan.transform.rotation, targetRotation, Time.deltaTime * smoothFactor);

        localScanText.transform.position = localScan.transform.position + (wand.transform.forward * .1f) + (wand.transform.up * .065f);
        localScanText.transform.rotation = localScan.transform.rotation;

        sunZenithAngle = (int)theSun.GetComponent<Sun>().zenithAngle;

        // perform temperature and position calculations.
        if (planet != null) {
            playerCoordinates = (gameObject.transform.position - new Vector3(0, 750f, 3500f));
            playerCoordinates = planet.transform.localRotation * playerCoordinates;
            tempPositionModifier = .75f - (Mathf.Abs(playerCoordinates.y) / (planet.GetComponent<PlanetGeometry>().diameter / 2) * .75f);
        }
                
        if (sunZenithAngle < 90) {
            tempSunModifier += (.0007f * Time.deltaTime);
            if (tempSunModifier > .24f) {
                tempSunModifier = .25f;
            }
        }
        else {
            tempSunModifier -= (.0007f * Time.deltaTime);
            if (tempSunModifier < .05f) {
                tempSunModifier = 0.05f;
            }
        }

        float temperature = (wand.GetComponent<PlanetMetaData>().temp[1] - wand.GetComponent<PlanetMetaData>().temp[0]) *
                            (tempSunModifier + tempPositionModifier) + wand.GetComponent<PlanetMetaData>().temp[0];

        // describe our rocks.
        detectedObject = "";
        if (wand.GetComponent<TelePortParabola>().whatIsHit.Contains("Rock")) {
            string[] rockSplit = wand.GetComponent<TelePortParabola>().whatIsHit.Split('_');
            rnd = new System.Random(Int32.Parse(rockSplit[1]));
            detectedObject = "Rock/Mineral \n(" +
                 wand.GetComponent<PlanetMetaData>().rocks[rnd.Next(0, 5)] + ", " +
                 wand.GetComponent<PlanetMetaData>().rocks[rnd.Next(0, 5)] + ")";
        } else if (wand.GetComponent<TelePortParabola>().whatIsHit.Contains("Tree")) {
            detectedObject = "Organic/Tree Like";
        } else if (wand.GetComponent<TelePortParabola>().whatIsHit.Contains("Planet")) {
            detectedObject = "Terrain/Lithosphere";
        }
        
        string allText = "";
        allText += ("<color=grey>Corrodinates:</color> " + playerCoordinates + "\n");
        allText += ("<color=grey>Elevation Above Hydrosphere:</color> " + (int)(gameObject.transform.position.y - 750 - waterline) + "M\n");
        allText += ("<color=grey>Temperature:</color> " + (int)temperature + "K" + "\n");
        allText += ("<color=grey>Solar Zenith Angle:</color> " + sunZenithAngle + "\n");
        allText += ("<color=grey>Weather:</color> " + "Calm" + "\n\n");
        allText += ("<color=orange>Detected Object:</color>\n");
        allText += detectedObject;
        localScanText.GetComponent<TextMesh>().text = allText;
    }

    public void Dismiss() {
        localScan.GetComponent<MeshRenderer>().enabled = false;
        infoDisplayed = false;
        planetFound = false;
    }
}
