using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ControlHintManager : MonoBehaviour {
	public static ControlHintManager Instance;

	[Header("UI Settings")]
	public CanvasGroup controlHintCanvasGroup;
	public TextMeshProUGUI controlHintText;

	[Header("Settings")]
	public float fadeDuration = 0.5f;
	public float displayDuration = 10f;

	[Header("Control Hints Data")]
	public ControlHintData controlHintData;

	private int currentHintIndex = 0;
	private bool hintsShown = false;

	[Header("Stamina Bar")]
	public Image staminaBar;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy(gameObject);
		}

		if (controlHintCanvasGroup != null) {
			controlHintCanvasGroup.alpha = 0f;
		}
	}

	public void ShowControlHints() {
		if (hintsShown)
			return;
		hintsShown = true;
		StartCoroutine(RunHints());
	}

	private IEnumerator RunHints() {
		if (controlHintData == null || controlHintData.hints.Count == 0)
			yield break;

		for (currentHintIndex = 0; currentHintIndex < controlHintData.hints.Count; currentHintIndex++) {
			string hint = controlHintData.hints[currentHintIndex];
			controlHintText.text = hint;

			float timer = 0f;
			while (timer < fadeDuration) {
				timer += Time.deltaTime;
				controlHintCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
				yield return null;
			}
			controlHintCanvasGroup.alpha = 1f;

			// Try making the stamina wheel pulse to show players unless they are blind
			bool pulseStamina = (currentHintIndex == 3 && staminaBar != null);
			float displayTimer = 0f;
			while (displayTimer < displayDuration) {
				displayTimer += Time.deltaTime;
				if (pulseStamina) {
					float pulse = (Mathf.Sin(Time.time * 10f) + 1f) / 2f;
					staminaBar.color = Color.Lerp(Color.white, Color.blue, pulse);
				}
				yield return null;
			}

			timer = 0f;
			while (timer < fadeDuration) {
				timer += Time.deltaTime;
				controlHintCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
				yield return null;
			}
			controlHintCanvasGroup.alpha = 0f;

			if (pulseStamina) {
				staminaBar.color = Color.white;
			}
		}
	}
}
