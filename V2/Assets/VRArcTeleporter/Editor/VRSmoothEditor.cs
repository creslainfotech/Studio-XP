//========= Copyright 2017, Sam Tague, All rights reserved. ===================
//
// Editor for VRSmooth
//
//===================Contact Email: Sam@MassGames.co.uk===========================
#if VRInteraction
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRInteraction;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace VRArcTeleporter
{
	[CustomEditor(typeof(VRSmooth))]
	public class VRSmoothEditor : Editor 
	{
		private VRSmooth smooth;

		public virtual void OnEnable()
		{
			smooth = (VRSmooth)target;
			if (smooth.GetComponent<VRInteractor>() == null)
			{
				smooth.gameObject.AddComponent<VRInteractor>();
				EditorUtility.SetDirty(smooth);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SerializedProperty moveSetup = serializedObject.FindProperty("moveSetup");
			EditorGUILayout.PropertyField(moveSetup);
			VRSmooth.MoveSetup moveSetupEnum = (VRSmooth.MoveSetup)moveSetup.intValue;

			switch(moveSetupEnum)
			{
			case VRSmooth.MoveSetup.MOVE:
				SerializedProperty runSpeed = serializedObject.FindProperty("runSpeed");
				EditorGUILayout.PropertyField(runSpeed);
				break;
			case VRSmooth.MoveSetup.TURN:
				SerializedProperty turnAngle = serializedObject.FindProperty("turnAngle");
				EditorGUILayout.PropertyField(turnAngle);

				SerializedProperty turnSpeed = serializedObject.FindProperty("turnSpeed");
				EditorGUILayout.PropertyField(turnSpeed);

				SerializedProperty turnSound = serializedObject.FindProperty("turnSound");
				EditorGUILayout.PropertyField(turnSound);
				break;
			}
			SerializedProperty vrRigRoot = serializedObject.FindProperty("vrRigRoot");
			EditorGUILayout.PropertyField(vrRigRoot);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif