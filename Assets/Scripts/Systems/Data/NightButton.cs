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
				nightText.color = isUnlocked ? Color.green : new Color(1f, 0f, 0f, 0.75f);
		}
	}
}
