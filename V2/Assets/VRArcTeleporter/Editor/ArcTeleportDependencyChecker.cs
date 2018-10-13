using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

public class ArcTeleportDependencyChecker : EditorWindow 
{
	#if !VRInteraction
	[DidReloadScripts]
	private static void CheckVRInteraction()
	{
		EditorWindow.GetWindow(typeof(ArcTeleportDependencyChecker), true, "VR Interaction Dependency", true);
	}
	#endif

	void OnGUI()
	{
		#if !VRInteraction
		EditorGUILayout.HelpBox("This asset requires VRInteraction to work. Please download and import the VRInteraction asset to continue", MessageType.Info);
		if (GUILayout.Button("VRInteraction"))
		{
			Application.OpenURL("https://assetstore.unity.com/packages/tools/input-management/vr-interaction-119934");
		}
		if (GUILayout.Button("VR Interaction Github"))
		{
			Application.OpenURL("https://github.com/MasOverflow/VR-Interaction");
		}
		#else
		Close();
		#endif
	}
}
