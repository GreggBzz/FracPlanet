using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetProperties : MonoBehaviour {
    public string[] planetTypes = new string[5];


    // Use this for initialization
    void Start () {
        planetTypes[0] = "MoltenPlanet";
        planetTypes[1] = "GasGiantPlanet";
        planetTypes[2] = "TerraPlanet";
        planetTypes[3] = "IcyPlanet";
        planetTypes[4] = "RockyPlanet";
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void SetupPlanet() {
    }

    public void GetPlanetStats() {
    }
}
