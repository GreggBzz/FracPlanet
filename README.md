## Vive Planet ##
This is the legacy procedural planet generation and UI code that lead to the creation of the project called Exoplanet: https://store.steampowered.com/app/894380/Exoplanet/

Once Exoplanet's final act and scene are polished and tested in the comming month of June 2023 (Unbelievably, I'm *almost done!!*) I'll refresh this code.

Much of it is similar, but I want to offer the more robust, Unity 2021 LTS compatible and feature rich planet generator as seen in Exoplanet as open source.

That said, if you want to play around with this version, here's a few notes. Things will _probably_ work...

## Controls ##
* You only need one controller.
* The Menu button (not the Steam button) cycles menus.
* Select a planet type from the toggle menu. 
* Cycle through planet outlines until you find one that you want to generate.
* Generate a planet by hitting the trigger.
* Select teleportation and point and click to teleport to and around the planet.

### Current Features ###
* Touchpad Radial Toggle Menu (WandController, RadialMenuItem, RadialMenuManager)
* Planet Mesh and Texture Generation (PlanetManager, PlanetMesh, PlanetTexture)
* Teleporation to and around planets.
* Some procedual clouds and material methods. 
* A single pass 6 texture splat map shader.

Vive Planet uses the icosahedron tesselation and displacement technique.

Enjoy!
