using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;

// Handles the player movement and interaction

public class PlayerController : MonoBehaviour
{
	[Header("Camera")]
	public Camera playerCamera;

	[Header("Movement Settings")]
	public float walkSpeed = 4f;
	public float runSpeed = 6f;
	public float jumpHeight = 1f;
	public float lookSensitivity = 0.1f;

	[Header("FOV Settings")]
	public float normalFOV = 75f;
	public float sprintFOV = 80f;
	private float currentFOV;

	[Header("Stamina Settings")]
	public float maxStamina = 100f;
	public float staminaDrainRate = 10f;
	public float staminaRechargeRate = 8f;
	public float staminaCoolDown = 1f;
	private float staminaCoolDownTimer = 0f;
	private float currentStamina;

	[Header("Stamina UI")]
	public Image staminaBar;
	public float staminaFadeSpeed = 1f;

	[Header("Head Bob Settings")]
	public float headBobPower = 0.05f;
	public float headBobSpeed = 5f;

	[Header("Flashlight Settings")]
	public GameObject flashlightGameObject;
	public Light flashlight;
	public float maxLightIntensity = 5f;
	public float minLightIntensity = 0f;
	public float maxLightRange = 15f;
	public float minLightRange = 0f;
	public float drainRate = 0.1f;
	private float currentLightIntensity;
	private float currentLightRange;
	private float rangeDrainRate;
	private bool isRecharging = false;

	private bool flashlightEnabled = true;
	private Animation flashlightShake;

	private CharacterController characterController;
	private Vector3 moveDirection = Vector3.zero;

	private float gravity = -9.81f;
	private float verticalVelocity = 0f;
	private float currentSpeed = 0f;
	private float verticalRotation = 0f;
	private bool isRunning = false;
	private bool isWalking = false;

	private float headBobTimer = 0f;
	private Vector3 cameraOriginalPosition;

	private InteractionManager interactionManager;

	private GameObject footstepsGameObject;
	private AudioSource footsteps;

	public PlayerControlActions playerControlActions;

	private bool isDead = false;

	private Vector2 movementInput;
	private Vector2 lookInput;

	private void Awake()
	{
		playerControlActions = new PlayerControlActions();

        staminaBar = GameObject.Find("StaminaBar")?.GetComponent<Image>();

        if (staminaBar == null)
        {
            staminaBar = FindObjectsOfType<Image>(true)
                .FirstOrDefault(img => img.gameObject.name == "StaminaBar");
        }

        if (staminaBar == null)
		{
			Debug.LogError("No Stamina Bar");
		}
	}

	private void OnEnable()
	{
		playerControlActions.Player.Enable();
	}

	private void OnDisable()
	{
		playerControlActions.Player.Disable();
	}

	private void Start()
	{
		int interactableLayer = LayerMask.NameToLayer("Interactable");
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Interactable"), true);

		footstepsGameObject = transform.Find("Footsteps").gameObject;
		footsteps = footstepsGameObject.GetComponent<AudioSource>();
		characterController = GetComponent<CharacterController>();
		interactionManager = FindObjectOfType<InteractionManager>();
		currentFOV = normalFOV;
		currentStamina = maxStamina;

		flashlightShake = flashlightGameObject.GetComponent<Animation>();

		cameraOriginalPosition = playerCamera.transform.localPosition;

		if (interactionManager != null)
			interactionManager.OnLocalPlayerSetup(this, playerCamera);

		if (flashlight != null)
		{
			currentLightIntensity = maxLightIntensity;
			flashlight.intensity = currentLightIntensity;

			currentLightRange = maxLightRange;
			flashlight.range = currentLightRange;

			if (maxLightIntensity != minLightIntensity)
			{
				rangeDrainRate = drainRate * (maxLightRange - minLightRange) / (maxLightIntensity - minLightIntensity);
			}
			else
			{
				rangeDrainRate = 0f;
			}
		}
	}

	private void Update()
	{
		if (isDead)
			return;

		if (playerControlActions.Player.FlashlightToggle.triggered && !isRecharging)
		{
			SoundManager.Instance.PlaySound("Flashlight");
			flashlightEnabled = !flashlightEnabled;
			if (flashlight != null)
			{
				flashlight.enabled = flashlightEnabled;
			}
		}

		RetrieveDataFromInputSystem();
		HandleMovement();
		HandleLook();
		UpdateFOV();
		HandleHeadBob();
		HandleFlashlight();

		if (isRunning)
		{
			staminaCoolDownTimer = 0f;
		}
		else
		{
			staminaCoolDownTimer += Time.deltaTime;
			if (staminaCoolDownTimer >= staminaCoolDown && currentStamina < maxStamina)
			{
				currentStamina += staminaRechargeRate * Time.deltaTime;
				currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
			}
		}

		if (staminaBar != null)
		{
			float staminaRatio = currentStamina / maxStamina;
			staminaBar.fillAmount = staminaRatio;

			Color baseColor;
			if (staminaRatio < 0.25f)
			{
				baseColor = Color.red;
			}
			else if (staminaRatio < 0.5f)
			{
				baseColor = Color.yellow;
			}
			else
			{
				baseColor = Color.white;
			}

			float targetAlpha = (currentStamina >= maxStamina) ? 0f : 1f;
			Color currentColor = staminaBar.color;
			baseColor.a = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * staminaFadeSpeed);
			staminaBar.color = baseColor;
		}

		if (interactionManager != null)
			interactionManager.OnLocalPlayerUpdate();
	}

	private void RetrieveDataFromInputSystem()
	{
		movementInput = playerControlActions.Player.Move.ReadValue<Vector2>();
		lookInput = playerControlActions.Player.Look.ReadValue<Vector2>();
	}

	private void HandleMovement()
	{
		float moveX = movementInput.x;
		float moveZ = movementInput.y;

		Vector3 inputDirection = transform.right * moveX + transform.forward * moveZ;

		if (inputDirection.magnitude > 1f)
		{
			inputDirection.Normalize();
		}

		if (playerControlActions.Player.Sprint.IsPressed() && currentStamina > 0 && inputDirection.magnitude > 0)
		{
			isWalking = false;
			isRunning = true;
			currentStamina -= staminaDrainRate * Time.deltaTime;
			if (currentStamina < 0)
				currentStamina = 0;
		}
		else
		{
			isWalking = true;
			isRunning = false;
		}

		float targetSpeed = isRunning ? runSpeed : walkSpeed;
		if (inputDirection.magnitude == 0)
			targetSpeed = 0;

		currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);

		moveDirection = inputDirection * currentSpeed;

		if (characterController.isGrounded)
		{
			//if (playerControlActions.Player.Jump.triggered) // no bunnyhop
			if (playerControlActions.Player.Jump.IsPressed()) // bunnyhop
			{
				verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
				footsteps.Stop();
			}
		}
		else
		{
			verticalVelocity += gravity * Time.deltaTime;
		}

		moveDirection.y = verticalVelocity;
		characterController.Move(moveDirection * Time.deltaTime);

		if (isRunning && characterController.isGrounded && inputDirection.magnitude > 0f && currentStamina > 0)
		{
			SoundManager.Instance.PlaySound("PlayerRun", footsteps);
		}
		else if (isWalking && characterController.isGrounded && inputDirection.magnitude > 0f)
		{
			SoundManager.Instance.PlaySound("PlayerWalk", footsteps);
		}
		else
		{
			footsteps.Stop();
		}
	}

	private void HandleLook()
	{
		if (UIManager.Instance != null && (UIManager.Instance.IsPaused() || UIManager.Instance.IsMainMenuActive()))
			return;

		float mouseX = lookInput.x * lookSensitivity;
		float mouseY = lookInput.y * lookSensitivity;

		transform.Rotate(Vector3.up * mouseX);

		verticalRotation -= mouseY;
		verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

		playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
	}

	private void UpdateFOV()
	{
		float targetFOV = isRunning ? sprintFOV : normalFOV;
		currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * 5f);
		playerCamera.fieldOfView = currentFOV;
	}

	private void HandleHeadBob()
	{
		if (currentSpeed > 0 && characterController.isGrounded)
		{
			float bobSpeed = currentSpeed * headBobSpeed / walkSpeed;
			headBobTimer += Time.deltaTime * bobSpeed;
			float bobOffsetY = Mathf.Sin(headBobTimer) * headBobPower;
			playerCamera.transform.localPosition = cameraOriginalPosition + new Vector3(0f, bobOffsetY, 0f);
		}
		else
		{
			headBobTimer = 0f;
			playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, cameraOriginalPosition, Time.deltaTime * 5f);
		}
	}

	private void HandleFlashlight()
	{
		if (flashlight == null || !flashlightEnabled)
			return;

		if (!isRecharging)
		{
			currentLightIntensity -= drainRate * Time.deltaTime;
			currentLightIntensity = Mathf.Clamp(currentLightIntensity, minLightIntensity, maxLightIntensity);
			flashlight.intensity = currentLightIntensity;

			currentLightRange -= rangeDrainRate * Time.deltaTime;
			currentLightRange = Mathf.Clamp(currentLightRange, minLightRange, maxLightRange);
			flashlight.range = currentLightRange;
		}

		if (playerControlActions.Player.FlashlightRecharge.triggered && !isRecharging)
		{
			StartCoroutine(RechargeFlashlight());
		}
	}

	private IEnumerator RechargeFlashlight()
	{
		isRecharging = true;

		flashlight.enabled = false;
		SoundManager.Instance.PlaySound("Flashlight");
		SoundManager.Instance.PlaySound("FlashlightShake");

		if (flashlightShake != null)
		{
			flashlightShake.Play();
		}

		yield return new WaitForSeconds(1.5f);

		currentLightIntensity = maxLightIntensity;
		flashlight.intensity = currentLightIntensity;

		currentLightRange = maxLightRange;
		flashlight.range = currentLightRange;

		flashlight.enabled = true;
		SoundManager.Instance.PlaySound("Flashlight");

		isRecharging = false;
	}

	public void Die()
	{
		if (isDead)
			return;

		isDead = true;

		if (characterController != null)
			characterController.enabled = false;

		if (flashlight != null)
			flashlight.enabled = false;

		footsteps.Stop();
		Debug.Log("player died");
	}
}
