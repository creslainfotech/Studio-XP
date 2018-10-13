//========= Copyright 2017, Sam Tague, All rights reserved. ===================
//
// If using both ArcTeleport and VRSmooth you can add this script and specify an
// action to switching the movement modes. Or just call the SwitchLoco method from
// your own script.
//
//===================Contact Email: Sam@MassGames.co.uk===========================
#if VRInteraction
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRInteraction;

namespace VRArcTeleporter
{
	public class VRLocomotionSwitcher : MonoBehaviour 
	{
		public enum LocoType
		{
			ARCTELEPORTER,
			SMOOTH
		}

		public string switchAction = "SWITCH";
		public AudioClip switchSound;

		ArcTeleporter _teleporter;
		VRSmooth _smooth;
		VRInteractor _interactor;
		LocoType _currentType;

		void Start()
		{
			_teleporter = GetComponent<ArcTeleporter>();
			_smooth = GetComponent<VRSmooth>();
			_interactor = GetComponent<VRInteractor>();
			if (_teleporter == null)
			{
				Debug.LogError("Missing ArcTeleporter", gameObject);
				return;
			}
			if (_smooth == null)
			{
				Debug.LogError("Missing VRSmooth", gameObject);
				return;
			}

			_currentType = (LocoType)PlayerPrefs.GetInt("LocoType", 0);
			if (_teleporter.enabled == _smooth.enabled)
			{
				_teleporter.enabled = _currentType == LocoType.ARCTELEPORTER;
				_smooth.enabled = _currentType == LocoType.SMOOTH;
			}
		}

		void InputReceived(string action)
		{
			if (action == switchAction)
			{
				if (switchSound != null) AudioSource.PlayClipAtPoint(switchSound, transform.position);

				SwitchLoco(_currentType == LocoType.ARCTELEPORTER ? LocoType.SMOOTH : LocoType.ARCTELEPORTER);
				PlayerPrefs.SetInt("LocoType", (int)_currentType);
			}
		}

		public void SwitchLoco(LocoType newLocoType, bool tellOtherHand = true)
		{
			_currentType = newLocoType;
			_teleporter.enabled = newLocoType == LocoType.ARCTELEPORTER;
			_smooth.enabled = newLocoType == LocoType.SMOOTH;

			if (!tellOtherHand || _interactor == null) return;

			VRInteractor otherHand = _interactor.GetOtherController();
			if (otherHand == null) return;

			VRLocomotionSwitcher otherLocoSwitch = otherHand.GetComponent<VRLocomotionSwitcher>();
			if (otherLocoSwitch == null) return;

			otherLocoSwitch.SwitchLoco(newLocoType, false);
		}
	}
}
#endif