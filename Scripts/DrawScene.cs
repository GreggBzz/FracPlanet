using UnityEngine;

public class DrawScene : MonoBehaviour {
    // Draw or destroy world objects, excluding controller menus.

    // the WandController object for the right controller.
    private WandController wand;

    // A line drawn from the controller.
    public LineRenderer pointerLineRenderer;
    private Vector3[] lineRendererVertices = new Vector3[2];
    public GameObject pointerLine;
    private Color baseColor;
    private Color hitColor;
    public string onWhichPlanet = "";


    // use the planetmanager to draw and update planet related stuff.
    private PlanetManager planetManager;

    void Start () {
        planetManager = gameObject.AddComponent<PlanetManager>();
        wand = GameObject.Find("Controller (right)").GetComponent<WandController>();
	}
	
	void Update () {
    }

    public void DestroyPointerLine() {
        if (pointerLineRenderer != null) {
            pointerLineRenderer = null;
            Destroy(pointerLine);
         }
    }

    public void AddPointerLine(Color newBaseColor, Color newHitColor) {
        if (pointerLineRenderer != null) {
            return;
        }
        baseColor = newBaseColor;
        hitColor = newHitColor;
        pointerLine = new GameObject("Pointer Line");
        pointerLineRenderer = pointerLine.AddComponent<LineRenderer>();
        pointerLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        pointerLineRenderer.startWidth = 0.01f;
        pointerLineRenderer.endWidth = 0.01f;
        pointerLineRenderer.numPositions = 2;
    }
    public void UpdatePointerLine() {
        // Update our LineRenderer
        if (pointerLineRenderer && pointerLineRenderer.enabled) {
            RaycastHit hit;
            Vector3 startPos = wand.transform.position;
            // If our raycast hits, end the line at that positon. Otherwise,
            // just make our line point straight out for 200 meters.
            // If the raycast hits, the line will be green, otherwise it'll be red.
            if (Physics.Raycast(startPos, wand.transform.forward, out hit, 1000.0f)) {
                lineRendererVertices[1] = hit.point;
                pointerLineRenderer.startColor = baseColor;
                pointerLineRenderer.endColor = baseColor;
            }
            else {
                lineRendererVertices[1] = startPos + wand.transform.forward * 1000.0f;
                pointerLineRenderer.startColor = hitColor;
                pointerLineRenderer.endColor = hitColor;
            }

            lineRendererVertices[0] = wand.transform.position;
            pointerLineRenderer.SetPositions(lineRendererVertices);
        }
    }

    public void AddPlanet() {
        planetManager.AddPlanet(wand.transform, planetManager.distScale, planetManager.
                                planetCircumference);
    }

    public void PausePlanet() {
        // "pause" this planet if we've teleported to it.
        // "unpause" all the other planets.
        planetManager.PausePlanet(onWhichPlanet);
    }

    public void DestroyPlanets() {
        planetManager.DestroyPlanets();
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

    public void UpdatePlanets() {
        string switch_string = wand.radialMenu.whatIsSelected;
        switch (switch_string) 
        {
            case "Out":
                if (planetManager.distScale > 7000F) planetManager.distScale = 7000F;
                    else planetManager.distScale += 1F;
                break;
            case "In":
                if (planetManager.distScale < 100F) planetManager.distScale = 100F;
                    else planetManager.distScale -= 1F;
                break;
            case "Grow":
                if (planetManager.planetCircumference > 4000F) planetManager.planetCircumference = 4000F;
                    else planetManager.planetCircumference += 1F;
                break;
            case "Shrink":
                if (planetManager.planetCircumference < 100F) planetManager.planetCircumference = 100F;
                    else planetManager.planetCircumference -= 1F;
                break;
            default:
                break;
        }
        if (wand.radialMenu.curMenuType == ("Planet Menu - Child")) {
            planetManager.UpdatePlanetOutline(wand.transform);
        }
    }
}
