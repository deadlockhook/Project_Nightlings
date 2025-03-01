using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TVVideoPlayer : MonoBehaviour
{
	[Header("Playlist Settings")]
	public TVPlaylist playlist;

	public bool playOnStart = true;

	[Header("Other Settings")]
	public AudioSource audioSource;

	private VideoPlayer videoPlayer;
	private int currentVideoIndex = 0;
	private bool isPlayingStatic = false;
	private bool isTransitioning = false;

	private void Awake()
	{
		videoPlayer = GetComponent<VideoPlayer>();
		if (videoPlayer == null)
		{
			videoPlayer = gameObject.AddComponent<VideoPlayer>();
		}

		videoPlayer.playOnAwake = false;
		videoPlayer.isLooping = false;
		videoPlayer.renderMode = VideoRenderMode.MaterialOverride;

		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
			}
		}

		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		videoPlayer.SetTargetAudioSource(0, audioSource);

		if (videoPlayer.targetMaterialRenderer == null)
		{
			Renderer renderer = GetComponent<Renderer>();
			if (renderer != null)
			{
				videoPlayer.targetMaterialRenderer = renderer;
				videoPlayer.targetMaterialProperty = "_MainTex";
			}
			else
			{
				Debug.LogError("nope");
			}
		}
	}

	private void Start()
	{
		videoPlayer.loopPointReached += OnVideoFinished;

		if (playlist == null)
		{
			return;
		}

		if (playlist.videos.Count == 0)
		{
			return;
		}

		if (playOnStart)
		{
			StartVideoSequence();
		}
	}

	private void OnVideoFinished(VideoPlayer source)
	{
		if (isTransitioning)
			return;

		if (isPlayingStatic)
		{
			isPlayingStatic = false;
			PlayNextContentVideo();
		}
		else
		{
			PlayStatic();
		}
	}

	private void PlayVideo(int index)
	{
		if (playlist == null || index < 0 || index >= playlist.videos.Count)
			return;

		currentVideoIndex = index;
		TVPlaylist.VideoEntry entry = playlist.videos[currentVideoIndex];

		videoPlayer.clip = entry.videoClip;
		audioSource.volume = entry.volume;

		videoPlayer.Play();
		isPlayingStatic = false;
	}

	private void PlayStatic()
	{
		if (playlist == null || playlist.staticVideo == null)
		{
			PlayNextContentVideo();
			return;
		}

		isPlayingStatic = true;
		isTransitioning = true;

		videoPlayer.clip = playlist.staticVideo;
		audioSource.volume = playlist.staticVolume;

		videoPlayer.Play();

		if (playlist.staticVideo.length > playlist.staticDuration)
		{
			StartCoroutine(WaitAndPlayNext(playlist.staticDuration));
		}
		else
		{
			isTransitioning = false;
		}
	}

	private IEnumerator WaitAndPlayNext(float delay)
	{
		yield return new WaitForSeconds(delay);
		isTransitioning = false;
		PlayNextContentVideo();
	}

	private void PlayNextContentVideo()
	{
		if (playlist == null || playlist.videos.Count == 0)
			return;

		currentVideoIndex = (currentVideoIndex + 1) % playlist.videos.Count;
		PlayVideo(currentVideoIndex);
	}

	public void StartVideoSequence()
	{
		if (playlist == null || playlist.videos.Count == 0)
			return;

		StopVideoSequence();
		currentVideoIndex = 0;
		PlayVideo(currentVideoIndex);
	}

	public void StopVideoSequence()
	{
		StopAllCoroutines();
		isTransitioning = false;
		isPlayingStatic = false;
		videoPlayer.Stop();
	}

	public void SkipToNextVideo()
	{
		if (isPlayingStatic)
		{
			StopAllCoroutines();
			isTransitioning = false;
			PlayNextContentVideo();
		}
		else
		{
			PlayStatic();
		}
	}

	public void SetPlaylist(TVPlaylist newPlaylist)
	{
		playlist = newPlaylist;
		StartVideoSequence();
	}

	public void TogglePause()
	{
		if (videoPlayer.isPlaying)
		{
			videoPlayer.Pause();
		}
		else
		{
			videoPlayer.Play();
		}
	}
}
