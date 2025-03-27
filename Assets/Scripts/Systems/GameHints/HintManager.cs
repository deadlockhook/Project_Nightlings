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

	private Queue<HintQueueEntry> hintQueue = new Queue<HintQueueEntry>();

	private class HintQueueEntry
	{
		public HintType type;
		public string message;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
		if (hintCanvasGroup != null)
			hintCanvasGroup.alpha = 0f;
	}

	public void DisplayGameHint(HintType type)
	{
		if (shownHints.Contains(type))
			return;

		string message = hintData.GetRandomHint(type);
		if (string.IsNullOrEmpty(message))
			return;

		hintQueue.Enqueue(new HintQueueEntry { type = type, message = message });

		if (!isHintActive)
			StartCoroutine(ProcessQueue());
	}

	private IEnumerator ProcessQueue()
	{
		isHintActive = true;

		while (hintQueue.Count > 0)
		{
			HintQueueEntry entry = hintQueue.Dequeue();
			yield return StartCoroutine(ShowHint(entry.message, entry.type));
		}
		isHintActive = false;
	}

	private IEnumerator ShowHint(string message, HintType type)
	{
		hintText.text = message;

		float timer = 0f;
		while (timer < fadeDuration)
		{
			timer += Time.deltaTime;
			hintCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
			yield return null;
		}
		hintCanvasGroup.alpha = 1f;

		yield return new WaitForSeconds(displayDuration);

		timer = 0f;
		while (timer < fadeDuration)
		{
			timer += Time.deltaTime;
			hintCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
			yield return null;
		}
		hintCanvasGroup.alpha = 0f;

		shownHints.Add(type);
	}

	public void ResetGameHints() {
		StopAllCoroutines();
		hintQueue.Clear();
		isHintActive = false;
		shownHints.Clear();
		if (hintCanvasGroup != null)
			hintCanvasGroup.alpha = 0f;
	}
}
