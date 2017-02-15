using UnityEngine;
using System.Collections;

public class WandController : SteamVR_TrackedController {

    public SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)controllerIndex); } }
    public Vector3 velocity { get { return controller.velocity; } }
    public Vector3 angularVelocity { get { return controller.angularVelocity; } }
    public Vector3 menuPos;
    public RadialMenuManager radialMenu;
    private DrawScene aScene;
  
    protected override void Start() {
        base.Start();
        aScene = gameObject.AddComponent<DrawScene>();
        radialMenu = gameObject.AddComponent<RadialMenuManager>();
    }

    protected override void Update() {
        base.Update();
        // high level, we're either teleporting, making a planet or nothing.
        string switch_string = radialMenu.whatIsSelected;
        if (radialMenu.whatIsSelected.Contains("Planet")) switch_string = "Planet";
        switch (switch_string)
        {
            case "Teleport":
                aScene.AddPointerLine(Color.green, Color.red);
                break;
            case "Planet":
                aScene.SetPlanetType(radialMenu.whatIsSelected);
                radialMenu.SwitchToChild();
                aScene.AddPointerLine(Color.red, Color.magenta);
                aScene.AddPlanetOutline();
                break;
            case "Delete":
                aScene.DestroyPlanets();
                aScene.DestroyPlanetOutline();
                radialMenu.whatIsSelected = "";
                break;
            case "Home":
                transform.parent.eulerAngles = new Vector3(0F, 0F, 0F);
                transform.parent.position = new Vector3(0F, 0F, 0F);
                radialMenu.whatIsSelected = "";
                break;
            default:
                if (!radialMenu.curMenuType.Contains(" - Child")) {
                    aScene.DestroyPointerLine();
                }
                break;
        }
        // draw/update the pointer line?
        if (aScene.pointerLineRenderer && aScene.pointerLineRenderer.enabled) {
            aScene.UpdatePointerLine();
        }
        // draw/update the planets?
        aScene.UpdatePlanets();
        // draw/update the radial menu.
        radialMenu.UpdateMenu(GetTouchpadAxis(), transform, controller.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad));
    }

    public float GetTriggerAxis() {
        if (controller == null)
            return 0;
        return controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis1).x;
    }
    public Vector2 GetTouchpadAxis() {
        if (controller == null)
            return new Vector2();
        return controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
    }
    public override void OnTriggerClicked(ClickedEventArgs e) {
        base.OnTriggerClicked(e);
    }
    public override void OnTriggerUnclicked(ClickedEventArgs e) {
        base.OnTriggerUnclicked(e);
        string switch_string = radialMenu.whatIsSelected;
        if (radialMenu.curMenuType.Contains("Planet Menu - Child")) switch_string = "Planet";
        switch (switch_string) {
            case "":
                break;
            case "Teleport":
                // Do teleport stuff.
                // There's no [CameraRig], we can't teleport, so return.
                if (transform.parent == null)
                    return;
                // Perform a raycast starting from the controller's position and going 1000 meters
                // out in the forward direction of the controller to see if we hit something to teleport to.
                RaycastHit hit;
                Vector3 startPos = transform.position;
                if (Physics.Raycast(startPos, transform.forward, out hit, 10000.0f)) {
                    transform.parent.position = hit.point;
                    // We're going to a planet, transform the angle so we're going around it.
                    if (hit.transform.gameObject.name.Contains("Planet")) {
                        Vector3 centerHitObject = hit.transform.gameObject.transform.position;
                        Vector3 outerHitObject = hit.point;
                        transform.parent.eulerAngles = Quaternion.FromToRotation(Vector3.up, outerHitObject - centerHitObject).eulerAngles;
                    }
                    else {
                        transform.parent.eulerAngles = new Vector3(0F, 0F, 0F);
                    }
                }
                if (Physics.Raycast(startPos, transform.forward, out hit, 10000.0f)) {
                    transform.parent.position = hit.point;
                }
                break;
            case "Planet":
                aScene.AddPlanet();
                System.GC.Collect();
                break;
            default:
                aScene.DestroyPointerLine();
                break;
        }
    }
    public override void OnMenuClicked(ClickedEventArgs e) {
        base.OnMenuClicked(e);
        aScene.DestroyPlanetOutline();
        aScene.DestroyPointerLine();
        radialMenu.Cycle();
    }
    public override void OnMenuUnclicked(ClickedEventArgs e) {
        base.OnMenuUnclicked(e);
    }
    public override void OnSteamClicked(ClickedEventArgs e) {
        base.OnSteamClicked(e);
    }
    public override void OnPadClicked(ClickedEventArgs e) {
        base.OnPadClicked(e);
        if (radialMenu.curMenuType != "") {
            radialMenu.SelectItem(true);
        }

    }
    public override void OnPadUnclicked(ClickedEventArgs e) {
        base.OnPadUnclicked(e);
        if (radialMenu.curMenuType != "") {
            radialMenu.SelectItem(false);
        }  
    }
    public override void OnPadTouched(ClickedEventArgs e) {
        base.OnPadTouched(e);
    }
    public override void OnPadUntouched(ClickedEventArgs e) {
        base.OnPadUntouched(e);
    }
    public override void OnGripped(ClickedEventArgs e) {
        base.OnGripped(e);
    }
    public override void OnUngripped(ClickedEventArgs e) {
        base.OnUngripped(e);
    }
}
