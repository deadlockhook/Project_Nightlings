using UnityEngine;
using UnityEngine.UI;

public class IconFill : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void Fill(float fillAmount)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(fillAmount);
    }
}
