using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSounds : MonoBehaviour {
    public GameObject planetWind;
    public GameObject planetOcean;
    private GameObject headset;
    private Vector3 planetCenter = new Vector3(0, 750, 3500);

    private void Awake() {
        planetOcean = new GameObject("Ocean Sounds");
        planetOcean.AddComponent<AudioSource>();
        planetOcean.GetComponent<AudioSource>().enabled = false;
        planetWind = new GameObject("Wind Sounds");
        planetWind.AddComponent<AudioSource>();
        planetWind.GetComponent<AudioSource>().enabled = false;
        enabled = false;
        headset = GameObject.Find("[CameraRig]");
    }

    public void EnableSounds(string curPlanetType, bool hasOcean = false, bool hasAtmosphere = false, float diameter = 2500F, float maxElev = 2500F) {
        if (curPlanetType.Contains("Rock")) return; // no sound on Rocky planets.
        // If enabled, set the volume.
        float curElev = Vector3.Distance(headset.transform.position, planetCenter);
        if (enabled) { setVolume(maxElev, diameter / 2.0F, curElev); return; }
        // If we're not enabled, setup all sounds depending on ocean and atmosphere.
        if (hasOcean) {
            planetOcean.GetComponent<AudioSource>().clip = Resources.Load("PlanetSounds/" + curPlanetType.Replace("Planet", "") + "Ocean") as AudioClip;
            planetOcean.GetComponent<AudioSource>().loop = true;
            planetOcean.GetComponent<AudioSource>().enabled = true;
            planetOcean.GetComponent<AudioSource>().Play();
        }
        // If there's an atmosphere, sound.
        if (hasAtmosphere) {
            planetWind.GetComponent<AudioSource>().clip = Resources.Load("PlanetSounds/" + curPlanetType.Replace("Planet", "") + "Wind") as AudioClip;
            planetWind.GetComponent<AudioSource>().loop = true;
            planetWind.GetComponent<AudioSource>().enabled = true;
            planetWind.GetComponent<AudioSource>().Play();
            setVolume(maxElev, diameter / 2.0F, curElev);
            enabled = true;
        }
    }

    public void DisableSounds() {
        enabled = false;
        planetOcean.GetComponent<AudioSource>().enabled = false;
        planetWind.GetComponent<AudioSource>().enabled = false;
    }

    private void setVolume(float maxElev, float radius, float curElev) {
        float soundDistance = (curElev - radius) / (maxElev - radius);
        if (soundDistance <= 0F) soundDistance = 0F;
        if (soundDistance >= 1F) soundDistance = 1F;
        if (soundDistance > .1F) {
            planetWind.GetComponent<AudioSource>().volume = soundDistance;
            planetOcean.GetComponent<AudioSource>().volume = .125F - soundDistance / 8F;
        }
        else {
            planetWind.GetComponent<AudioSource>().volume = soundDistance;
            planetOcean.GetComponent<AudioSource>().volume = 1 - soundDistance;
        }
    }
}
