# Monolights 2D deferred lights 
Implementation of a 2D deferred lights engine for Monogame. Inspired on the effect as used in the games _Full Bore_ (Whole Hog Games), _Hive Jump_(Graphite Lab). Also see _Sprite Lamp_ (Snake Hill Games) for a related tool. 

* LightingEngine, the sample application.
* MonoLights, the actual implementation of the Deferred engine.

# The example application
Run the application and use the following keys to toggle basic settings:

* **Mouse**: use the mouse to move the light around (both lighttypes). 
* **Left Mouse button**: spawn a random pointlight at the mousepointer location
* **Right button**: toggle between pointlight and spotlight
* **Left, Right**: rotate the spotlight
* **Up, Down**: Increase, decrease the light power (both lighttypes).
* **A, Z**: Increase, decrease the 'Z' value of the light (both lighttypes).
* **S, X**: Increase, decrease the light decay (both lighttypes).
* **D, C**: Increase, decrease the beam width of the spotlight.
* **F1**: Toggle drawing the debug (intermediate) rendertargets.

