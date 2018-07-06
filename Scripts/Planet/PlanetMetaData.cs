using System;
using UnityEngine;

public class PlanetMetaData : MonoBehaviour {
    // base data from procedual mesh generation.
    public int seed;
    public string planetType;
    public float diameter;
    public bool hasAtmosphere;
    public bool hasClouds;
    public bool hasOcean;

    // metadata returned from parameters.
    public string atmosphere = "Unknown";
    public string lithosphere = "Unknown";
    public string hydrosphere = "Unknown";
    public string gravity = "Unknown";
    public string weather = "Unknown";
    public string climate = "Unknown";
    public string mass = "Unknown";
    public string density = "Unknown";
    public string type = "Unknown";

    // what temperature, types of rocks for planetside metadata.
    public int[] temp = new int[2];
    public string[] rocks = new string[6];

    public string corrdinates = "Unknown"; // really, the random seed.
    private bool partialData = false;
    private bool[] unknowns = new bool[7];
    private string linebreak;

    // Earth's diameter in meters and mass in kilograms as a refrence. 
    private const double earthScale = 12742000;
    private const double earthMass = 5.972; // x10 ^ 24th.

    private System.Random rnd;

    public void initialize(int aSeed, float aDiameter, string aPlanetType, bool atmosphere, bool clouds, bool ocean) {
        seed = aSeed;
        rnd = new System.Random(seed);
        diameter = aDiameter;
        planetType = aPlanetType;
        hasAtmosphere = atmosphere;
        hasOcean = ocean;
        hasClouds = clouds;
        unknowns[0] = (rnd.NextDouble() <= .2); unknowns[1] = (rnd.NextDouble() <= .2);
        unknowns[2] = (rnd.NextDouble() <= .2); unknowns[3] = (rnd.NextDouble() <= .4);
        unknowns[4] = (rnd.NextDouble() <= .4); unknowns[5] = (rnd.NextDouble() <= .8);
        unknowns[6] = (rnd.NextDouble() <= .8);
    }

    private string setHydrosphere() {
        rnd = new System.Random(seed);
        if ((partialData) && unknowns[0]) { return "Unknown\n"; }
        linebreak = ",\n                       ";



        string[] elements = { "Carbon Dioxide", "Liquid Nitrogen",
                                 "Liquid Oxygen", "Liquid Hydrogen",
                                 "Liquid Helium", "Liquid Argon", "Methane",
                                 "Ethane", "Sodium", "Potassium", "Ammonia",
                                 "Complex Ammonia Compounds", "Water",
                                 "Complex Chlorine Compounds", "Acetylene",
                                 "Diacetylene", "Complex Hydrocarbons", "Aerosols",
                                 "Mercury", "Sulfuric Acid", "Hydrochloric Acid" };

        if (!hasOcean) { return "None";  }

        if ((planetType).Contains("Icy")) {
            hydrosphere = (elements[rnd.Next(0, 9)] + linebreak +
                           elements[rnd.Next(0, 9)] + linebreak +
                           elements[rnd.Next(0, 9)]);
        }
        if ((planetType).Contains("Molten")) {
            hydrosphere = (elements[rnd.Next(11, 21)] + linebreak +
                           elements[rnd.Next(11, 21)] + linebreak +
                           elements[rnd.Next(11, 21)]);
        }
        if ((planetType).Contains("Terra")) {
            hydrosphere = (elements[rnd.Next(11, 17)] + linebreak +
                           elements[rnd.Next(11, 17)] + linebreak +
                           elements[rnd.Next(11, 17)]);
        }
        return hydrosphere;
    }

    private string setAtmosphere() {
        rnd = new System.Random(seed);
        if ((partialData) && unknowns[1]) { return "Unknown\n"; }
        linebreak = ",\n                     ";
        string[] elements = { "Carbon Dioxide", "Nitrogen",
                                "Nitrogen Compounds", "Nitrogen Isotopes",
                                "Oxygen", "Ozone and Oxygen Isotopes",
                                "Carbon Monoxide", "Argon", "Hydrogen",
                                "Helium", "Methane", "Ethane", "Acetylene",
                                "Diacetylene", "Complex Hydrocarbons",
                                "Aerosols", "Complex Ammonia Compounds",
                                "Complex Chlorine Compounds", "Chlorine Gas",
                                "Sodium", "Potassium", "Sulfuric Acid",
                                "Neon Gas", "Fluorine Gas",
                                "Unknown Complex Molecules", "Water Vapor", };

        if (!hasAtmosphere) { return "None"; }
        if ((planetType).Contains("Icy")) {
            atmosphere = (elements[rnd.Next(12, 18)] + linebreak +
                          elements[rnd.Next(12, 18)] + linebreak +
                          elements[rnd.Next(12, 18)]);
        }
        if ((planetType).Contains("Molten")) {
            atmosphere = (elements[rnd.Next(14, 24)] + linebreak +
                          elements[rnd.Next(14, 24)] + linebreak +
                          elements[rnd.Next(14, 24)]);
        }
        if ((planetType).Contains("Terra")) {
            atmosphere = (elements[rnd.Next(0, 14)] + linebreak +
                          elements[rnd.Next(0, 14)] + linebreak +
                          elements[rnd.Next(0, 14)]);
        }
        if ((planetType).Contains("Rocky")) {
            atmosphere = "None";
        }
        return atmosphere;
    }

    private string setLithosphere() {
        rnd = new System.Random(seed);
        if ((partialData) && unknowns[2]) { return "Unknown\n"; }
        linebreak = ",\n                       ";
        string[] elements = { "Mercury", "Pyroxenite", "Zinc", "Aluminum", "Antimony",
                                 "Chromium", "Cobalt", "Feldspar", "Copper",
                                 "Gold", "Lead", "Iron", "Magnesium",
                                 "Molybdenum", "Nickel", "Platinum",
                                 "Plutonium", "Promethium", "Silicates", "Silver",
                                 "Tin", "Titanium", "Tungsten"};

        if ((planetType).Contains("Icy")) {
            for (int i = 0; i <= rocks.Length - 1; i++) {
                rocks[i] = elements[rnd.Next(0, 23)];
            }
            lithosphere = (rocks[0] + linebreak +
                           rocks[1] + linebreak +
                           rocks[2]);
        }
        if ((planetType).Contains("Molten")) {
            for (int i = 0; i <= rocks.Length - 1; i++) {
                rocks[i] = elements[rnd.Next(5, 23)];
            }
            lithosphere = (rocks[0] + linebreak +
                           rocks[1] + linebreak +
                           rocks[2]);
        }
        if ((planetType).Contains("Terra")) {
            for (int i = 0; i <= rocks.Length - 1; i++) {
                rocks[i] = elements[rnd.Next(2, 23)];
            }
            lithosphere = (rocks[0] + linebreak +
                           rocks[1] + linebreak +
                           rocks[2]);
        }
        if ((planetType).Contains("Rocky")) {
            for (int i = 0; i <= rocks.Length - 1; i++) {
                rocks[i] = elements[rnd.Next(0, 23)];
            }
            lithosphere = (rocks[0] + linebreak +
                           rocks[1] + linebreak +
                           rocks[2]);
        }
        return lithosphere;
    }
    
    private string setClimate() {
        rnd = new System.Random(seed);
        if ((partialData) && unknowns[3]) { return "Unknown\n"; }
        string[] climates = { "Sub-Artic", "Artic", "Temperate", "Tropical",
                             "Searing", "Inferno" };
        int highK = 0; int lowK = 0; int lowI = 0; int highI = 0;
        if ((planetType).Contains("Icy")) {
            lowK = rnd.Next(50, 118); highK = rnd.Next(118, 191);
            lowI = 0; highI = 1;
            if (lowK > 100) { lowI = 1; }
        }
        else if ((planetType).Contains("Terra") || (planetType).Contains("Rock"))  {
            lowK = rnd.Next(90, 216); highK = rnd.Next(216, 331);
            lowI = 0; highI = 2;
            if (lowK > 100) { lowI = 1; }
            if (lowK > 200) { lowI = 2; }
            if (highK > 280) { highI = 3; }
            if (highK > 320) { highI = 4; }
        }
        else if ((planetType).Contains("Molten")) {
            lowK = rnd.Next(350, 626); highK = rnd.Next(626, 901);
            lowI = 4; highI = 5;
            if (lowK > 400) { lowI = 5; }
        }
        climate = (lowK + "K" + " to " + highK + "K" + " (" + climates[lowI] + " to " + climates[highI] + ")");
        temp[0] = lowK; temp[1] = highK;
        return climate;
    }

    private string setWeather() {
        rnd = new System.Random(seed);
        if ((partialData) && unknowns[4]) { return "Unknown\n"; }
        string[] weathers = { "None", "Calm", "Normal", "Severe", "Violent",
                             "Very-Violent" };
        if (!hasAtmosphere) {
            weather = "None";
        }
        else {
            int lowW = rnd.Next(1, 4);
            int highW = rnd.Next(3, 6);
            weather = (weathers[lowW] + " to " + weathers[highW]);
        }
        return weather;
    }

    private string setGravity() {
        rnd = new System.Random(seed);
        if ((partialData) && unknowns[5]) { return "Unknown\n"; }
        // Fudge bigly on the gravity forumla. Sorry Newton. 
        double tempGrav = (intMass() / earthMass) * (diameter / 2500);
        tempGrav = Math.Round(tempGrav, 2);
        gravity = System.Convert.ToString(tempGrav + "G's");
        return gravity;
    }
    
    private string setMass() {
        rnd = new System.Random(seed);
        if ((partialData) && unknowns[6]) { return "Unknown\n"; }
        mass = (Math.Round(intMass(), 2) +  "x10^24 kg");
        return mass;
    }

    private double intMass() {
        rnd = new System.Random(seed);
        if (planetType.Contains("Gas")) {
            return (diameter / 2500) * earthMass * (rnd.NextDouble() * (.8 - .3) + .3);
        }
        else {
            return (diameter / 2500) * earthMass * (rnd.NextDouble() * (1.3 - .7) + .7);
        }
    }

    public string getData(string planetType, int planetSeed, bool partial = false) {
        partialData = partial;
        string allText = "";
        allText += ("<color=orange>Type:</color> " + planetType + "\n");
        allText += ("<color=orange>Index:</color> " + planetSeed + "\n\n");
        allText += ("<color=grey>Mass:</color> " + setMass() + "\n");
        allText += ("<color=grey>Gravity:</color> " + setGravity() + "\n");
        allText += ("<color=grey>Climate:</color> " + setClimate() + "\n");
        allText += ("<color=grey>Weather:</color> " + setWeather() + "\n");
        allText += ("<color=grey>Atmosphere:</color> " + setAtmosphere() + "\n");
        allText += ("<color=grey>Hydrosphere:</color> " + setHydrosphere() + "\n");
        allText += ("<color=grey>Lithosphere:</color> " + setLithosphere() + "\n");
        return allText;
    }
}
