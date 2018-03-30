using UnityEngine;
using System;

public class WandController : SteamVR_TrackedController {

    public SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)controllerIndex); } }
    public Vector3 velocity { get { return controller.velocity; } }
    public Vector3 angularVelocity { get { return controller.angularVelocity; } }
    public Vector3 menuPos;
    public RadialMenuManager radialMenu;
    private DrawScene aScene;
    private TeleportArc teleportArc;

    private DateTime before;
    private DateTime after;

  
    protected override void Start() {
        base.Start();
        aScene = gameObject.AddComponent<DrawScene>();
        radialMenu = gameObject.AddComponent<RadialMenuManager>();
        teleportArc = gameObject.AddComponent<TeleportArc>();
    }

    protected override void Update() {
        base.Update();
        // high level, we're either teleporting, making a planet or nothing.
        // if we're rendering a planet, limit the user options.

        string switch_string = radialMenu.whatIsSelected;
        if (radialMenu.whatIsSelected.Contains("Planet")) switch_string = "Planet";
        switch (switch_string)
        {
            case "Teleport":
                teleportArc.AddTeleportLine(Color.green, Color.red);
                break;
            case "Planet":
                aScene.SetPlanetType(radialMenu.whatIsSelected);
                radialMenu.SwitchToChild();
                aScene.AddPlanetOutline();
                break;
            case "Delete":
                aScene.DestroyPlanets();
                aScene.DestroyPlanetOutline();
                radialMenu.whatIsSelected = "";
                break;
            case "Home":
                aScene.Teleport(true);
                radialMenu.whatIsSelected = "";
                aScene.onWhichPlanet = "";
                aScene.PausePlanet();
                break;
            default:
                if (radialMenu.curMenuType != null) {
                    if (!radialMenu.curMenuType.Contains(" - Child")) {
                        teleportArc.DestroyTeleportLine();
                    }
                }
                break;
        }
        // draw/update the pointer line?
        if (teleportArc.lineRenderer && teleportArc.lineRenderer.enabled) {
            teleportArc.UpdateTeleportLine();
        }

        // draw/update the planets?
        aScene.UpdatePlanets();
        // update the terrain LOD rotation and ensure player top dead center position if detail terrain exists.
        aScene.MatchTerrainRotation();
        // Have we just teleported? Delay the transform until we fade out. Fade in once it's done.
        aScene.TeleportFade();
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
                // There's no CameraRig, we can't teleport, so return.
                if (transform.parent == null)
                    return;
                aScene.Teleport();
                break;
            case "Planet":
                aScene.AddPlanet();
                //System.GC.Collect();
                aScene.DestroyPlanetOutline();
                radialMenu.Cycle("Destroy Menu");
                break;
            default:
                teleportArc.DestroyTeleportLine();
                break;
        }
        // do we need to pause any planets?
        aScene.PausePlanet();
    }

    public override void OnMenuClicked(ClickedEventArgs e) {
        base.OnMenuClicked(e);
        aScene.DestroyPlanetOutline();
        teleportArc.DestroyTeleportLine();
        // if we're rendering a planet, don't let the user do anything besides destroy/teleport.
        if (aScene.havePlanet) {
            radialMenu.Cycle("Destroy Menu");
        }
        else {
            radialMenu.Cycle();
        }
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
