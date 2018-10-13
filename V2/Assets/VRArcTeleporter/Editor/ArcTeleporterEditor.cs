//========= Copyright 2017, Sam Tague, All rights reserved. ===================
//
// Editor for ArcTeleporter
//
//===================Contact Email: Sam@MassGames.co.uk===========================
#if VRInteraction
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using VRInteraction;

namespace VRArcTeleporter
{
	[CustomEditor(typeof(ArcTeleporter))]
	public class ArcTeleporterEditor : Editor
	{
		// target component
		public ArcTeleporter teleporter = null;

		int raycastLayersSize = 0;
		int tagsSize = 0;

		private VRInput input;

		static bool raycastLayerFoldout = false;
		static bool tagsFoldout = false;
		static bool helpFoldout = false;

		public virtual void OnEnable()
		{
			teleporter = (ArcTeleporter)target;
			if (teleporter.raycastLayer != null)
				raycastLayersSize = teleporter.raycastLayer.Count;
			else raycastLayersSize = 0;
			if (teleporter.tags != null)
				tagsSize = teleporter.tags.Count;
			else tagsSize = 0;

			input = teleporter.GetComponent<VRInput>();
			if (input == null)
			{
				input = teleporter.gameObject.AddComponent<VRInput>();
				SetDefaultActions();
				EditorUtility.SetDirty(teleporter);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		private void SetDefaultActions()
		{
			bool useShow = teleporter.controlScheme == ArcTeleporter.ControlScheme.TWO_BUTTON_MODE;
			int showAction = 1;
			int teleportAction = useShow ? 2 : 1;
			if (input.VRActions == null) input.VRActions = useShow ? new string[] { "NONE", "SHOW", "TELEPORT" } : new string[] { "NONE", "TELEPORT" };
			else
			{
				List<string> newActions = new List<string>();
				bool addShow = useShow ? true : false;
				bool addTeleport = true;
				for(int i=0; i<input.VRActions.Length; i++)
				{
					if (input.VRActions[i] == "SHOW")
					{
						showAction = i;
						addShow = false;
					}
					if (input.VRActions[i] == "TELEPORT")
					{
						teleportAction = i;
						addTeleport = false;
					}
					newActions.Add(input.VRActions[i]);
				}
				if (addShow)
				{
					showAction = newActions.Count;
					newActions.Add("SHOW");
				}
				if (addTeleport)
				{
					teleportAction = newActions.Count;
					newActions.Add("TELEPORT");
				}
				input.VRActions = newActions.ToArray();
			}

			input.gripKey = teleportAction;
			input.gripKeyOculus = teleportAction;
			if (useShow)
			{
				input.menuKey = showAction;
				input.menuKeyOculus = showAction;
			}

			EditorUtility.SetDirty(teleporter);
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Reset Actions To Teleporter Default"))
			{
				SetDefaultActions();
			}

			serializedObject.Update();

			SerializedProperty controlScheme = serializedObject.FindProperty("controlScheme");
			controlScheme.intValue = (int)(ArcTeleporter.ControlScheme)EditorGUILayout.EnumPopup("Control Scheme", (ArcTeleporter.ControlScheme)controlScheme.intValue);
			ArcTeleporter.ControlScheme controlSchemeEnum = (ArcTeleporter.ControlScheme)controlScheme.intValue;

			if (controlSchemeEnum == ArcTeleporter.ControlScheme.PRESS_AND_RELEASE)
			{
				int showIndex = -1;
				for(int i=0; i<input.getVRActions.Length; i++)
				{
					if (input.getVRActions[i] == "SHOW")
					{
						showIndex = i;
						break;
					}
				}
				if (showIndex != -1)
				{
					if (input.triggerKey == showIndex || input.padCentre == showIndex || input.padTop == showIndex ||
						input.padLeft == showIndex || input.padRight == showIndex || input.padBottom == showIndex ||
						input.gripKey == showIndex || input.menuKey == showIndex || input.AXKey == showIndex ||
						input.triggerKeyOculus == showIndex || input.padCentreOculus == showIndex || input.padTopOculus == showIndex ||
						input.padLeftOculus == showIndex || input.padRightOculus == showIndex || input.padBottomOculus == showIndex ||
						input.gripKeyOculus == showIndex || input.menuKeyOculus == showIndex || input.AXKeyOculus == showIndex)
					{
						EditorGUILayout.HelpBox("The SHOW action has no effect when in press and release control scheme", MessageType.Warning);
					}
				}
			}

			SerializedProperty transition = serializedObject.FindProperty("transition");
			transition.intValue = (int)(ArcTeleporter.Transition)EditorGUILayout.EnumPopup("Transition", (ArcTeleporter.Transition)transition.intValue);
			ArcTeleporter.Transition transitionEnum = (ArcTeleporter.Transition)transition.intValue;
			EditorGUI.indentLevel++;
			switch(transitionEnum)
			{
			case ArcTeleporter.Transition.FADE:
				SerializedProperty fadeMat = serializedObject.FindProperty("fadeMat");
				fadeMat.objectReferenceValue = EditorGUILayout.ObjectField("Fade Material", fadeMat.objectReferenceValue, typeof(Material), false);
				EditorGUILayout.HelpBox("Material should be using a transparent shader with a colour field. Use the ExampleFade material in the materials folder or make your own", MessageType.Info);
				SerializedProperty fadeDuration = serializedObject.FindProperty("fadeDuration");
				fadeDuration.floatValue = EditorGUILayout.FloatField("Fade Duration", fadeDuration.floatValue);
				break;
			case ArcTeleporter.Transition.DASH:
				SerializedProperty dashSpeed = serializedObject.FindProperty("dashSpeed");
				dashSpeed.floatValue = EditorGUILayout.FloatField("Dash Speed", dashSpeed.floatValue);
				break;
			}
			EditorGUI.indentLevel--;

			SerializedProperty firingMode = serializedObject.FindProperty("firingMode");
			firingMode.intValue = (int)(ArcTeleporter.FiringMode)EditorGUILayout.EnumPopup("Firing Mode", (ArcTeleporter.FiringMode)firingMode.intValue);
			ArcTeleporter.FiringMode firingModeEnum = (ArcTeleporter.FiringMode)firingMode.intValue;

			EditorGUI.indentLevel++;

			switch(firingModeEnum)
			{
			case ArcTeleporter.FiringMode.ARC:
				SerializedProperty arcImplementation = serializedObject.FindProperty("arcImplementation");
				arcImplementation.intValue = (int)(ArcTeleporter.ArcImplementation)EditorGUILayout.EnumPopup("Arc Implementation", (ArcTeleporter.ArcImplementation)arcImplementation.intValue);
				ArcTeleporter.ArcImplementation arcImplementationEnum = (ArcTeleporter.ArcImplementation)arcImplementation.intValue;
				switch(arcImplementationEnum)
				{
				case ArcTeleporter.ArcImplementation.FIXED_ARC:
					EditorGUI.indentLevel++;
					SerializedProperty maxDistance = serializedObject.FindProperty("maxDistance");
					maxDistance.floatValue = EditorGUILayout.FloatField("Max Distance", maxDistance.floatValue);
					EditorGUI.indentLevel--;
					break;
				case ArcTeleporter.ArcImplementation.PHYSICS_ARC:
					EditorGUI.indentLevel++;
					SerializedProperty gravity = serializedObject.FindProperty("gravity");
					gravity.floatValue = EditorGUILayout.FloatField("Gravity", gravity.floatValue);

					SerializedProperty initialVelMagnitude = serializedObject.FindProperty("initialVelMagnitude");
					initialVelMagnitude.floatValue = EditorGUILayout.FloatField("Initial Velocity Magnitude", initialVelMagnitude.floatValue);

					SerializedProperty timeStep = serializedObject.FindProperty("timeStep");
					timeStep.floatValue = EditorGUILayout.FloatField("Time Step", timeStep.floatValue);
					EditorGUI.indentLevel--;
					break;
				}

				SerializedProperty arcLineWidth = serializedObject.FindProperty("arcLineWidth");
				arcLineWidth.floatValue = EditorGUILayout.FloatField("Arc Width", arcLineWidth.floatValue);

				SerializedProperty arcMat = serializedObject.FindProperty("arcMat");
				var oldArcMatInt = arcMat.intValue;
				arcMat.intValue = (int)(ArcTeleporter.ArcMaterial)EditorGUILayout.EnumPopup("Use Material", (ArcTeleporter.ArcMaterial)arcMat.intValue);
				ArcTeleporter.ArcMaterial arcMatEnum = (ArcTeleporter.ArcMaterial)arcMat.intValue;


				if (arcMatEnum == ArcTeleporter.ArcMaterial.MATERIAL)
				{
					SerializedProperty goodTeleMat = serializedObject.FindProperty("goodTeleMat");
					goodTeleMat.objectReferenceValue = EditorGUILayout.ObjectField("Good Material", goodTeleMat.objectReferenceValue, typeof(Material), false);

					SerializedProperty badTeleMat = serializedObject.FindProperty("badTeleMat");
					badTeleMat.objectReferenceValue = EditorGUILayout.ObjectField("Bad Material", badTeleMat.objectReferenceValue, typeof(Material), false);

					SerializedProperty matScale = serializedObject.FindProperty("matScale");
					matScale.floatValue = EditorGUILayout.FloatField("Material scale", matScale.floatValue);

					SerializedProperty texMovementSpeed = serializedObject.FindProperty("texMovementSpeed");
					texMovementSpeed.vector2Value = EditorGUILayout.Vector2Field("Material Movement Speed", texMovementSpeed.vector2Value);
				} else
				{
					SerializedProperty goodTeleShader = serializedObject.FindProperty("goodTeleShader");
					SerializedProperty badTeleShader = serializedObject.FindProperty("badTeleShader");
					if (goodTeleShader.objectReferenceValue == null || badTeleShader.objectReferenceValue == null)
					{
						Object customShader = (Object)Shader.Find("Custom/ArcShader");
						if (customShader == null) customShader = (Object)Shader.Find("Standard");
						goodTeleShader.objectReferenceValue = customShader;
						badTeleShader.objectReferenceValue = customShader;
					}
					SerializedProperty goodSpotCol = serializedObject.FindProperty("goodSpotCol");
					goodSpotCol.colorValue = EditorGUILayout.ColorField("Good Colour", goodSpotCol.colorValue);
					goodTeleShader.objectReferenceValue = EditorGUILayout.ObjectField("Good Shader", goodTeleShader.objectReferenceValue, typeof(Shader), false);

					SerializedProperty badSpotCol = serializedObject.FindProperty("badSpotCol");
					badSpotCol.colorValue = EditorGUILayout.ColorField("Bad Colour", badSpotCol.colorValue);
					badTeleShader.objectReferenceValue = EditorGUILayout.ObjectField("Bad Shader", badTeleShader.objectReferenceValue, typeof(Shader), false);
				}
				break;
			case ArcTeleporter.FiringMode.PROJECTILE:

				SerializedProperty teleportProjectile = serializedObject.FindProperty("teleportProjectilePrefab");
				teleportProjectile.objectReferenceValue = EditorGUILayout.ObjectField("Teleport Projectile Prefab", teleportProjectile.objectReferenceValue, typeof(GameObject), false);
				EditorGUILayout.HelpBox("Projectile prefab should have a rigidbody attached", MessageType.Info);

				SerializedProperty initVelocity = serializedObject.FindProperty("maxDistance");
				initVelocity.floatValue = EditorGUILayout.FloatField("Inital Velocity", initVelocity.floatValue);
				break;
			}

			EditorGUI.indentLevel--;

			SerializedProperty teleportCooldown = serializedObject.FindProperty("teleportCooldown");
			teleportCooldown.floatValue = EditorGUILayout.FloatField("Teleport Cooldown", teleportCooldown.floatValue);

			SerializedProperty disableRoomRotationWithTrackpad = serializedObject.FindProperty("disableRoomRotationWithTrackpad");
			disableRoomRotationWithTrackpad.boolValue = EditorGUILayout.Toggle("Disable Room Rotation", disableRoomRotationWithTrackpad.boolValue);

			SerializedProperty useLastGoodSpot = serializedObject.FindProperty("useLastGoodSpot");
			GUIContent useLastGoodSpotContent = new GUIContent("Use Last Good Spot", "The last good spot found since the arc began will be used");
			EditorGUILayout.PropertyField(useLastGoodSpot);

			SerializedProperty hideWhenHoldingItem = serializedObject.FindProperty("hideWhenHoldingItem");
			GUIContent hideWhenHoldingItemContent = new GUIContent("Hide When Holding Item", "Disables the teleporter functions when holding an interactable item");
			EditorGUILayout.PropertyField(hideWhenHoldingItem, hideWhenHoldingItemContent);

			SerializedProperty useTeleportHighlight = serializedObject.FindProperty("useTeleportHighlight");
			useTeleportHighlight.boolValue = EditorGUILayout.Toggle("Use Teleport Highlight", useTeleportHighlight.boolValue);
			if (useTeleportHighlight.boolValue)
			{
				SerializedProperty teleportHighlight = serializedObject.FindProperty("teleportHighlight");
				if (teleportHighlight.objectReferenceValue == null)
				{
					string[] results = AssetDatabase.FindAssets("TeleportHighlightExample");
					foreach(string result in results)
					{
						if (result == null || result == "") continue;
						GameObject highlightExample = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(result));
						if (highlightExample != null)
						{
							teleportHighlight.objectReferenceValue = (Object)highlightExample;
							break;
						}
					}
				}
				teleportHighlight.objectReferenceValue = EditorGUILayout.ObjectField("Teleport Highlight", teleportHighlight.objectReferenceValue, typeof(GameObject), false);
			}

			SerializedProperty canPickupItems = serializedObject.FindProperty("canPickupItems");
			EditorGUILayout.PropertyField(canPickupItems);
			if (canPickupItems.boolValue)
			{
				SerializedProperty pickupHighlight = serializedObject.FindProperty("pickupHighlight");
				EditorGUILayout.PropertyField(pickupHighlight);
			}

			SerializedProperty useRoomShapeHighlight = serializedObject.FindProperty("useRoomShapeHighlight");
			if (useRoomShapeHighlight.boolValue)
			{
				SerializedProperty roomShape = serializedObject.FindProperty("roomShape");
				if (roomShape.objectReferenceValue == null)
				{
					string[] results = AssetDatabase.FindAssets("RoomHighlightExample");
					foreach(string result in results)
					{
						if (result == null || result == "") continue;
						GameObject highlightExample = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(result));
						if (highlightExample != null)
						{
							roomShape.objectReferenceValue = (Object)highlightExample;
							break;
						}
					}
				}
				roomShape.objectReferenceValue = EditorGUILayout.ObjectField("Room Highlight", roomShape.objectReferenceValue, typeof(GameObject), false);
			}

			SerializedProperty vrCamera = serializedObject.FindProperty("vrCamera");
			GUIContent vrCameraContent = new GUIContent("VR Camera", "If the camera for this teleport is not the main camera you can optionally specify it here.");
			EditorGUILayout.PropertyField(vrCamera, vrCameraContent);

			SerializedProperty vrPlayArea = serializedObject.FindProperty("vrPlayArea");
			GUIContent vrPlayAreaContent = new GUIContent("VR Play Area", "Should be the root object of the vr rig, you can optionally specify it here if it is not being found correctly");
			EditorGUILayout.PropertyField(vrPlayArea, vrPlayAreaContent);

			SerializedProperty onMoveSound = serializedObject.FindProperty("onMoveSound");
			onMoveSound.objectReferenceValue = EditorGUILayout.ObjectField("On Move Sound", onMoveSound.objectReferenceValue, typeof(AudioClip), false);

			SerializedProperty onlyLandOnFlat = serializedObject.FindProperty("onlyLandOnFlat");
			onlyLandOnFlat.boolValue = EditorGUILayout.Toggle("Only land on flat", onlyLandOnFlat.boolValue);
			if (onlyLandOnFlat.boolValue)
			{
				SerializedProperty slopeLimit = serializedObject.FindProperty("slopeLimit");
				slopeLimit.floatValue = EditorGUILayout.FloatField("Slope limit", slopeLimit.floatValue);
			}

			SerializedProperty onlyLandOnTag = serializedObject.FindProperty("onlyLandOnTag");
			onlyLandOnTag.boolValue = EditorGUILayout.Toggle("Only land on tagged", onlyLandOnTag.boolValue);

			if (onlyLandOnTag.boolValue)
			{
				tagsFoldout = EditorGUILayout.Foldout(tagsFoldout, "Tags");
				if (tagsFoldout)
				{
					EditorGUI.indentLevel++;
					tagsSize = EditorGUILayout.IntField("Size", tagsSize);

					SerializedProperty tags = serializedObject.FindProperty("tags");
					if (tagsSize != tags.arraySize) tags.arraySize = tagsSize;

					for (int i=0 ; i<tagsSize ; i++)
					{
						SerializedProperty tagName = tags.GetArrayElementAtIndex(i);
						tagName.stringValue = EditorGUILayout.TextField("Element "+i, tagName.stringValue);
					}
					EditorGUI.indentLevel--;
				}
			}

			raycastLayerFoldout = EditorGUILayout.Foldout(raycastLayerFoldout, "Raycast Layers");
			if (raycastLayerFoldout)
			{
				EditorGUI.indentLevel++;
				raycastLayersSize = EditorGUILayout.IntField("Size", raycastLayersSize);

				SerializedProperty raycastLayers = serializedObject.FindProperty("raycastLayer");
				if (raycastLayersSize != raycastLayers.arraySize) raycastLayers.arraySize = raycastLayersSize;
				for(int i=0 ; i<raycastLayersSize ; i++)
				{
					SerializedProperty raycastLayerName = raycastLayers.GetArrayElementAtIndex(i);
					raycastLayerName.stringValue = EditorGUILayout.TextField("Element "+i, raycastLayerName.stringValue);
				}
				EditorGUILayout.HelpBox("Leave raycast layers empty to collide with everything", MessageType.Info);
				if (raycastLayers.arraySize > 0)
				{
					SerializedProperty ignoreRaycastLayers = serializedObject.FindProperty("ignoreRaycastLayers");
					ignoreRaycastLayers.boolValue = EditorGUILayout.Toggle("Ignore raycast layers", ignoreRaycastLayers.boolValue);
					EditorGUILayout.HelpBox("Ignore raycast layers True: Ignore anything on the layers specified. False: Ignore anything on layers not specified", MessageType.Info);
				}
				EditorGUI.indentLevel--;
			}

			SerializedProperty offsetTrans = serializedObject.FindProperty("offsetTrans");
			if (offsetTrans.objectReferenceValue == null)
			{
				if (GUILayout.Button("Create Offset Transform"))
				{
					GameObject newOffset = new GameObject("Offset");
					newOffset.transform.parent = teleporter.transform;
					newOffset.transform.localPosition = Vector3.zero;
					newOffset.transform.localRotation = Quaternion.identity;
					newOffset.transform.localScale = Vector3.one;
					offsetTrans.objectReferenceValue = newOffset.transform;
				}
			}
			offsetTrans.objectReferenceValue = EditorGUILayout.ObjectField("Offset", offsetTrans.objectReferenceValue, typeof(Transform), true);

			serializedObject.ApplyModifiedProperties();

			helpFoldout = EditorGUILayout.Foldout(helpFoldout, "Help");
			if (helpFoldout)
			{
				EditorGUILayout.HelpBox(
					"The ArcTeleporter script implements the 'Input Received' method called by VRInput, it responds to:\n" +
					"SHOW: Shows and hides the arc when on two button mode.\n" +
					"TELEPORT: Teleports when the SHOW key is held down in two button mode or teleports on press and release mode\n" +
					"\n" +
					"Control Scheme:\n" +
					"Two Button Mode: The two button mode seperates out show the arc/firing the projectile and actually teleporting yourself, " +
					"because of this you can display the arc then decide not to teleport at all by releasing the SHOW key. In order to teleport you must be " +
					"holding down the SHOW and TELEPORT keys.\n" +
					"Press and Release: The press and release mode combines showing and teleporting into one button, in this mode holding down " +
					"a TELEPORT key will show the arc/fire the projectile and releasing will teleport, this has the benefit of freeing up a key " +
					"for your game but the downside of not letting you cancel a teleport once you hold down the button.\n" +
					"\n" +
					"Transition:\n" +
					"Instance: Instant teleportation is just like it sounds the moment the teleport is activated you will be moved to the new " +
					"spot.\n" +
					"Fade: The fade mode adds a slight delay during which a plane will be faded in, the teleport happens and then the plane is faded out. You can set the " +
					"material the plane will use by placing your material in the Fade Material slot (the example uses a standard shader thats just black). " +
					"You can also set the fade duration which will determine the time it takes to fade in and out, the default being 0.5 seconds.\n" +
					"Dash: Dash moves the player area at a set speed (Dash Speed) toward the destination point.\n" +
					"\n" +
					"Firing Mode:\n" +
					"Arc:\n" +
					"\n" +
					"Arc Implementation:\n" +
					"Fixed Arc: The fixed arc is good if there is little verticality in your level, it works well with the line renderer as " +
					"each line segment is equal distance so there are no stretched textures.\n" +
					"Physics Arc: The physics arc will allow you to point straight up and have the arc come right back down how you might " +
					"expect, the issue is each line segment will be different sizes so it is recommended to use the Color instead of a material, this is " +
					"because there are some angles where part of the line will appear stretched.\n" +
					"You can test both out by changing the Arc Implementation variable and see which works best for you.\n" +
					"\n" +
					"Arc Width: The width of the line renderer in unity units\n" +
					"\n" +
					"Use Material:\n" +
					"Material: The material mode is best used with the fixed arc implementation and will allow you to use your own texture and " +
					"shader for the arc, the material used will switch between the good and bad spots depending on whether it's currently pointed " +
					"at a valid teleport spot an invalid spot.\n" +
					"The scale is used for making sure the texture is not stretched or squashed and the movement speed can be used to animated the " +
					"material (should use small numbers and this is affected by scale).\n" +
					"Colour: The colour can be used for a simpler look and will let you set the good and bad colours with alpha, Best used with the\n" +
					"physics implementation.\n" +
					"\n" +
					"Teleport Cooldown:\n" +
					"You can add a cooldown here that will lock the ability to teleport for the given number of seconds, just set this to zero if you don't want " +
					"a cooldown. Just as advice it's important to communicate to the player that a cooldown is in effect so make sure to tie this into you UI in " +
					"some way if you intend to use a long cooldown.\n" +
					"\n" +
					"Disable Room Rotation: This is if you are using the trackpad as your show teleport button and will disable the ability to rotate " +
					"the room when moving and holding down the trackpad.\n" +
					"\n" +
					"Teleport and Room Highlight: The teleport highlight just like the room highlight can take a prefab, however the position and rotation " +
					"will be affected so if you want to replace them you should mirror them on the existing ones to get an idea of how they should be positioned.\n" +
					"\n" +
					"Only Land on Flat:\n" +
					"This will allow you to specify a slope limit in degrees, 0 will stop you from landing on anything, 90 will allow you to teleport onto " +
					"vertical walls and 180 is the same as having it toggled off.\n" +
					"\n" +
					"Only land on tagged:\n" +
					"Ticking this will allow you to expand a list, you can set the size and type in the name of all the tags you want to whitelist. The tag " +
					"must be on the object with the collider to work. This will allow you to specify the floor of a room for example, and prevent movement onto a table.\n" +
					"\n" +
					"Raycast layers:\n" +
					"Expanding raycast layers and adding to the size will allow you to enter the name of layers you either want the arc to go through or layers " +
					"you only want the arc to hit, you can change this by toggling the Ignore Raycast layers boolean, when ticked it will only hit things on  " +
					"the layers specified and when unticked will hit everything else. This is useful if you have invisible triggers in your scene you want the " +
					"arc to just go straight though. Remember that the object with the collider is the one that has to be set to the layer in order to work.\n" +
					"If you are having the issue of held items preventing the arc from showing you can but the items colliders on a separate layer and specifying it here." +
					"\n" +
					"Offset Transform:\n" +
					"You can click the create offset transform button to create and assign a new offset as a child of the controller.\n" +
					"The best way to place this is to start the game and pause when you can see the controller, move the transform " +
					"into the position you want then click the cog in the top right of the transform and click 'Copy Component'. " +
					"Stop the game and click the cog on the offset transform and click 'Paste Component As Value'.", MessageType.Info);
			}
		}
	}
}
#endif