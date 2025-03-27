using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITextGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private TMP_Text uiText;
    private Material textMaterial;
    private Material textMaterialOriginal;
    private Button button;

    void Start()
    {
        uiText = transform.Find("Text").GetComponent<TMP_Text>();
        textMaterialOriginal = uiText.fontMaterial;
        textMaterial = Instantiate(textMaterialOriginal);
        uiText.fontMaterial = textMaterial;
        button = GetComponent<Button>();
        DisableGlow();
    }
    public void EnableGlow(Color glowColor, float glowPower = 0.5f)
    {
        textMaterial.EnableKeyword("GLOW_ON");
        textMaterial.SetColor(ShaderUtilities.ID_GlowColor, textMaterialOriginal.GetColor(ShaderUtilities.ID_GlowColor));
        textMaterial.SetFloat(ShaderUtilities.ID_GlowPower, textMaterialOriginal.GetFloat(ShaderUtilities.ID_GlowPower));
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        EnableGlow(Color.cyan, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DisableGlow();
    }
    public void DisableGlow()
    {
        textMaterial.DisableKeyword("GLOW_ON");
        textMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0);
    }
}
