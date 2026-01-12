using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoControllerScript : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Assign in Inspector
    public VideoClip videoClip1;    // First clip
    public VideoClip videoClip2;    // Second clip
    public Button playButton;       // Assign in Inspector

    private bool isFirstVideoPlaying = true;

    void Start()
    {
        playButton.onClick.AddListener(PlayFirstVideo);
        videoPlayer.loopPointReached += OnVideoFinished; // Listen for end of video
    }

    void PlayFirstVideo()
    {
        videoPlayer.clip = videoClip1;
        videoPlayer.Play();
        isFirstVideoPlaying = true;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (isFirstVideoPlaying)
        {
            // Play second video
            videoPlayer.clip = videoClip2;
            videoPlayer.Play();
            isFirstVideoPlaying = false;
        }
        else
        {
            // Disable VideoPlayer after second video
            videoPlayer.gameObject.SetActive(false);
        }
    }
}


