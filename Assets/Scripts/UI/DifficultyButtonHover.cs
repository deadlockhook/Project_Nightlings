using UnityEngine;
using UnityEngine.EventSystems;

public class DifficultyButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField] private int difficultyIndex;

	public void OnPointerEnter(PointerEventData eventData)
	{
		UIManager.Instance.ShowDifficultyDescription(difficultyIndex);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UIManager.Instance.ClearDifficultyDescription();
	}

	public void OnSelect(BaseEventData eventData)
	{
		UIManager.Instance.ShowDifficultyDescription(difficultyIndex);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		UIManager.Instance.ClearDifficultyDescription();
	}
}
