using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PlayVideoSequence : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    private List<VideoClip> clipSequence;
    private Dictionary<string, VideoClip> videoClipsDict;
    public int currentVideoIndex = 0;
    private bool isPreparingNext = false;

    void Start()
    {
        videoPlayer.gameObject.SetActive(true);
        videoClipsDict = new Dictionary<string, VideoClip>
        {
            { "CutShot_Miss", Resources.Load<VideoClip>("Videos/CutShot_Miss") },
            { "CutShot_Deflect", Resources.Load<VideoClip>("Videos/CutShot_Deflect") },
            { "CutShot_Moderate", Resources.Load<VideoClip>("Videos/CutShot_Moderate") },
            { "BackFootDefense_Deflect", Resources.Load<VideoClip>("Videos/Backfoot_Deflect") },
            { "BackFootDefense_Down", Resources.Load<VideoClip>("Videos/Backfoot") },
            { "WC_Catch", Resources.Load<VideoClip>("Videos/WC_Catch") },
            { "Out", Resources.Load<VideoClip>("Videos/Out") },
            { "Six", Resources.Load<VideoClip>("Videos/Six") },
        };

        clipSequence = new List<VideoClip>();
        videoPlayer.loopPointReached += OnVideoFinished;
    }


    void Update()
    {


        if (GameManager.Instance.startVideoSequence)
        {
            Debug.Log("Starting video sequence");
            videoPlayer.gameObject.SetActive(true);

            // Ensure the sequence is cleared and re-initialized only once
            clipSequence.Clear();
            Debug.Log(GameManager.Instance.range + "...from player");
            if (GameManager.Instance.sliderScore < .6f)
            {
                switch (GameManager.Instance.range)
                {
                    case "Out Of Range":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Miss"]);
                            clipSequence.Add(videoClipsDict["CutShot_Miss"]);
                            clipSequence.Add(videoClipsDict["WC_Catch"]);
                            break;
                        }
                    case "Very Early":
                        {
                            clipSequence.Add(videoClipsDict["BackFootDefense_Deflect"]);
                            clipSequence.Add(videoClipsDict["BackFootDefense_Deflect"]);
                            clipSequence.Add(videoClipsDict["WC_Catch"]);
                            clipSequence.Add(videoClipsDict["Out"]);
                            break;
                        }
                    case "Early":
                        {
                            clipSequence.Add(videoClipsDict["BackFootDefense_Deflect"]);
                            clipSequence.Add(videoClipsDict["BackFootDefense_Deflect"]);
                            clipSequence.Add(videoClipsDict["WC_Catch"]);
                            break;
                        }
                    case "Good":
                        {
                            clipSequence.Add(videoClipsDict["BackFootDefense_Down"]);
                            break;
                        }
                    case "Very Good":
                        {
                            clipSequence.Add(videoClipsDict["BackFootDefense_Down"]);
                            break;
                        }
                    case "Perfect":
                        {
                            clipSequence.Add(videoClipsDict["BackFootDefense_Down"]);
                            break;
                        }
                    case "Late":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Miss"]);
                            clipSequence.Add(videoClipsDict["CutShot_Miss"]);
                            clipSequence.Add(videoClipsDict["WC_Catch"]);
                            clipSequence.Add(videoClipsDict["Out"]);
                            break;
                        }
                }
            }
            else
            {
                switch (GameManager.Instance.range)
                {
                    case "Out Of Range":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Miss"]);
                            clipSequence.Add(videoClipsDict["CutShot_Miss"]);
                            clipSequence.Add(videoClipsDict["WC_Catch"]);
                            break;
                        }
                    case "Very Early":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Deflect"]);
                            clipSequence.Add(videoClipsDict["CutShot_Deflect"]);
                            clipSequence.Add(videoClipsDict["WC_Catch"]);
                            clipSequence.Add(videoClipsDict["Out"]);
                            break;
                        }
                    case "Early":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Miss"]);
                            clipSequence.Add(videoClipsDict["CutShot_Miss"]);
                            clipSequence.Add(videoClipsDict["WC_Catch"]);
                            break;
                        }
                    case "Good":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Moderate"]);
                            break;
                        }
                    case "Very Good":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Moderate"]);
                            break;
                        }
                    case "Perfect":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Moderate"]);
                            break;
                        }
                    case "Late":
                        {
                            clipSequence.Add(videoClipsDict["CutShot_Deflect"]);
                            clipSequence.Add(videoClipsDict["CutShot_Deflect"]);
                            clipSequence.Add(videoClipsDict["WC_Catch"]);
                            clipSequence.Add(videoClipsDict["Out"]);
                            break;
                        }
                }
            }






            //clipSequence.Add(videoClipsDict["CutShotMiss"]);
            //clipSequence.Add(videoClipsDict["CutShotBack"]);
            //clipSequence.Add(videoClipsDict["CutShotMiss"]);
            //clipSequence.Add(videoClipsDict["CutShotBack"]);

            currentVideoIndex = 0;
            PlayFirstVideo();
            GameManager.Instance.startVideoSequence = false;
        }
    }

    void PlayFirstVideo()
    {
        if (clipSequence.Count < 1) return;

        videoPlayer.clip = clipSequence[0];
        videoPlayer.Play();

        // Preload the next clip if available
        if (clipSequence.Count > 1)
        {
            StartCoroutine(PrepareNextClip(1));
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (currentVideoIndex < clipSequence.Count - 1)
        {
            currentVideoIndex++;
            videoPlayer.clip = clipSequence[currentVideoIndex];
            videoPlayer.Play();

            // Preload the next clip if there is one
            if (currentVideoIndex + 1 < clipSequence.Count)
            {
                StartCoroutine(PrepareNextClip(currentVideoIndex + 1));
            }
        }
        else
        {
            videoPlayer.gameObject.SetActive(false);
            clipSequence.Clear();
            currentVideoIndex = 0;
        }
    }

    IEnumerator PrepareNextClip(int index)
    {
        if (isPreparingNext) yield break;  // Avoid multiple preloads
        isPreparingNext = true;

        VideoClip nextClip = clipSequence[index];
        videoPlayer.clip = nextClip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        isPreparingNext = false;
    }
}

