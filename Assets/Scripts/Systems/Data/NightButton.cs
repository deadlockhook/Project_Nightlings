using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NightButton : MonoBehaviour
{
	public int nightIndex;
	public TextMeshProUGUI nightText;
	public Button button;
	public GameObject lockIcon;

	private void OnEnable()
	{
		UpdateButtonState();
	}

	public void UpdateButtonState()
	{
		if (ProgressManager.Instance != null)
		{
			bool isUnlocked = ProgressManager.Instance.IsNightUnlocked(nightIndex);

			if (lockIcon != null)
				lockIcon.SetActive(!isUnlocked);

			button.interactable = isUnlocked;

			if (nightText != null)
				nightText.color = isUnlocked ? Color.blue : new Color(0.5f, 0.5f, 0.5f, 0.75f);
		}
	}
}
