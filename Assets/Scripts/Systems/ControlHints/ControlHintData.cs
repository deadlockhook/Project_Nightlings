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
        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump"))
        {
            return true;
        }

        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        {
            return false;
        }

        return false;
    }
}
