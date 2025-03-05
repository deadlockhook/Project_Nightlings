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
	public bool enableStatic = true;

	private VideoPlayer videoPlayer;
	private int currentVideoIndex = 0;
	private bool isPlayingStatic = false;
	private bool isTransitioning = false;

	private void Awake()
	{
		videoPlayer = GetComponent<VideoPlayer>();
		if (videoPlayer == null)
			videoPlayer = gameObject.AddComponent<VideoPlayer>();

		videoPlayer.playOnAwake = false;
		videoPlayer.isLooping = false;
		videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
			if (audioSource == null)
				audioSource = gameObject.AddComponent<AudioSource>();
		}
		videoPlayer.SetTargetAudioSource(0, audioSource);

		if (videoPlayer.targetMaterialRenderer == null)
		{
			Renderer renderer = GetComponent<Renderer>();
			if (renderer != null)
			{
				videoPlayer.targetMaterialRenderer = renderer;
				videoPlayer.targetMaterialProperty = "_MainTex";
			}
		}
	}

	private void Start()
	{
		videoPlayer.loopPointReached += OnVideoFinished;

		if (playlist != null && playlist.videos.Count > 0 && playOnStart)
			StartVideoSequence();
	}

	private void OnVideoFinished(VideoPlayer source)
	{
		if (isTransitioning)
			return;

		if (enableStatic)
		{
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
		else
		{
			PlayNextContentVideo();
		}
	}

	private void PlayClip(VideoClip clip, float volume, bool playStatic = false)
	{
		videoPlayer.Stop();
		videoPlayer.clip = clip;
		audioSource.volume = volume;
		videoPlayer.SetTargetAudioSource(0, audioSource);
		videoPlayer.enabled = true;
		audioSource.enabled = true;

		if (playStatic)
			videoPlayer.time = 0;

		videoPlayer.Play();
		isPlayingStatic = playStatic;
	}

	private void PlayVideo(int index)
	{
		if (playlist == null || index < 0 || index >= playlist.videos.Count)
			return;

		currentVideoIndex = index;
		var entry = playlist.videos[currentVideoIndex];
		PlayClip(entry.videoClip, entry.volume, false);
	}

	private void PlayStatic()
	{
		if (playlist == null || playlist.staticVideo == null)
		{
			PlayNextContentVideo();
			return;
		}

		isTransitioning = true;
		PlayClip(playlist.staticVideo, playlist.staticVolume, true);

		if (playlist.staticVideo.length > playlist.staticDuration)
			StartCoroutine(WaitAndPlayNext(playlist.staticDuration));
		else
			isTransitioning = false;
	}

	private IEnumerator WaitAndPlayNext(float delay)
	{
		yield return new WaitForSeconds(delay);
		videoPlayer.Stop();
		yield return new WaitForSeconds(0.1f);
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
			videoPlayer.Pause();
		else
			videoPlayer.Play();
	}
}
