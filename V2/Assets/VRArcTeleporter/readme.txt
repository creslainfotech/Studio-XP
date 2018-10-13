VR Arc Teleporter comes with everything you need to quickly get a locomotion system in your VR game.
To setup for SteamVR:
Drag the '[CameraRig]' prefab from the prefabs folder in SteamVR into the scene, on each controller object click 'Add Component' and search for 
'Arc Teleporter'.
To Setup for Oculus Native:
Create a new empty gameobject in the scene (optionally make sure it is on the floor), drag the 'OVRCameraRig' prefab as a child of the empty gameobject 
(optionally make sure 'Tracking Origin Type' is set to 'Floor Level' on the OVRManager component), drag the 'LocalAvatar' prefab as a child of the empty gameobject, 
on either both controllers or both hand objects click 'Add Component' and search for 'Arc Teleporter'.

Now allows for fading transitions, custom control schemes, a rotatable room and fire a projectile and teleport
to it as an alternative to the arc.

Make sure you do the same things with both controllers individually, unless you want them to act differently.

Oculus Native:
Switching between Oculus and SteamVR means using the alternative camera rig and switching the Virtual Reality SDK
found in Edit->Project Settings->Player.

VRInput:
When adding the Arc Teleporter script a VRInput will be added automatically. You can customize what you want the different
buttons to do. The arc teleporter will react to SHOW and TELEPORT although you can add your own script that reacts to different
names. VRInput will use the SendMessage method to call 'Input Received' with the action name as a parameter.

All information on the Arc Teleport component can be found by expanding the 'Help' foldout at the bottom of the component.

Thanks for downloading.

Whats new!
2.1:
-Added ability to pickup items with teleporter arc
-Added "Hide When Holding" toggle, disabling teleporter methods while holding an item
-Added "Use Last Good Spot" toggle, will use the last good spot since show the arc instead of doing nothing if currently pointing at a bad spot
2.0:
-All new VRInteraction base asset!
-Smooth locomotion.
-Oculus Native room rotation improved.
-Shader bugs fixed.
1.2.3:
-Allowed for disabling the script at runtime without errors
1.2.2:
-Updated to 2017.2
-Update Oculus SDK
-Fixed build issue that didn't include the custom arc shader
1.2.1:
-Updated SteamVR
-Updated Oculus SDK
1.2:
-Added full Oculus native support
1.1.2
-Add Oculus support through SteamVR
-Seperated controls to VRInput script
1.1.1
-Updated SteamVR to 1.2
-Updated obsolete methods
1.1
-Dash mode (with optional blur toggle)
-New optional arc implementation (Fixed Arc and Physics Arc)
-Added offset transform
-Redone controller keys (seperated trackpad)
-Added optional teleporting cooldown timer
-Exapanded upon documentation
-Controller listeners are removed when the controller is disabled

1.0
-Added press and release to teleport as an alterative control scheme
-Added fade transition
-Projectile firing mode
-Can now rotate room with the trackpad
-Fixed bug where line would flicker red

0.5
-Fixed mistake with ExamplePlayer prefab in example scene

0.4
-Moved Editor script to an editor folder so the project can build.
-Added slope limit to land on flat for some leniency.
-Added tags list for limiting what can be teleported to based on tag.
-Added boolean to disable the premade controls.
-ArcTeleporter is now completely scaleable like the rest of the steamVR camera rig.

0.3
-Added layers. Make a list of layers you either only want to hit or that you want to ignore by toggle the Ignore Raycast Layers boolean

0.2
-Fixed issue where part of the line renderer would not disable when nothing was hit

0.1
-Initial release