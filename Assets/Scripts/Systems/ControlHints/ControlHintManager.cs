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

	// Very scuffed pulse effect, but it works I guess
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
			Vector3 originalScale = controlHintText.transform.localScale;
			controlHintText.color = Color.white;
			controlHintText.transform.localScale = originalScale;
			SoundManager.Instance.PlaySound(hintSound);
			float halfPulseDuration = initialPulseTime / 2f;
			float pulseTimer = 0f;
			while (pulseTimer < initialPulseTime) {
				pulseTimer += Time.deltaTime;
				float colorT = Mathf.Clamp01(pulseTimer / initialPulseTime);
				controlHintText.color = Color.Lerp(Color.white, targetOrange, colorT);
				float scaleMultiplier = pulseTimer < halfPulseDuration ? Mathf.Lerp(1f, sizeChange, pulseTimer / halfPulseDuration) : Mathf.Lerp(sizeChange, 1f, (pulseTimer - halfPulseDuration) / halfPulseDuration);
				controlHintText.transform.localScale = originalScale * scaleMultiplier;
				yield return null;
			}
			controlHintText.color = targetOrange;
			controlHintText.transform.localScale = originalScale;
			float displayTimer = 0f;
			while (displayTimer < displayDuration) {
				displayTimer += Time.deltaTime;
				float pulse = (Mathf.Sin(Time.time * colorPulseTime) + 1f) / 2f;
				Color darkerOrange = targetOrange * darkMul;
				Color lighterOrange = Color.Lerp(targetOrange, Color.white, colorPulseLerp);
				controlHintText.color = Color.Lerp(darkerOrange, lighterOrange, pulse);
				if (currentHintIndex == 3 && staminaBar != null) {
					float staminaPulse = (Mathf.Sin(Time.time * 10f) + 1f) / 2f;
					staminaBar.color = Color.Lerp(Color.white, Color.blue, staminaPulse);
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
			if (currentHintIndex == 3 && staminaBar != null) {
				staminaBar.color = Color.white;
			}
		}
	}

	public void ResetControlHints()
	{
		StopAllCoroutines();
		hintsShown = false;
		if (controlHintCanvasGroup != null)
			controlHintCanvasGroup.alpha = 0f;
	}
}
