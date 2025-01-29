using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Handles the player movement and interaction

public class PlayerController : MonoBehaviour
{
	[Header("Camera")]
	public Camera playerCamera;

	[Header("Movement Settings")]
	public float walkSpeed = 4f;
	public float runSpeed = 6f;
	public float jumpHeight = 1f;
	public float lookSensitivity = 2f;

	[Header("FOV Settings")]
	public float normalFOV = 75f;
	public float sprintFOV = 80f;
	private float currentFOV;

	[Header("Stamina Settings")]
	public float maxStamina = 100f;
	public float staminaDrainRate = 1f;
	private float currentStamina;

	[Header("UI Settings")]
	public TextMeshProUGUI staminaText;

	[Header("Head Bob Settings")]
	public float headBobPower = 0.05f;
	public float headBobSpeed = 5f;

	private CharacterController characterController;
	private Vector3 moveDirection = Vector3.zero;

	private float gravity = -9.81f;
	private float verticalVelocity = 0f;
	private float currentSpeed = 0f;
	private float verticalRotation = 0f;
	private bool isRunning = false;

	private float headBobTimer = 0f;
	private Vector3 cameraOriginalPosition;

	private InteractionManager interactionManager;

	private void Start()
	{
		characterController = GetComponent<CharacterController>();
		interactionManager = FindObjectOfType<InteractionManager>();
		currentFOV = normalFOV;
		currentStamina = maxStamina;

		cameraOriginalPosition = playerCamera.transform.localPosition;

		if (interactionManager != null)
			interactionManager.OnLocalPlayerSetup(this, playerCamera);
	}

	private void Update()
	{
		HandleMovement();
		HandleLook();
		UpdateFOV();
		UpdateStaminaUI();
		HandleHeadBob();

		if (interactionManager != null)
			interactionManager.OnLocalPlayerUpdate();
	}

	private void HandleMovement()
	{
		float moveX = Input.GetAxis("Horizontal");
		float moveZ = Input.GetAxis("Vertical");
		Vector3 inputDirection = transform.right * moveX + transform.forward * moveZ;

		if (inputDirection.magnitude > 1f)
		{
			inputDirection.Normalize();
		}

		if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && inputDirection.magnitude > 0)
		{
			isRunning = true;
			currentStamina -= staminaDrainRate * Time.deltaTime;
			if (currentStamina < 0) currentStamina = 0;
		}
		else
		{
			isRunning = false;
		}

		float targetSpeed = isRunning ? runSpeed : walkSpeed;
		if (inputDirection.magnitude == 0) targetSpeed = 0;

		currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);

		moveDirection = inputDirection * currentSpeed;

		if (characterController.isGrounded)
		{
			if (Input.GetButtonDown("Jump"))
			{
				verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
			}
		}
		else
		{
			verticalVelocity += gravity * Time.deltaTime;
		}

		moveDirection.y = verticalVelocity;
		characterController.Move(moveDirection * Time.deltaTime);
	}

	private void HandleLook()
	{
		float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

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

	private void UpdateStaminaUI()
	{
		if (staminaText)
		staminaText.text = $"Stamina: {Mathf.CeilToInt(currentStamina)}";
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
}
