using System.Collections.Generic;
using UnityEngine;

public enum HintType {
	Window,
	PetDoor,
	BasementHatch,
	Fireplace,
	Skylight,
	Toilet,
	CandyBar
}

[System.Serializable]
public class ActivityHint {
	public HintType hintType;
	public List<string> hints;
}

[CreateAssetMenu(fileName = "GameHintData", menuName = "Game/Hints")]
public class GameHintData : ScriptableObject {
	public List<ActivityHint> activityHints;

	public string GetRandomHint(HintType type) {
		ActivityHint ah = activityHints.Find(x => x.hintType == type);
		if (ah != null && ah.hints.Count > 0) {
			return ah.hints[Random.Range(0, ah.hints.Count)];
		}
		return "";
	}
}
