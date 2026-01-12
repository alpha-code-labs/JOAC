using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace IKExperiment
{
    public class UpdateVideoPlayerOrientation : MonoBehaviour
    {
        public List<GameObject> videoPlayers;
        public GameObject skipButton;

        // Start is called before the first frame update
        void Start()
        {
            if (videoPlayers == null)
            {
                videoPlayers = new List<GameObject>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            {
                foreach (GameObject player in videoPlayers)
                {
                    if (player != null)
                    {
                        VideoPlayer vp = player.GetComponent<VideoPlayer>();
                        if (vp != null)
                        {
                            // Rotate the video player to match landscape orientation
                            vp.transform.rotation = Quaternion.Euler(0, 0, 90);
                            RectTransform rectTransform = skipButton.GetComponent<RectTransform>();
                            rectTransform.anchoredPosition = new Vector2(-113, 2124);
                            rectTransform.localRotation = Quaternion.Euler(0, 0, 90);
                        }
                    }
                }
            }

            if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
            {
                foreach (GameObject player in videoPlayers)
                {
                    if (player != null)
                    {
                        VideoPlayer vp = player.GetComponent<VideoPlayer>();
                        if (vp != null)
                        {
                            // Rotate the video player to match landscape orientation
                            vp.transform.rotation = Quaternion.Euler(0, 0, -90);
                            RectTransform rectTransform = skipButton.GetComponent<RectTransform>();
                            rectTransform.anchoredPosition = new Vector2(-977, 182);
                            rectTransform.localRotation = Quaternion.Euler(0, 0, -90);
                        }
                    }
                }
            }
        }
    }
}
