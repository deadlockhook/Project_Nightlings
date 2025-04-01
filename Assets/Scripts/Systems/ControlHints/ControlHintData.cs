using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ControlHint
{
    public string hintText;
    public Sprite keyboardMouseSprite;
    public Sprite controllerSprite;
    public string additionalInfo;
}

[CreateAssetMenu(fileName = "ControlHintData", menuName = "Game/ControlHints", order = 1)]
public class ControlHintData : ScriptableObject
{
    public List<ControlHint> hints;

    public Sprite GetInputDeviceSprite(ControlHint hint)
    {
        if (IsUsingController())
        {
            return hint.controllerSprite;
        }
        else
        {
            return hint.keyboardMouseSprite;
        }
    }

    private bool IsUsingController()
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

        if (Mathf.Abs(leftStickX) > 0.2f ||
            Mathf.Abs(leftStickY) > 0.2f ||
            Mathf.Abs(rightStickX) > 0.2f ||
            Mathf.Abs(rightStickY) > 0.2f)
        {
            return true;
        }

        if (Input.GetMouseButton(0) ||
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.Space) ||
            Input.anyKeyDown)
        {
            return false;
        }

        return false;
    }
}
