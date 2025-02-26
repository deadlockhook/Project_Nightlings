using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
	public static ProgressManager Instance;

	[System.Serializable]
	public class GameProgress
	{
		public bool night1Completed = false;
		public bool night2Completed = false;
		public bool night3Completed = false;
	}

	private GameProgress progress;

	private const string PROGRESS_KEY = "GameProgress";
	private const string NIGHT1_KEY = "Night1Completed";
	private const string NIGHT2_KEY = "Night2Completed";
	private const string NIGHT3_KEY = "Night3Completed";

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		LoadProgress();
	}

	private void LoadProgress()
	{
		progress = new GameProgress();

		progress.night1Completed = PlayerPrefs.GetInt(NIGHT1_KEY, 0) == 1;
		progress.night2Completed = PlayerPrefs.GetInt(NIGHT2_KEY, 0) == 1;
		progress.night3Completed = PlayerPrefs.GetInt(NIGHT3_KEY, 0) == 1;
	}

	private void SaveProgress()
	{
		PlayerPrefs.SetInt(NIGHT1_KEY, progress.night1Completed ? 1 : 0);
		PlayerPrefs.SetInt(NIGHT2_KEY, progress.night2Completed ? 1 : 0);
		PlayerPrefs.SetInt(NIGHT3_KEY, progress.night3Completed ? 1 : 0);
		PlayerPrefs.Save();
	}

	public void CompleteNight(int nightIndex)
	{
		switch (nightIndex)
		{
			case 0:
				progress.night1Completed = true;
				break;
			case 1:
				progress.night2Completed = true;
				break;
			case 2:
				progress.night3Completed = true;
				break;
		}
		SaveProgress();
	}

	public bool IsNightUnlocked(int nightIndex)
	{
		switch (nightIndex)
		{
			case 0:
				return true;
			case 1:
				return progress.night1Completed;
			case 2:
				return progress.night2Completed;
			default:
				return false;
		}
	}

	public void ResetProgress()
	{
		progress.night1Completed = false;
		progress.night2Completed = false;
		progress.night3Completed = false;
		SaveProgress();
	}
}
