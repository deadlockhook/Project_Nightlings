using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ControlHintManager : MonoBehaviour
{
    public static ControlHintManager Instance;

    [Header("UI Settings")]
    public CanvasGroup controlHintCanvasGroup;
    public TextMeshProUGUI controlHintText;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public float displayDuration = 10f;

    [Header("Control Hints Data")]
    public ControlHintData controlHintData;
    public Image controlHintImage;

    [Header("Stamina Bar")]
    public Image staminaBar;

    [Header("Pulse Settings")]
    public float initialPulseTime = 0.3f;
    public float sizeChange = 1.2f;
    public float colorPulseTime = 2f;
    public float colorPulseLerp = 0.2f;
    public float darkMul = 0.9f;
    public string hintSound = "HintAppear";
    public Color targetOrange = new Color(1f, 0.5f, 0f);

    private int currentHintIndex = 0;
    private bool hintsShown = false;
    private bool isController = false;
    private bool wasControllerPreviously = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (controlHintCanvasGroup != null)
        {
            controlHintCanvasGroup.alpha = 0f;
        }
    }

    public void ShowControlHints()
    {
        if (hintsShown)
            return;
        hintsShown = true;
        StartCoroutine(RunHints());
    }

    private IEnumerator RunHints()
    {
        if (controlHintData == null || controlHintData.hints.Count == 0)
            yield break;

        for (currentHintIndex = 0; currentHintIndex < controlHintData.hints.Count; currentHintIndex++)
        {
            ControlHint currentHint = controlHintData.hints[currentHintIndex];

            controlHintText.text = currentHint.hintText + "\n" + currentHint.additionalInfo;

            UpdateInputDevice();
            UpdateHintSprite(currentHint);

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                controlHintCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            controlHintCanvasGroup.alpha = 1f;

            Vector3 originalScale = controlHintText.transform.localScale;
            controlHintText.color = Color.white;
            controlHintText.transform.localScale = originalScale;

            SoundManager.Instance.PlaySound(hintSound);

            float halfPulseDuration = initialPulseTime / 2f;
            float pulseTimer = 0f;
            while (pulseTimer < initialPulseTime)
            {
                pulseTimer += Time.deltaTime;
                float colorT = Mathf.Clamp01(pulseTimer / initialPulseTime);
                controlHintText.color = Color.Lerp(Color.white, targetOrange, colorT);

                float scaleMultiplier = pulseTimer < halfPulseDuration
                    ? Mathf.Lerp(1f, sizeChange, pulseTimer / halfPulseDuration)
                    : Mathf.Lerp(sizeChange, 1f, (pulseTimer - halfPulseDuration) / halfPulseDuration);

                controlHintText.transform.localScale = originalScale * scaleMultiplier;
                yield return null;
            }
            controlHintText.color = targetOrange;
            controlHintText.transform.localScale = originalScale;

            float displayTimer = 0f;
            while (displayTimer < displayDuration)
            {
                displayTimer += Time.deltaTime;
                float pulse = (Mathf.Sin(Time.time * colorPulseTime) + 1f) / 2f;
                Color darkerOrange = targetOrange * darkMul;
                Color lighterOrange = Color.Lerp(targetOrange, Color.white, colorPulseLerp);
                controlHintText.color = Color.Lerp(darkerOrange, lighterOrange, pulse);

                bool previousIsController = isController;
                UpdateInputDevice();

                if (isController != previousIsController)
                {
                    UpdateHintSprite(currentHint);
                }

                if (currentHintIndex == 3 && staminaBar != null)
                {
                    float staminaPulse = (Mathf.Sin(Time.time * 10f) + 1f) / 2f;
                    staminaBar.color = Color.Lerp(Color.white, Color.blue, staminaPulse);
                }

                yield return null;
            }

            timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                controlHintCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            controlHintCanvasGroup.alpha = 0f;

            if (currentHintIndex == 3 && staminaBar != null)
            {
                staminaBar.color = Color.white;
            }
        }
    }

    private bool CheckForControllerInput()
    {
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + i)))
            {
                return true;
            }
        }

        float leftStickX = Input.GetAxisRaw("Horizontal");
        float leftStickY = Input.GetAxisRaw("Vertical");
        float rightStickX = Input.GetAxisRaw("Look X");
        float rightStickY = Input.GetAxisRaw("Look Y");

        return Mathf.Abs(leftStickX) > 0.2f ||
               Mathf.Abs(leftStickY) > 0.2f ||
               Mathf.Abs(rightStickX) > 0.2f ||
               Mathf.Abs(rightStickY) > 0.2f;
    }

    private bool CheckForKeyboardInput()
    {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 ||
        Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            return true;
        }

        if (Input.anyKey)
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(keyCode))
                {
                    if (keyCode >= KeyCode.JoystickButton0) continue;
                    return true;
                }
            }
        }
        return false;

    }

    private void UpdateInputDevice()
    {
        bool controllerInput = CheckForControllerInput();
        bool keyboardInput = CheckForKeyboardInput();

        if (controllerInput) isController = true;
        else if (keyboardInput) isController = false;
    }

    private void UpdateHintSprite(ControlHint currentHint)
    {
        Sprite newSprite = isController ? currentHint.controllerSprite : currentHint.keyboardMouseSprite;
        if (newSprite != null && controlHintImage.sprite != newSprite)
        {
            controlHintImage.sprite = newSprite;
        }
    }

    public void ResetControlHints()
    {
        if (controlHintCanvasGroup == null) return;

        StopAllCoroutines();
        hintsShown = false;
        controlHintCanvasGroup.alpha = 0f;
    }
}