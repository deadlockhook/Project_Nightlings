using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DifficultyButtonPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField] private Graphic targetGraphic;
	[SerializeField] private float pulseSpeed = 2f;

	private Coroutine pulseRoutine;
	private Color originalColor;

	private void Awake()
	{
		if (targetGraphic == null)
			targetGraphic = GetComponent<Graphic>();
		if (targetGraphic != null)
			originalColor = targetGraphic.color;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		StartPulse();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StopPulse();
	}

	public void OnSelect(BaseEventData eventData)
	{
		StartPulse();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		StopPulse();
	}

	private void StartPulse()
	{
		if (pulseRoutine == null && targetGraphic != null)
			pulseRoutine = StartCoroutine(PulseCoroutine());
	}

	private void StopPulse()
	{
		if (pulseRoutine != null)
		{
			StopCoroutine(pulseRoutine);
			pulseRoutine = null;
			if (targetGraphic != null)
				targetGraphic.color = originalColor;
		}
	}

	private IEnumerator PulseCoroutine()
	{
		float t = 0f;
		while (true)
		{
			t += Time.unscaledDeltaTime * pulseSpeed * Mathf.PI * 2f;
			float factor = (Mathf.Sin(t) + 1f) * 0.5f;
			targetGraphic.color = Color.Lerp(originalColor, Color.white, factor);
			yield return null;
		}
	}
}
