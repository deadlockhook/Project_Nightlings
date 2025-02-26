using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New TV Playlist", menuName = "Game/TV Playlist", order = 1)]
public class TVPlaylist : ScriptableObject
{
	[System.Serializable]
	public class VideoEntry
	{
		public string title;
		public VideoClip videoClip;
		[Range(0, 1)]
		public float volume = 1.0f;
	}

	[Header("Playlist Settings")]
	public string playlistName;

	public string description;

	[Header("Videos")]
	public List<VideoEntry> videos = new List<VideoEntry>();

	[Header("Static Settings")]
	public VideoClip staticVideo;

	[Range(0.1f, 3.0f)]
	public float staticDuration = 0.5f;

	[Range(0, 1)]
	public float staticVolume = 0.5f;
}
