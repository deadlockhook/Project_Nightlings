using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
	public float staminaRechargeBoost = 2.5f;
	private float staminaCoolDownTimer = 0f;
	private float currentStamina;

	[Header("Stamina UI")]
	private Image staminaBar;
	private CanvasGroup staminaCanvasGroup;
	public float staminaFadeSpeed = 1f;

	[Header("Sugar Rush Settings")]
	public float sugarRushDuration = 15f;
	public float speedBoostMultiplier = 1.3f;
	private bool isSugarRushActive = false;
	private float sugarRushTimer = 0f;
	private float originalWalkSpeed;
	private float originalRunSpeed;

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
	public TextMeshProUGUI rechargeFlashlightText;

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
	private Vector3 lastSpawnPosition;

	private void Awake()
	{
		playerControlActions = new PlayerControlActions();
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
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), interactableLayer, true);

		footstepsGameObject = transform.Find("Footsteps").gameObject;
		footsteps = footstepsGameObject.GetComponent<AudioSource>();
		characterController = GetComponent<CharacterController>();
		interactionManager = FindObjectOfType<InteractionManager>();
		currentFOV = normalFOV;
		currentStamina = maxStamina;
		flashlightShake = flashlightGameObject.GetComponent<Animation>();
		cameraOriginalPosition = playerCamera.transform.localPosition;

		if (interactionManager != null)
		{
			interactionManager.OnLocalPlayerSetup(this, playerCamera);
		}

		if(rechargeFlashlightText != null)
			rechargeFlashlightText.gameObject.SetActive(false);

		if (flashlight != null)
		{
			currentLightIntensity = maxLightIntensity;
			flashlight.intensity = currentLightIntensity;
			currentLightRange = maxLightRange;
			flashlight.range = currentLightRange;
			rangeDrainRate = maxLightIntensity != minLightIntensity ? drainRate * (maxLightRange - minLightRange) / (maxLightIntensity - minLightIntensity) : 0f;
			flashlightEnabled = false;
			flashlight.enabled = false;
		}

		lastSpawnPosition = transform.localPosition;
	}

	private void Update()
	{
		if (isDead)
			return;

		ProcessFlashlightToggle();
		RetrieveInput();
		HandleMovement();
		HandleLook();
		UpdateFOV();
		HandleHeadBob();
		HandleFlashlight();
		UpdateStamina();
		UpdateSugarRush();
		UpdateStaminaUI();

		if (interactionManager != null)
			interactionManager.OnLocalPlayerUpdate();

		if (rechargeFlashlightText != null)
		{
			if (currentLightIntensity < 0.5f && !isRecharging)
			{
				rechargeFlashlightText.text = "Press RMB to recharge flashlight!";
				rechargeFlashlightText.gameObject.SetActive(true);
			}
			else
			{
				rechargeFlashlightText.gameObject.SetActive(false);
			}
		}
	}

	// flashlight toggle on/off
	private void ProcessFlashlightToggle()
	{
		var controls = playerControlActions.Player;
		if (controls.FlashlightToggle.triggered && !isRecharging)
		{
			SoundManager.Instance.PlaySound("Flashlight");
			flashlightEnabled = !flashlightEnabled;
			if (flashlight != null)
				flashlight.enabled = flashlightEnabled;
		}
	}

	public void EnableFlashlight()
	{
		flashlightEnabled = true;
		if (flashlight != null)
			flashlight.enabled = true;
	}

	// Retrieve player input
	private void RetrieveInput()
	{
		movementInput = playerControlActions.Player.Move.ReadValue<Vector2>();
		lookInput = playerControlActions.Player.Look.ReadValue<Vector2>();
	}

	// Update stamina
	private void UpdateStamina()
	{
		if (isSugarRushActive)
		{
			currentStamina = maxStamina;
			staminaCoolDownTimer = 0f;
			return;
		}

		if (isRunning)
		{
			staminaCoolDownTimer = 0f;
		}
		else
		{
			staminaCoolDownTimer += Time.deltaTime;
			if (staminaCoolDownTimer >= staminaCoolDown && currentStamina < maxStamina)
			{
				float rechargeRate = staminaRechargeRate;
				if (movementInput.sqrMagnitude < 0.01f)
					rechargeRate *= staminaRechargeBoost;
				currentStamina = Mathf.Clamp(currentStamina + rechargeRate * Time.deltaTime, 0f, maxStamina);
			}
		}
	}

	// Update stamina UI
	private void UpdateStaminaUI()
	{
		if (staminaBar == null)
		{
			GameObject barObject = GameObject.Find("StaminaBar");
			if (barObject != null)
				staminaBar = barObject.GetComponent<Image>();
			else
				staminaBar = FindObjectsOfType<Image>(true).FirstOrDefault(img => img.gameObject.name == "StaminaBar");
		}

		if (staminaCanvasGroup == null)
		{
			GameObject panelObject = GameObject.Find("StaminaPanel");
			if (panelObject != null)
				staminaCanvasGroup = panelObject.GetComponent<CanvasGroup>();
			else
				staminaCanvasGroup = FindObjectsOfType<CanvasGroup>(true).FirstOrDefault(cg => cg.gameObject.name == "StaminaPanel");
		}

		if (staminaBar == null || staminaCanvasGroup == null)
			return;

		float ratio = currentStamina / maxStamina;
		staminaBar.fillAmount = ratio;

		if (ratio < 0.25f)
			staminaBar.color = Color.red;
		else if (ratio < 0.5f)
			staminaBar.color = Color.yellow;
		else
			staminaBar.color = Color.white;

		float targetAlpha = (currentStamina >= maxStamina) ? 0f : 1f;
		staminaCanvasGroup.alpha = Mathf.Lerp(staminaCanvasGroup.alpha, targetAlpha, Time.deltaTime * staminaFadeSpeed);
	}

	// Get player input direction
	private Vector3 GetInputDirection()
	{
		Vector3 dir = transform.right * movementInput.x + transform.forward * movementInput.y;
		if (dir.sqrMagnitude > 1f)
			dir.Normalize();
		return dir;
	}

	// Handle player sprinting
	private void ProcessSprint(Vector3 inputDir)
	{
		if (!isRecharging && playerControlActions.Player.Sprint.IsPressed() && currentStamina > 0 && inputDir.magnitude > 0)
		{
			isRunning = true;
			isWalking = false;
			currentStamina = Mathf.Max(currentStamina - staminaDrainRate * Time.deltaTime, 0f);
		}
		else
		{
			isRunning = false;
			isWalking = true;
		}
	}

	// Get target speed based on player input
	private float GetTargetSpeed(Vector3 inputDir)
	{
		float walk = walkSpeed;
		float run = runSpeed;
		if (isRecharging)
		{
			walk *= 0.75f;
			run = walk;
		}
		return inputDir.magnitude == 0 ? 0f : (isRunning ? run : walk);
	}

	// Handle player jump
	private void ProcessJump()
	{
		if (characterController.isGrounded)
		{
			if (playerControlActions.Player.Jump.IsPressed())
			{
				verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
				footsteps.Stop();
			}
		}
		else
		{
			verticalVelocity += gravity * Time.deltaTime;
		}
	}

	// Handle player footsteps sounds
	private void ProcessFootsteps(Vector3 inputDir)
	{
		if (characterController.isGrounded && inputDir.magnitude > 0f)
		{
			if (isRunning && currentStamina > 0)
				SoundManager.Instance.PlaySound("PlayerRun", footsteps);
			else if (isWalking)
				SoundManager.Instance.PlaySound("PlayerWalk", footsteps);
			else
				footsteps.Stop();
		}
		else
		{
			footsteps.Stop();
		}
	}

	// Handle player movement
	private void HandleMovement()
	{
		Vector3 inputDir = GetInputDirection();
		ProcessSprint(inputDir);
		float targetSpeed = GetTargetSpeed(inputDir);
		currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);
		moveDirection = inputDir * currentSpeed;
		ProcessJump();
		moveDirection.y = verticalVelocity;
		characterController.Move(moveDirection * Time.deltaTime);
		ProcessFootsteps(inputDir);
	}

	// Handle player looking around
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

	// Handle FOV when sprinting
	private void UpdateFOV()
	{
		float targetFOV = isRunning ? sprintFOV : normalFOV;
		currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * 5f);
		playerCamera.fieldOfView = currentFOV;
	}

	// Handle head bobbing when moving
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

	// Handle flashlight drain over time
	private void HandleFlashlight()
	{
		if (flashlight == null || !flashlightEnabled)
			return;
		if (!isRecharging)
		{
			currentLightIntensity = Mathf.Clamp(currentLightIntensity - drainRate * Time.deltaTime, minLightIntensity, maxLightIntensity);
			flashlight.intensity = currentLightIntensity;
			currentLightRange = Mathf.Clamp(currentLightRange - rangeDrainRate * Time.deltaTime, minLightRange, maxLightRange);
			flashlight.range = currentLightRange;
		}
		if (playerControlActions.Player.FlashlightRecharge.triggered && !isRecharging)
			StartCoroutine(RechargeFlashlight());
	}

	// Handle flashlight recharge
	private IEnumerator RechargeFlashlight()
	{
		isRecharging = true;
		flashlight.enabled = false;
		SoundManager.Instance.PlaySound("Flashlight");
		SoundManager.Instance.PlaySound("FlashlightShake");
		if (flashlightShake != null)
			flashlightShake.Play();
		yield return new WaitForSeconds(1.5f);
		currentLightIntensity = maxLightIntensity;
		flashlight.intensity = currentLightIntensity;
		currentLightRange = maxLightRange;
		flashlight.range = currentLightRange;
		flashlight.enabled = true;
		SoundManager.Instance.PlaySound("Flashlight");
		isRecharging = false;
	}

	// Handle player death
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

	// Handle player respawn
	public void Respawn()
	{
		transform.localPosition = lastSpawnPosition;
	}

	public bool EatCandy()
	{
		if (isSugarRushActive)
			return false;

		originalWalkSpeed = walkSpeed;
		originalRunSpeed = runSpeed;
		isSugarRushActive = true;
		sugarRushTimer = 0f;
		walkSpeed *= speedBoostMultiplier;
		runSpeed *= speedBoostMultiplier;
		currentStamina = maxStamina;
		return true;
	}

	private void UpdateSugarRush()
	{
		if (isSugarRushActive)
		{
			sugarRushTimer += Time.deltaTime;

			if (sugarRushTimer >= sugarRushDuration)
			{
				walkSpeed = originalWalkSpeed;
				runSpeed = originalRunSpeed;

				isSugarRushActive = false;
			}
		}
	}
}
