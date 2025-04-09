using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Linq;

public class InteractionManager : MonoBehaviour
{
	public enum Interactions
	{
		PickUpToy = 0,
		ThrowToy,
		PickUpFirewood,
		ThrowFirewood,
		CloseWindows,
		FlushToilet,
		CloseSkylightWithRemote,
		CloseBasementHatch,
	}

	private float interactionDistance = 1.8f;
	private float objectLockDistance = 1.2f;
	private float interactableMovementSpeed = 20.0f;

	public LayerMask interactableLayer;
	public LayerMask obstacleLayer;

	private PlayerController playerController = null;
	private Camera playerCamera = null;

	private GameObject interactableObject;
	private Rigidbody interactableObjRigidBody;

	private Outline currentOutline;

	public Image interactionIcon;
	public Sprite defaultCrosshairSprite;
	public Sprite handIconSprite;
	public Sprite handGrabIconSprite;

	private void Awake()
	{
		if (interactionIcon == null)
		{
			interactionIcon = GameObject.Find("InteractionIcon")?.GetComponent<Image>();
			if (interactionIcon == null)
			{
				interactionIcon = FindObjectsOfType<Image>(true).FirstOrDefault(img => img.gameObject.name == "InteractionIcon");
			}

			if (interactionIcon == null)
				Debug.LogError("No icon");
		}
	}

	public void OnLocalPlayerSetup(PlayerController targetController, Camera targetCamera)
	{
		playerController = targetController;
		playerCamera = targetCamera;
	}

	public void ApplyMotionOnInteractable(Rigidbody rigidBody)
	{
		Vector3 endPoint = playerCamera.transform.position + (playerCamera.transform.forward * objectLockDistance);
		Vector3 direction = (endPoint - rigidBody.position).normalized;
		float distance = Vector3.Distance(rigidBody.position, endPoint);

		if (!Physics.Raycast(playerCamera.transform.position, direction, distance, obstacleLayer))
		{
			Vector3 targetVelocity = direction * (distance * interactableMovementSpeed);
			rigidBody.velocity = Vector3.Lerp(rigidBody.velocity, targetVelocity, Time.deltaTime * interactableMovementSpeed);
			rigidBody.velocity *= 0.95f;
			rigidBody.angularVelocity *= 0.9f;
		}
	}

	public void OnLocalPlayerUpdate()
	{
		Ray outlineRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
		if (Physics.Raycast(outlineRay, out RaycastHit outlineHit, interactionDistance))
		{
			Outline outline = outlineHit.collider.GetComponentInChildren<Outline>();
			if (outline != null)
			{
				if (currentOutline != outline)
				{
					if (currentOutline != null)
						currentOutline.enabled = false;
					currentOutline = outline;
				}
				currentOutline.enabled = true;
			}
			else
			{
				if (currentOutline != null)
				{
					currentOutline.enabled = false;
					currentOutline = null;
				}
			}
		}
		else
		{
			if (currentOutline != null)
			{
				currentOutline.enabled = false;
				currentOutline = null;
			}
		}

		if (interactableObject != null)
		{
			Outline heldOutline = interactableObject.GetComponentInChildren<Outline>();
			if (heldOutline != null)
			{
				heldOutline.enabled = true;
			}
		}

		Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
		if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
		{
			if (hit.collider != null)
				OnObjectTraceCollide(hit.transform.gameObject, hit);
		}

		if (interactableObject != null)
		{
			if (interactableObject.IsDestroyed())
			{
				interactableObject = null;
				interactableObjRigidBody = null;
			}
			else if (!playerController.playerControlActions.Player.Interact.IsPressed())
			{
				if (interactableObjRigidBody)
				{
					interactableObjRigidBody.useGravity = true;
					interactableObjRigidBody.constraints = RigidbodyConstraints.None;
					interactableObjRigidBody = null;

					if (interactableObject.tag == "Interactable_Blocks")
					{
						GameObject parent = interactableObject.transform.parent.gameObject;
						if (parent)
						{
							Rigidbody[] blocks = parent.GetComponentsInChildren<Rigidbody>();
							for (int i = 0; i < blocks.Length; i++)
							{
								Rigidbody blockRigidBody = blocks[i];
								if (blockRigidBody)
								{
									blockRigidBody.useGravity = true;
									blockRigidBody.constraints = RigidbodyConstraints.None;
								}
							}
						}
					}
				}

				Outline heldOutline = interactableObject.GetComponentInChildren<Outline>();
				if (heldOutline != null)
				{
					heldOutline.enabled = false;
				}

				interactableObject = null;
			}
			else
			{
				if (interactableObjRigidBody)
				{
					if (interactableObject.tag == "Interactable_Blocks")
					{
						GameObject parent = interactableObject.transform.parent.gameObject;
						if (parent)
						{
							Rigidbody[] blocks = parent.GetComponentsInChildren<Rigidbody>();
							for (int i = 0; i < blocks.Length; i++)
							{
								Rigidbody blockRigidBody = blocks[i];
								if (blockRigidBody)
								{
									ApplyMotionOnInteractable(blockRigidBody);
								}
							}
						}
					}
					ApplyMotionOnInteractable(interactableObjRigidBody);
				}
				else
				{
					interactableObject.transform.position = playerCamera.transform.position + playerCamera.transform.forward;
				}
			}
		}

		bool isLookingAtInteractable = false;
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit iconHit, interactionDistance))
		{
			if (iconHit.collider != null)
			{
				string tag = iconHit.collider.gameObject.tag;
				if (
					tag.Contains("Interactable_") ||
					tag == "OutlineCommon" ||
					tag == "Toilet_Flush" ||
					tag == "BasementHatch_Door" ||
					tag == "Candy" ||
					tag == "Skylight_Remote" ||
					tag == "PowerRestore"
				)
				{
					isLookingAtInteractable = true;
				}
			}
		}

		if (interactionIcon != null)
		{
			if (interactableObject != null)
			{
				interactionIcon.sprite = handGrabIconSprite;
			}
			else if (isLookingAtInteractable)
			{
				interactionIcon.sprite = handIconSprite;
			}
			else
			{
				interactionIcon.sprite = defaultCrosshairSprite;
			}
		}
	}

	private void OnObjectTraceCollide(GameObject gameObj, RaycastHit hit)
	{
		bool interactTriggered = playerController.playerControlActions.Player.Interact.triggered;

		if (gameObj.tag.Contains("Interactable_"))
		{
			if (interactTriggered)
			{
				interactableObject = gameObj;
				interactableObjRigidBody = interactableObject.GetComponent<Rigidbody>();
				interactableObjRigidBody.useGravity = false;
				interactableObjRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
				interactableObjRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
				interactableObjRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

				if (interactableObject.tag == "Interactable_Blocks")
				{
					GameObject parent = interactableObject.transform.parent.gameObject;
					if (parent)
					{
						Rigidbody[] blocks = parent.GetComponentsInChildren<Rigidbody>();
						for (int i = 0; i < blocks.Length; i++)
						{
							Rigidbody blockRigidBody = blocks[i];
							if (blockRigidBody)
							{
								blockRigidBody.useGravity = false;
								blockRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
								blockRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
								blockRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
							}
						}
					}
				}
			}
		}

		if (interactTriggered)
		{
			if (gameObj.GetComponent<WindowsActivity>() != null)
				gameObj.GetComponent<WindowsActivity>().ResetActivity();

			if (gameObj.tag.Contains("BasementHatch_Door"))
				FindObjectOfType<BasementHatch>().ResetActivity();

			if (gameObj.tag.Contains("PowerRestore"))
				FindObjectOfType<ActivityDirector>().RestorePower();

            if (gameObj.tag.Contains("Skylight_Remote"))
				FindObjectOfType<SkylightActivity>().ResetActivity();

			if (gameObj.tag.Contains("Toilet_Flush"))
				FindObjectOfType<ToiletActivity>().ResetActivity();

			if (gameObj.tag.Contains("Telephone"))
				FindObjectOfType<ActivityDirector>().StopPhoneRing();

			if (gameObj.tag.Contains("Candy"))
			{
				if (FindObjectOfType<PlayerController>().EatCandy())
					Destroy(gameObj);
			}
		}
	}
}
