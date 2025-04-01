using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITextGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private TMP_Text uiText;
    private Material textMaterial;
    private Material textMaterialOriginal;
    private Button button;
    private Toggle toggle;
    private TMP_Dropdown dropdown;
    private Slider slider;

    void OnEnable()
    {
        uiText = transform.Find("Text")?.GetComponent<TMP_Text>();

        if (uiText == null)
        {
            Debug.LogError("UITextGlow: No TMP_Text child named 'Text' found!", this);
            enabled = false;
            return;
        }

        textMaterialOriginal = uiText.fontMaterial;
        textMaterial = Instantiate(textMaterialOriginal);
        uiText.fontMaterial = textMaterial;

        button = GetComponent<Button>();
        toggle = GetComponent<Toggle>();
        dropdown = GetComponent<TMP_Dropdown>();
        slider = GetComponent<Slider>();

        DisableGlow();

        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EnableGlow(Color.red, 0.5f);
        }

        if (toggle != null) toggle.onValueChanged.AddListener(OnToggleValueChanged);
        if (dropdown != null) dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        if (slider != null) slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnDisable()
    {
        if (toggle != null) toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        if (dropdown != null) dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        if (slider != null) slider.onValueChanged.RemoveListener(OnSliderValueChanged);

        if (uiText != null && textMaterialOriginal != null)
        {
            uiText.fontMaterial = textMaterialOriginal;
        }
    }

    public void EnableGlow(Color glowColor, float glowPower = 0.5f)
    {
        textMaterial.EnableKeyword("GLOW_ON");
        textMaterial.SetColor(ShaderUtilities.ID_GlowColor, glowColor);
        textMaterial.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);
    }

    public void DisableGlow()
    {
        textMaterial.DisableKeyword("GLOW_ON");
        textMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EnableGlow(Color.red, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DisableGlow();
    }

    public void OnSelect(BaseEventData eventData)
    {
        EnableGlow(Color.red, 0.5f);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DisableGlow();
    }

    public void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            EnableGlow(Color.red, 0.5f);
        }
        else
        {
            DisableGlow();
        }
    }

    public void OnDropdownValueChanged(int value)
    {
        EnableGlow(Color.red, 0.5f);
    }

    public void OnSliderValueChanged(float value)
    {
        EnableGlow(Color.red, 0.5f);
    }
}
