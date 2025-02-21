using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HintManager : MonoBehaviour
{
	public static HintManager Instance;

	[Header("UI Settings")]
	public CanvasGroup hintCanvasGroup;
	public TextMeshProUGUI hintText;

	[Header("Settings")]
	public float fadeDuration = 0.5f;
	public float displayDuration = 5f;

	[Header("Hint Settings")]
	public GameHintData hintData;

	private HashSet<HintType> shownHints = new HashSet<HintType>();
	private bool isHintActive = false;

	private void Awake()
	{
		if(Instance == null) {
			Instance = this;
		} else {
			Destroy(gameObject);
		}
		if (hintCanvasGroup != null)
			hintCanvasGroup.alpha = 0f;
	}

	public void DisplayGameHint(HintType type)
	{
		if(isHintActive || shownHints.Contains(type))
			return;

		string message = hintData.GetRandomHint(type);
		if(string.IsNullOrEmpty(message))
			return;

		StartCoroutine(ShowHint(message, type));
	}

	private IEnumerator ShowHint(string message, HintType type)
	{
		isHintActive = true;
		hintText.text = message;

		float timer = 0f;
		while(timer < fadeDuration) {
			timer += Time.deltaTime;
			hintCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
			yield return null;
		}
		hintCanvasGroup.alpha = 1f;

		yield return new WaitForSeconds(displayDuration);

		timer = 0f;
		while(timer < fadeDuration) {
			timer += Time.deltaTime;
			hintCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
			yield return null;
		}
		hintCanvasGroup.alpha = 0f;

		isHintActive = false;
		shownHints.Add(type);
	}
}
