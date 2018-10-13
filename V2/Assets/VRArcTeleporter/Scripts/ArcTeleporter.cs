//========= Copyright 2017, Sam Tague, All rights reserved. ===================
//
// Uses SHOW and TELEPORT methods from VRInput to teleport using either an arc
// or customizable projectile. Lots of options from the look of the arc to the
// transition method.
//
//===================Contact Email: Sam@MassGames.co.uk===========================
#if VRInteraction
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRInteraction;

namespace VRArcTeleporter
{
	public class ArcTeleporter : MonoBehaviour 
	{
		public enum ArcMaterial
		{
			MATERIAL,
			COLOUR
		}
		public enum ControlScheme
		{
			TWO_BUTTON_MODE,
			PRESS_AND_RELEASE
		}
		public enum Transition
		{
			INSTANT,
			FADE,
			DASH
		}
		public enum FiringMode
		{
			ARC,
			PROJECTILE
		}
		public enum ArcImplementation
		{
			FIXED_ARC,
			PHYSICS_ARC
		}

		public ControlScheme controlScheme = ControlScheme.PRESS_AND_RELEASE;
		public FiringMode firingMode = FiringMode.ARC;
		public ArcImplementation arcImplementation = ArcImplementation.FIXED_ARC;

		public Transition transition;
		public Material fadeMat;
		public float fadeDuration = 0.5f;
		public GameObject teleportProjectilePrefab;

		public bool onlyLandOnFlat = true;
		[Tooltip("Anywhere flat slope limit")]
		public float slopeLimit = 20;
		public bool onlyLandOnTag = false;
		public List<string> tags = new List<string>();
		public float gravity = 9f;
		public float initialVelMagnitude = 10f;
		public float timeStep = 0.1f;
		[Tooltip("Approximate max distance in unity units")]
		public float maxDistance = 5f;
		public float dashSpeed = 20f;
		public float teleportCooldown = 0f;
		public ArcMaterial arcMat = ArcMaterial.COLOUR;
		[Tooltip("Material for the arc leave null to use colour")]
		public Material goodTeleMat;
		public Material badTeleMat;
		public Shader goodTeleShader;
		public Shader badTeleShader;
		public float matScale = 5;
		public Vector2 texMovementSpeed = new Vector2(-0.035f, 0);
		[Tooltip("Arc colours for good and bad spots if no material is used")]
		public Color goodSpotCol = new Color(0, 0.6f, 1f, 0.2f);
		public Color badSpotCol = new Color(0.8f, 0, 0, 0.2f);
		public float arcLineWidth = 0.05f;
		[Tooltip("Leave empty to collide with everything")]
		public List<string> raycastLayer = new List<string>();
		[Tooltip("When true it will only hit things on the layers specified and when false will hit everything else.")]
		public bool ignoreRaycastLayers = false;
		public bool disableRoomRotationWithTrackpad;
		public bool useLastGoodSpot = true;
		public bool useTeleportHighlight = true;
		public GameObject teleportHighlight;
		public bool canPickupItems = false;
		public GameObject pickupHighlight;
		public bool hideWhenHoldingItem = true;
		public bool useRoomShapeHighlight = true;
		public GameObject roomShape;
		public Transform offsetTrans;
		public AudioClip onMoveSound;
		public Transform vrCamera;
		public Transform vrPlayArea;

		private VRInput input;
		private MeshRenderer fadeQuad;
		private GameObject projectileInstance;
		private Color fadeColour;
		private bool transitioning;
		private GameObject _teleportHighlightInstance;
		private GameObject _alternativeHighlightInstance;
		private GameObject _roomShapeInstance;
		private LayerMask raycastLayerMask;
		private LineRenderer _lineRenderer;
		private LineRenderer _lineRenderer2;
		private Quaternion _roomRotation;
		private Vector3 _roomPosition;
		private Vector3 _destination;
		private Vector3 _destinationNormal;
		private Vector2 oldTrackpadAxis = Vector2.zero;
		private float lastTeleportTime;
		private bool _goodSpot;
		private bool _lastGoodSpot;
		private bool _teleportActive;
		private VRInteractableItem _hoverItem;
		private VRInteractor _interactor;
		public bool teleportActive
		{
			get { return _teleportActive; }
		}
		public bool canTeleport
		{
			get { return _goodSpot || (useLastGoodSpot && _lastGoodSpot); }
		}

		protected virtual void Start ()
		{
			input = GetComponent<VRInput>();
			_interactor = GetComponent<VRInteractor>();
			if (vrCamera == null && Camera.main != null) vrCamera = Camera.main.transform;
			#if Int_SteamVR
			if (vrCamera == null && transform.root != null)
			{
				SteamVR_Camera steamVR_Camera = transform.root.GetComponentInChildren<SteamVR_Camera>();
				if (steamVR_Camera == null) vrCamera = steamVR_Camera.transform;
			}
			#endif
			if (vrCamera == null)
			{
				Debug.LogError("ArcTeleporter can't find camera!");
				enabled = false;
				return;
			}
			#if Int_SteamVR
			if (vrPlayArea == null)
			{
				SteamVR_PlayArea steamVR_PlayArea = transform.parent.gameObject.GetComponent<SteamVR_PlayArea>();
				if (steamVR_PlayArea != null) vrPlayArea = steamVR_PlayArea.transform;
			}
			#endif
			#if Int_Oculus
			if (vrPlayArea == null)
			{
				OvrAvatar ovrAvatar = transform.GetComponentInParent<OvrAvatar>();
				if (ovrAvatar != null && ovrAvatar.transform.parent != null) vrPlayArea = ovrAvatar.transform.parent;
				else if (ovrAvatar != null) vrPlayArea = ovrAvatar.transform;
			}
			#endif
			if (vrPlayArea == null && transform.parent != null) vrPlayArea = transform.parent;
			if (vrPlayArea == null)
			{
				Debug.LogError("ArcTeleporter must be a part of a vr rig, should at least have a parent");
				enabled = false;
				return;
			}

			lastTeleportTime = -teleportCooldown;

			GameObject arcParentObject = new GameObject("ArcTeleporter");
			arcParentObject.transform.localScale = vrPlayArea.localScale;
			GameObject arcLine1 = new GameObject("ArcLine1");
			arcLine1.transform.SetParent(arcParentObject.transform);
			_lineRenderer = arcLine1.AddComponent<LineRenderer>();
			GameObject arcLine2 = new GameObject("ArcLine2");
			arcLine2.transform.SetParent(arcParentObject.transform);
			_lineRenderer2 = arcLine2.AddComponent<LineRenderer>();
			_lineRenderer.startWidth = _lineRenderer.endWidth = arcLineWidth*vrPlayArea.localScale.magnitude;
			_lineRenderer2.startWidth = _lineRenderer2.endWidth = arcLineWidth*vrPlayArea.localScale.magnitude;
			if (arcMat == ArcMaterial.COLOUR && goodTeleShader != null)
			{
				_lineRenderer.material = new Material(goodTeleShader);
				_lineRenderer.material.SetColor("_Color", goodSpotCol);
				_lineRenderer2.material = new Material(goodTeleShader);
				_lineRenderer2.material.SetColor("_Color", goodSpotCol);
			} else if (goodTeleMat != null)
			{
				_lineRenderer.material = goodTeleMat;
				_lineRenderer2.material = goodTeleMat;
			}
			_lineRenderer.enabled = false;
			_lineRenderer2.enabled = false;

			if (teleportHighlight != null)
				_teleportHighlightInstance = (GameObject)Instantiate(teleportHighlight, Vector3.zero, Quaternion.identity);
			else
				_teleportHighlightInstance = new GameObject("TeleportHighlight");
			_teleportHighlightInstance.transform.SetParent(arcParentObject.transform);
			_teleportHighlightInstance.transform.localScale = Vector3.one;
			_teleportHighlightInstance.SetActive(false);
			if (roomShape != null)
				_roomShapeInstance = (GameObject)Instantiate(roomShape, Vector3.zero, Quaternion.identity);
			else
				_roomShapeInstance = new GameObject("TeleportRoom");
			_roomShapeInstance.transform.SetParent(arcParentObject.transform);
			_roomShapeInstance.transform.rotation = vrPlayArea.rotation;
			_roomShapeInstance.transform.localScale = Vector3.one;
			_roomShapeInstance.SetActive(false);
		}

		protected virtual void Update()
		{
			if (!teleportActive) return;
			switch(firingMode)
			{
			case FiringMode.ARC:
				CalculateArc();
				break;
			case FiringMode.PROJECTILE:
				CheckProjectilePoint();
				break;
			}
			PositionHightlightAndRoom();
		}

		private void PositionHightlightAndRoom()
		{
			if (_teleportHighlightInstance != null && _teleportHighlightInstance.activeSelf && (!useLastGoodSpot || _goodSpot || !_lastGoodSpot))
			{
				_teleportHighlightInstance.transform.position = _destination+(_destinationNormal*0.05f);
				if (_destinationNormal == Vector3.zero)
					_teleportHighlightInstance.transform.rotation = Quaternion.identity;
				else
				{
					if (_roomShapeInstance == null)
						_teleportHighlightInstance.transform.rotation = Quaternion.LookRotation(_destinationNormal);
					else
						_teleportHighlightInstance.transform.rotation = Quaternion.LookRotation(_destinationNormal, Quaternion.FromToRotation(vrPlayArea.transform.forward, _roomShapeInstance.transform.forward) * Camera.main.transform.forward);
				}
			}
			if (_roomShapeInstance != null && _roomShapeInstance.activeSelf && _teleportHighlightInstance != null)
			{
				Vector3 camSpot = new Vector3(vrCamera.position.x, 0, vrCamera.position.z);
				Vector3 roomSpot = new Vector3(vrPlayArea.position.x, 0, vrPlayArea.position.z);
				float angle = Quaternion.Angle(vrPlayArea.rotation, _roomShapeInstance.transform.rotation);
				Vector3 cross = Vector3.Cross(vrPlayArea.rotation * Vector3.forward, _roomShapeInstance.transform.rotation * Vector3.forward);
				if (cross.y < 0) angle = -angle;
				Quaternion difference = Quaternion.AngleAxis(angle, Vector3.up);
				Vector3 offset = difference * (roomSpot - camSpot);
				_roomShapeInstance.transform.position = (_destination + offset)+_destinationNormal*0.05f;
				if (!disableRoomRotationWithTrackpad)
				{
					Vector2 trackpadAxis = input.PadPosition;
					if (input.hmdType == VRInput.HMDType.VIVE)
					{
						if (trackpadAxis != Vector2.zero && oldTrackpadAxis != Vector2.zero)
						{
							float diff = (trackpadAxis.x-oldTrackpadAxis.x)*100;
							_roomShapeInstance.transform.RotateAround(_destination, Vector3.up, diff);
						}
						oldTrackpadAxis = trackpadAxis;
					} else //Oculus
					{
						_roomShapeInstance.transform.LookAt(_roomShapeInstance.transform.position + (vrPlayArea.rotation * new Vector3(trackpadAxis.x, 0, trackpadAxis.y)));
					}
				}
			}
		}

		private void CheckProjectilePoint()
		{
			if (!_teleportActive || projectileInstance == null) return;

			Ray ray = new Ray(projectileInstance.transform.position, Vector3.down);
			RaycastHit hit;
			//	Check if we hit something
			bool hitSomething = false;
			if (raycastLayer == null || raycastLayer.Count == 0)
				hitSomething = Physics.Raycast(ray, out hit, 1);
			else
			{
				LayerMask raycastLayerMask = 1 << LayerMask.NameToLayer(raycastLayer[0]);
				for(int j=1; j<raycastLayer.Count ; j++)
				{
					raycastLayerMask |= 1 << LayerMask.NameToLayer(raycastLayer[j]);
				}
				if (ignoreRaycastLayers) raycastLayerMask = ~raycastLayerMask;
				hitSomething = Physics.Raycast(ray, out hit, 1, raycastLayerMask);
			}
			_goodSpot = false;
			if (hitSomething)
			{
				_destination = hit.point;
				_destinationNormal = hit.normal;

				//PositionHightlightAndRoom();

				_goodSpot = IsGoodSpot(hit);

				if (_goodSpot)
				{
					if (_roomShapeInstance != null)
					{
						_roomShapeInstance.SetActive(true);
					}
				}
				if (_teleportHighlightInstance != null) _teleportHighlightInstance.SetActive(true);
			}
			if (!hitSomething) _teleportHighlightInstance.SetActive(false);
			if (!_goodSpot) _roomShapeInstance.SetActive(false);
		}

		private void CalculateArc()
		{
			//	Line renderer position storage (two because line renderer texture will stretch if one is used)
			List<Vector3> positions1 = new List<Vector3>();
			List<Vector3> positions2 = new List<Vector3>();
			//	first Vector3 positions array will be used for the curve and the second line renderer is used for the straight down after the curve
			bool useFirstArray = true;
			RaycastHit hit = new RaycastHit();
			float totalDistance1 = 0;
			float totalDistance2 = 0;

			//	Variables need for curve
			Vector3 currentPosition = transform.position;
			Quaternion currentRotation = transform.rotation;
			if (offsetTrans != null)
			{
				currentPosition = offsetTrans.position;
				currentRotation = offsetTrans.rotation;
			}
			Vector3 lastPostion;
			positions1.Add(currentPosition);

			switch(arcImplementation)
			{
			case ArcImplementation.FIXED_ARC:
				lastPostion = transform.position-transform.forward;
				Vector3 currentDirection = transform.forward;
				Vector3 downForward = new Vector3(transform.forward.x*0.01f, -1, transform.forward.z*0.01f);
				if (offsetTrans != null)
				{
					lastPostion = offsetTrans.position - offsetTrans.forward;
					currentDirection = offsetTrans.forward;
					downForward = new Vector3(offsetTrans.forward.x*0.01f, -1, offsetTrans.forward.z*0.01f);
				}

				//	Advance arc each iteration looking for a surface or until pointed staight down
				//	Should never come close to 500 iterations but just as safety to avoid indefinite looping
				int i=0;
				while(i<500)
				{
					i++;

					//	Rotate the current rotation toward the downForward rotation
					Quaternion downQuat = Quaternion.LookRotation(downForward);
					currentRotation = Quaternion.RotateTowards(currentRotation, downQuat, 1f);

					//	Make ray for new direction
					Ray newRay = new Ray(currentPosition, currentPosition-lastPostion);
					float length = (maxDistance*0.01f)*vrPlayArea.localScale.magnitude;
					if (currentRotation == downQuat)
					{
						//We have finished the arc and are facing down
						//So were going to use the second line renderer and extend the normal length as a last effort to hit something
						useFirstArray = false;
						length = (maxDistance*matScale)*vrPlayArea.localScale.magnitude;
						positions2.Add(currentPosition);
					}
					float raycastLength = length*1.1f;

					//	Check if we hit something
					bool hitSomething = false;
					if (raycastLayer == null || raycastLayer.Count == 0)
						hitSomething = Physics.Raycast(newRay, out hit, raycastLength);
					else
					{
						LayerMask raycastLayerMask = 1 << LayerMask.NameToLayer(raycastLayer[0]);
						for(int j=1; j<raycastLayer.Count ; j++)
						{
							raycastLayerMask |= 1 << LayerMask.NameToLayer(raycastLayer[j]);
						}
						if (ignoreRaycastLayers) raycastLayerMask = ~raycastLayerMask;
						hitSomething = Physics.Raycast(newRay, out hit, raycastLength, raycastLayerMask);
					}

					if (hitSomething)
					{
						//	Depending on whether we had switched to the first or second line renderer
						//	add the point and finish calculating the total distance
						if (useFirstArray)
						{
							totalDistance1 += (currentPosition-hit.point).magnitude;
							positions1.Add(hit.point);
						} else
						{
							totalDistance2 += (currentPosition-hit.point).magnitude;
							positions2.Add(hit.point);
						}
						_destinationNormal = hit.normal;
						//	And we're done
						break;
					}

					//	Convert the rotation to a forward vector and apply to our current position
					currentDirection = currentRotation * Vector3.forward;
					lastPostion = currentPosition;
					currentPosition += currentDirection*length;

					//	Depending on whether we have switched to the second line renderer add this point and update total distance
					if (useFirstArray)
					{
						totalDistance1 += length;
						positions1.Add(currentPosition);
					} else
					{
						totalDistance2 += length;
						positions2.Add(currentPosition);
					}

					//	If we're pointing down then we did the whole arc and down without hitting anything so we're done
					if (currentRotation == downQuat) break;
				}
				break;
			case ArcImplementation.PHYSICS_ARC:
				lastPostion = currentPosition;
				Vector3 initVel = transform.forward*(initialVelMagnitude*vrPlayArea.localScale.x);
				if (offsetTrans != null)
				{
					initVel = offsetTrans.forward*(initialVelMagnitude*vrPlayArea.localScale.x);
				}
				Vector3 velocity = initVel;
				Vector3 acc = new Vector3(0, -gravity*vrPlayArea.localScale.x, 0);
				int i2 = 0;
				while(i2<500)
				{
					i2++;
					velocity += acc * timeStep;
					currentPosition += velocity * timeStep;

					//	Make ray for new direction
					Ray newRay = new Ray(lastPostion, currentPosition-lastPostion);
					float length = Vector3.Distance(currentPosition, lastPostion);
					float raycastLength = length*1.1f;

					//	Check if we hit something
					bool hitSomething = false;
					if (raycastLayer == null || raycastLayer.Count == 0)
						hitSomething = Physics.Raycast(newRay, out hit, raycastLength);
					else
					{
						LayerMask raycastLayerMask = 1 << LayerMask.NameToLayer(raycastLayer[0]);
						for(int j=1; j<raycastLayer.Count ; j++)
						{
							raycastLayerMask |= 1 << LayerMask.NameToLayer(raycastLayer[j]);
						}
						if (ignoreRaycastLayers) raycastLayerMask = ~raycastLayerMask;
						hitSomething = Physics.Raycast(newRay, out hit, raycastLength, raycastLayerMask);
					}

					if (hitSomething)
					{
						totalDistance1 += Vector3.Distance(lastPostion, hit.point);
						positions1.Add(hit.point);
						_destinationNormal = hit.normal;
						//	And we're done
						break;
					}

					totalDistance1 += length;
					positions1.Add(currentPosition);
					lastPostion = currentPosition;
					if (totalDistance1 > 30f*vrPlayArea.localScale.x)
						break;
				}
				break;
			}

			if (useFirstArray)
			{
				_lineRenderer2.enabled = false;
				_destination = positions1[positions1.Count-1];
			} else
			{
				_lineRenderer2.enabled = true;
				_destination = positions2[positions2.Count-1];
			}

			//PositionHightlightAndRoom();

			//	Decide using the current teleport rule whether this is a good teleporting spot or not
			_goodSpot = IsGoodSpot(hit);

			//	Update line, teleport highlight and room highlight based on it being a good spot or bad
			if (_goodSpot)
			{
				if (arcMat == ArcMaterial.COLOUR)
				{
					_lineRenderer.startColor = _lineRenderer.endColor = goodSpotCol;
					_lineRenderer.material.SetColor("_Color", goodSpotCol);
					_lineRenderer2.startColor = _lineRenderer2.endColor = goodSpotCol;
					_lineRenderer2.material.SetColor("_Color", goodSpotCol);
				} else if (goodTeleMat != null)
				{
					if (_lineRenderer.material.mainTexture.name != goodTeleMat.mainTexture.name) _lineRenderer.material = goodTeleMat;
					if (_lineRenderer2.material.mainTexture.name != goodTeleMat.mainTexture.name) _lineRenderer2.material = goodTeleMat;
				}
				if (_roomShapeInstance != null)
				{
					_roomShapeInstance.SetActive(true);
				}
			} else
			{
				if (arcMat == ArcMaterial.COLOUR)
				{
					_lineRenderer.startColor = _lineRenderer.endColor = badSpotCol;
					_lineRenderer.material.SetColor("_Color", badSpotCol);
					_lineRenderer2.startColor = _lineRenderer2.endColor = badSpotCol;
					_lineRenderer2.material.SetColor("_Color", badSpotCol);
				} else if (badTeleMat != null)
				{
					if (_lineRenderer.material.mainTexture.name != badTeleMat.mainTexture.name) _lineRenderer.material = badTeleMat;
					if (_lineRenderer2.material.mainTexture.name != badTeleMat.mainTexture.name) _lineRenderer2.material = badTeleMat;
				}
				if (_roomShapeInstance != null) _roomShapeInstance.SetActive(false);
			}
				
			_lineRenderer.positionCount = positions1.Count;
			_lineRenderer.SetPositions(positions1.ToArray());
			_lineRenderer.material.mainTextureScale = new Vector2((totalDistance1*matScale)/vrPlayArea.localScale.magnitude, 1);
			_lineRenderer.material.mainTextureOffset = new Vector2(_lineRenderer.material.mainTextureOffset.x+texMovementSpeed.x, _lineRenderer.material.mainTextureOffset.y+texMovementSpeed.y);

			if (_lineRenderer2.enabled)
			{
				_lineRenderer2.positionCount = positions2.Count;
				_lineRenderer2.SetPositions(positions2.ToArray());
				_lineRenderer2.material.mainTextureScale = new Vector2((totalDistance2*matScale)/vrPlayArea.localScale.magnitude, 1);
				_lineRenderer2.material.mainTextureOffset = new Vector2(_lineRenderer2.material.mainTextureOffset.x+texMovementSpeed.x, _lineRenderer2.material.mainTextureOffset.y+texMovementSpeed.y);
			}
		}

		//	Overide and change to expand on what is a good landing spot
		virtual protected bool IsGoodSpot(RaycastHit hit)
		{
			if (hit.transform == null) return false;

			if (canPickupItems)
			{
				VRInteractableItem item = hit.collider.GetComponent<VRInteractableItem>();
				if (item == null)
				{
					VRItemCollider itemCollider = hit.collider.GetComponent<VRItemCollider>();
					if (itemCollider != null) item = itemCollider.item;
				}
				if (item != null && _hoverItem == null)
				{
					_hoverItem = item;
					if (teleportHighlight != null && pickupHighlight != null)
					{
						if (_alternativeHighlightInstance == null) _alternativeHighlightInstance = Instantiate<GameObject>(pickupHighlight);
						GameObject tempHighlight = _teleportHighlightInstance;
						_teleportHighlightInstance = _alternativeHighlightInstance;
						_alternativeHighlightInstance = tempHighlight;
						_alternativeHighlightInstance.SetActive(false);
						_teleportHighlightInstance.SetActive(true);
					}
				} else if (item == null && _hoverItem != null)
				{
					_hoverItem = null;
					if (teleportHighlight != null && pickupHighlight != null)
					{
						if (_alternativeHighlightInstance == null) _alternativeHighlightInstance = Instantiate<GameObject>(teleportHighlight);
						GameObject tempHighlight = _teleportHighlightInstance;
						_teleportHighlightInstance = _alternativeHighlightInstance;
						_alternativeHighlightInstance = tempHighlight;
						_alternativeHighlightInstance.SetActive(false);
						_teleportHighlightInstance.SetActive(true);
					}
				}
			}

			if (onlyLandOnFlat)
			{
				float angle = Vector3.Angle(Vector3.up, hit.normal);
				if (angle > slopeLimit)
					return false;
			}
			if (onlyLandOnTag)
			{
				bool foundSpot = false;
				foreach(string tag in tags)
				{
					if (hit.transform.tag == tag)
						foundSpot = true;
				}
				if (!foundSpot) return false;
			}
			_lastGoodSpot = true;
			return true;
		}

		public void InputReceived(string method)
		{
			if (method == "SHOW" || method == "SHOWReleased" ||
				method == "TELEPORT" || method == "TELEPORTReleased")
				SendMessage(method, SendMessageOptions.RequireReceiver);
		}

		protected virtual void SHOW()
		{
			if (controlScheme == ControlScheme.TWO_BUTTON_MODE)
				EnableTeleport();
		}

		protected virtual void SHOWReleased()
		{
			if (controlScheme == ControlScheme.TWO_BUTTON_MODE)
				DisableTeleport();
		}

		protected virtual void TELEPORT()
		{
			switch(controlScheme)
			{
			case ControlScheme.TWO_BUTTON_MODE:
				Teleport();
				break;
			case ControlScheme.PRESS_AND_RELEASE:
				EnableTeleport();
				break;
			}
		}

		protected virtual void TELEPORTReleased()
		{
			if (controlScheme == ControlScheme.PRESS_AND_RELEASE)
			{
				Teleport();
				DisableTeleport();
			}
		}

		public virtual void EnableTeleport()
		{
			if (transitioning || !enabled || (_interactor != null && hideWhenHoldingItem && _interactor.heldItem != null)) return;
			_teleportActive = true;
			_lastGoodSpot = false;
			switch(firingMode)
			{
			case FiringMode.ARC:
				_lineRenderer.enabled = true;
				if (_teleportHighlightInstance != null) _teleportHighlightInstance.SetActive(true);
				if (_roomShapeInstance != null)
				{
					_roomShapeInstance.transform.rotation = vrPlayArea.rotation;
					_roomShapeInstance.SetActive(true);
				}
				CalculateArc();
				break;
			case FiringMode.PROJECTILE:
				if (teleportProjectilePrefab == null)
				{
					Debug.LogError("Teleport projectile prefab is not set");
					return;
				}
				if (projectileInstance != null) DisableTeleport();
				projectileInstance = (GameObject)Instantiate(teleportProjectilePrefab, Vector3.zero, Quaternion.identity);
				if (offsetTrans == null) projectileInstance.transform.position = transform.position+(transform.forward*0.5f);
				else projectileInstance.transform.position = offsetTrans.position+(transform.forward*0.5f);
				Rigidbody body = projectileInstance.GetComponentInChildren<Rigidbody>();
				if (body == null)
				{
					Debug.LogError("Teleport projectile has no rigidbody");
					return;
				}
				body.AddForce(transform.forward*(maxDistance*100)*vrPlayArea.localScale.x);
				CheckProjectilePoint();
				break;
			}
		}

		public virtual void DisableTeleport()
		{
			_teleportActive = false;
			switch(firingMode)
			{
			case FiringMode.ARC:
				if (_lineRenderer)
				{
					_lineRenderer.enabled = false;
					_lineRenderer2.enabled = false;
				}
				break;
			case FiringMode.PROJECTILE:
				Destroy(projectileInstance);
				break;
			}
			if (_teleportHighlightInstance != null)
				_teleportHighlightInstance.SetActive(false);
			if (_roomShapeInstance != null)
			{
				_roomShapeInstance.transform.rotation = vrPlayArea.rotation;
				_roomShapeInstance.transform.position = vrPlayArea.position;
				_roomShapeInstance.SetActive(false);
				oldTrackpadAxis = Vector2.zero;
			}
		}

		public virtual void Teleport()
		{
			if (_interactor != null && hideWhenHoldingItem && _interactor.heldItem != null) return;
			if (_hoverItem != null)
			{
				if (_interactor == null)
				{
					Debug.LogError("Need VRInteractor attached to same object as teleporter to pickup items", gameObject);
					return;
				}
				if (_interactor.hoverItem != null) _interactor.hoverItem.DisableHover(_interactor);
				_interactor.hoverItem = _hoverItem;
				_interactor.TryPickup();
				_hoverItem = null;
				if (teleportHighlight != null && pickupHighlight != null)
				{
					if (_alternativeHighlightInstance == null) _alternativeHighlightInstance = Instantiate<GameObject>(teleportHighlight);
					GameObject tempHighlight = _teleportHighlightInstance;
					_teleportHighlightInstance = _alternativeHighlightInstance;
					_alternativeHighlightInstance = tempHighlight;
					_alternativeHighlightInstance.SetActive(false);
					_teleportHighlightInstance.SetActive(true);
				}
				return;
			}

			if (transitioning || (Time.time - lastTeleportTime) < teleportCooldown || !enabled) return;
			if (canTeleport)
			{
				_roomRotation = _roomShapeInstance.transform.rotation;
				_roomPosition = _roomShapeInstance.transform.position;
			}
			if (onMoveSound != null) AudioSource.PlayClipAtPoint(onMoveSound, transform.position);
			switch(firingMode)
			{
			case FiringMode.ARC:
				if (teleportActive && canTeleport)
				{
					switch(transition)
					{
					case Transition.INSTANT:
						MoveToTarget();
						lastTeleportTime = Time.time;
						break;
					case Transition.FADE:
						if (fadeQuad == null) 
						{
							CreateFadeQuad();
							StartCoroutine(fadeTransition());
							lastTeleportTime = Time.time;
						}
						break;
					case Transition.DASH:
						StartCoroutine(dashTransition());
						lastTeleportTime = Time.time;
						break;
					}
				}
				break;
			case FiringMode.PROJECTILE:
				if (projectileInstance != null)
				{
					switch(transition)
					{
					case Transition.INSTANT:
						if (canTeleport)
						{
							MoveToTarget();
							lastTeleportTime = Time.time;
						}
						break;
					case Transition.FADE:
						if (canTeleport && fadeQuad == null) 
						{
							CreateFadeQuad();
							StartCoroutine(fadeTransition());
							lastTeleportTime = Time.time;
						}
						break;
					case Transition.DASH:
						StartCoroutine(dashTransition());
						lastTeleportTime = Time.time;
						break;
					}
				}
				break;
			}
		}

		protected virtual void MoveToTarget()
		{
			vrPlayArea.position = _roomPosition;
			vrPlayArea.rotation = _roomRotation;
			if (_roomShapeInstance != null)
			{
				_roomShapeInstance.transform.rotation = vrPlayArea.rotation;
				_roomShapeInstance.transform.position = vrPlayArea.position;
				_roomShapeInstance.SetActive(false);
				oldTrackpadAxis = Vector2.zero;
			}
		}

		protected virtual void CreateFadeQuad()
		{
			GameObject fadeQuadObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
			fadeQuadObj.transform.SetParent(vrCamera);
			Vector3 fadeQuadPosition = vrCamera.position + (vrCamera.forward*(vrPlayArea.localScale.magnitude*0.01f));
			Vector3 fadeQuadScale = Vector3.one;
			if (Camera.main.nearClipPlane != 0) 
			{
				fadeQuadPosition += vrCamera.forward*Camera.main.nearClipPlane;
				if (Camera.main.nearClipPlane > 1)
					fadeQuadScale *= Camera.main.nearClipPlane*5f;
			}
			fadeQuadObj.transform.position = fadeQuadPosition;
			fadeQuadObj.transform.LookAt(fadeQuadObj.transform.position+vrCamera.forward, vrCamera.up);
			fadeQuadObj.transform.localScale = fadeQuadScale;
			Destroy(fadeQuadObj.GetComponent<Collider>());
			fadeQuad = fadeQuadObj.GetComponent<MeshRenderer>();
			fadeQuad.material = fadeMat;
			fadeQuad.receiveShadows = false;
			fadeQuad.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			fadeQuad.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			fadeColour = fadeQuad.material.color;
		}

		protected virtual IEnumerator fadeTransition()
		{
			if (fadeQuad == null) yield break;
			transitioning = true;
			Color transparent = new Color(0, 0, 0, 0);

			fadeQuad.material.color = transparent;
			float t = 0;
			float startTime = Time.time;
			while(t < 1)
			{
				if (fadeQuad == null)
				{
					transitioning = false;
					yield break;
				}
				float currentTime = Time.time;
				float elapsedTime = currentTime - startTime;
				t = elapsedTime / (fadeDuration*0.5f);
				fadeQuad.material.color = Color.Lerp(transparent, fadeColour, t);
				yield return null;
			}

			MoveToTarget();

			startTime = Time.time;
			t = 0;
			while(t < 1)
			{
				if (fadeQuad == null)
				{
					transitioning = false;
					yield break;
				}
				float currentTime = Time.time;
				float elapsedTime = currentTime - startTime;
				t = elapsedTime / (fadeDuration*0.5f);
				fadeQuad.material.color = Color.Lerp(fadeColour, transparent, t);
				yield return null;
			}
			if (fadeQuad == null)
			{
				transitioning = false;
				yield break;
			}
			Destroy(fadeQuad.gameObject);
			transitioning = false;
		}

		protected virtual IEnumerator dashTransition()
		{
			vrPlayArea.rotation = _roomRotation;

			Vector3 initRoomPos = _roomPosition;
			while (0.01f < Vector3.Distance(vrPlayArea.position, _roomPosition))
			{
				if (_roomPosition != initRoomPos) yield break;
				float step = dashSpeed * Time.deltaTime;
				vrPlayArea.position = Vector3.MoveTowards(vrPlayArea.position, _roomPosition, step);
				yield return null;
			}
				
			vrPlayArea.position = _roomPosition;
			if (_roomShapeInstance != null)
			{
				_roomShapeInstance.transform.rotation = vrPlayArea.rotation;
				_roomShapeInstance.transform.position = vrPlayArea.position;
				_roomShapeInstance.SetActive(false);
				oldTrackpadAxis = Vector2.zero;
			}
		}
		void OnDisable()
		{
			DisableTeleport();
		}
	}
}
#endif
