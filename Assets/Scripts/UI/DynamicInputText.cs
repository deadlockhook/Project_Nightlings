using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.LowLevel;

public class DynamicInputText : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private bool isUsingController;

    [TextArea]
    public string keyboardText = "Press {IMAGE} to jump";
    public string controllerText = "Press {IMAGE} to jump";

    public Sprite keyboardSprite;
    public Sprite controllerSprite;
    public Image displayImage;
    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        InputSystem.onEvent += HandleInputEvent;
        UpdateDisplayBasedOnCurrentDevice();
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= HandleInputEvent;
    }

    private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        var previousState = isUsingController;

        if (device is Gamepad)
        {
            isUsingController = true;
        }
        else if (device is Keyboard || device is Mouse)
        {
            isUsingController = false;
        }

        if (previousState != isUsingController)
        {
            UpdateText();
        }
    }

    private void UpdateDisplayBasedOnCurrentDevice()
    {
        isUsingController = Gamepad.current != null;
        UpdateText();
    }

    private void UpdateText()
    {
        string rawText = isUsingController ? controllerText : keyboardText;
        textComponent.text = rawText.Replace("{IMAGE}", "");

        if (displayImage != null)
        {
            displayImage.sprite = isUsingController ? controllerSprite : keyboardSprite;
        }
    }
}