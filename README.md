# FracPlanet
Fractal planet script for Unity 5.

This will create a highly tesselated procedual planet mesh in Unity, with it's center at a specified Vector3 and it's angle at a specified Vector3. The pScale variable determins how big. The default size is 50 radius; pScale divides that.

Initialize it like this:

```
        private GameObject planet = new GameObject;
        private Geosphere myPlanet = new Geosphere;

        Texture txrEarthPlanet = Resources.Load("txrEarthPlanet") as Texture;
        Material planetSurface = new Material(Shader.Find("Standard"));
        planetSurface.mainTexture = txrEarthPlanet;
                
        planet = new GameObject("My Planet");
        planet.AddComponent<MeshFilter>();
        planet.AddComponent<MeshRenderer>();
        planet.AddComponent<Geosphere>();
        planet.GetComponent<Geosphere>().pScale = 2;
        planet.GetComponent<Renderer>().material = planetSurface;
        myPlanet = planet.GetComponent<Geosphere>();
        myPlanet.Generate(beginPos, beginAngle);
 
 ```

You'll need some texture in your Resources folder. I've included txrEarthPlanet.
The default texturing method is currently lame. But, there's hope, there's a lot of triangles to work with.
