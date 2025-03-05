using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ControlHintData", menuName = "Game/ControlHints", order = 1)]
public class ControlHintData : ScriptableObject {
	public List<string> hints;
}
