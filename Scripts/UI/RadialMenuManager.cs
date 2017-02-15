using UnityEngine;
using System.Collections;

// Controller class for touchpad radial menu management.
// Set stuff in defineTyes() and Start(). Construct with Generate().
// Setup texutres, the number of items (max 10) and menu item text.

public class RadialMenuManager : MonoBehaviour {
    private RadialMenuItem[] radialMenuItem = new RadialMenuItem[10];
    private GameObject[] menuItem = new GameObject[10];
    private string[] menuTitle = new string[10];
    private Texture[] menuTexture = new Texture[10];
    private Material[] menuMaterial = new Material[10];
    private int hoverItem;
    private int itemsCount;
    public string curMenuType;
    public bool drawItems;
    // 10 possible menutypes, each a root or a child.
    private string[] menuTypes = new string[10];
    private int totalTypes;
    // publish what the user has selected.
    public string whatIsSelected = "";
    // keep track of what the user has pressed down on.
    private string whatIsPushed = "";

    void Start() {
        // Configure your menutypes here, with " - Child" menus last. Define items in defineTypes().
        menuTypes[0] = "Planet Menu";
        menuTypes[1] = "Destroy Menu";
        menuTypes[2] = "Planet Menu - Child";
        totalTypes = 3;
        curMenuType = "";
    }

    public void Cycle() {
        if (curMenuType == "") {
            Generate(new Vector3(0, 0, 0), menuTypes[0]);
            return;
        }
        if (curMenuType.Contains(" - Child")) {
            // subtract " - Child" and return to the parent menu.
            string tmpMenuType = curMenuType.Substring(0, curMenuType.Length - 8);
            DestroyItems();
            Generate(new Vector3(0, 0, 0), tmpMenuType);
            return;
        }
        // cycle to the next menu, skipping child menus.
        for (int i = 0; i <= totalTypes - 1; i++) {
            if (curMenuType == menuTypes[i]) {
                if (menuTypes[i + 1].Contains(" - Child")) {
                    curMenuType = "";
                    DestroyItems();
                    return;
                }
                else {
                    DestroyItems();
                    Generate(new Vector3(0, 0, 0), menuTypes[i + 1]);
                    return;
                }
            }
        }
        DestroyItems();
        return;
    }

    public void SwitchToChild() {
        // switch to the child menu of the current menu, if it exists.
        for (int i = 0; i <= totalTypes -1; i++) {
            if (menuTypes[i] == (curMenuType + " - Child")) {
                DestroyItems();
                Generate(new Vector3(0, 0, 0), menuTypes[i]);
                return;
            }
        }
        Debug.Log("No child menu for: " + curMenuType);
    }

    public void Generate(Vector3 sPos, string menuType) {
        defineItems(menuType);
        for (int i = 0; i <= itemsCount - 1; i++) {
            menuMaterial[i] = new Material(Shader.Find("Transparent/Diffuse"));
            menuMaterial[i].mainTexture = menuTexture[i];
            menuItem[i] = new GameObject(menuTitle[i]);
            menuItem[i].AddComponent<MeshFilter>();
            menuItem[i].AddComponent<MeshRenderer>();
            menuItem[i].AddComponent<RadialMenuItem>();
            menuItem[i].GetComponent<Renderer>().material = menuMaterial[i];
            radialMenuItem[i] = menuItem[i].GetComponent<RadialMenuItem>();
            radialMenuItem[i].Generate(sPos, i + 1, itemsCount);
        }
        curMenuType = menuType;
        whatIsPushed = ""; whatIsSelected = "";
    }

    public void DestroyItems() {
        for (int i = 0; i <= itemsCount; i++) {
            Destroy(menuItem[i]);
            radialMenuItem[i] = null;
        }
        curMenuType = "";
    }

    public void SelectItem(bool clicked) {
        // if the pad is clicked, drop the item like a toggle.
        if (clicked) whatIsPushed = menuTitle[hoverItem];
        // if the pad is unclicked, raise the pushed item indicating it's selected.
        else {
            // we're toggling off.
            if (whatIsSelected == whatIsPushed) {
                whatIsSelected = ""; whatIsPushed = "";
                return;
            }
            whatIsSelected = whatIsPushed; whatIsPushed = "";
        }
    }

    public void UpdateMenu(Vector2 touchPadAxis, Transform wandTransform, bool padTouched) {
        // there's nothing to update/display. return.
        if (curMenuType == "") return;
        float curAngle = (Mathf.Atan2(touchPadAxis.x, touchPadAxis.y) + Mathf.PI);
        float closeAngle = 0F;
        for (int i = 0; i <= itemsCount - 1; i++) {
            if (Mathf.Abs(curAngle - radialMenuItem[i].angle) < Mathf.Abs(curAngle - closeAngle)) {
                closeAngle = radialMenuItem[i].angle;
                hoverItem = i;
            }
        }
        // set hoverItem out of bounds in case the pad was not touched.
        if (curAngle == Mathf.PI) hoverItem = 99;
        for (int i = 0; i <= itemsCount - 1; i++) {
            float raiseItemBy = .035F;
            // raise item if it's hovered and not pushed or selected.
            if ((i == hoverItem) && (menuTitle[i] != whatIsSelected) && (menuTitle[i] != whatIsPushed)) {
                raiseItemBy = .05F;
            }
            else if (menuTitle[i] == whatIsPushed) {
                raiseItemBy = .025F;
            }
            else if (menuTitle[i] == whatIsSelected) {
                raiseItemBy = .07F;
            }
            menuItem[i].transform.position = wandTransform.position + wandTransform.up * raiseItemBy;
            menuItem[i].transform.eulerAngles = wandTransform.eulerAngles;
        }
        // return hoverItem to 0 so it plays nice in other functions.
        if (!padTouched) hoverItem = 0;
    }

    // define your menutypes here.
    private void defineItems(string menuType) {
        switch (menuType) {
            case "Planet Menu":
                itemsCount = 6;
                menuTexture[0] = Resources.Load("MenuItems/MoltenPlanet") as Texture;
                menuTexture[1] = Resources.Load("MenuItems/GasGiantPlanet") as Texture;
                menuTexture[2] = Resources.Load("MenuItems/TerraPlanet") as Texture;
                menuTexture[3] = Resources.Load("MenuItems/IcyPlanet") as Texture;
                menuTexture[4] = Resources.Load("MenuItems/RockyPlanet") as Texture;
                menuTexture[5] = Resources.Load("MenuItems/Teleport") as Texture;
                menuTitle[0] = "MoltenPlanet";
                menuTitle[1] = "GasGiantPlanet";
                menuTitle[2] = "TerraPlanet";
                menuTitle[3] = "IcyPlanet";
                menuTitle[4] = "RockyPlanet";
                menuTitle[5] = "Teleport";
                break;
            case "Destroy Menu":
                itemsCount = 2;
                menuTexture[0] = Resources.Load("MenuItems/Delete") as Texture;
                menuTexture[1] = Resources.Load("MenuItems/Home") as Texture;
                menuTitle[0] = "Delete";
                menuTitle[1] = "Home";
                break;
            case "Planet Menu - Child":
                itemsCount = 4;
                menuTexture[0] = Resources.Load("MenuItems/Out") as Texture;
                menuTexture[1] = Resources.Load("MenuItems/Shrink") as Texture;
                menuTexture[2] = Resources.Load("MenuItems/In") as Texture;
                menuTexture[3] = Resources.Load("MenuItems/Grow") as Texture;
                menuTitle[0] = "Out";
                menuTitle[1] = "Shrink";
                menuTitle[2] = "In";
                menuTitle[3] = "Grow";
                break;
            default:
                itemsCount = 0;
                Debug.Log("Please select an existing menuType.");
                curMenuType = "";
                break;
        }
    }
}
