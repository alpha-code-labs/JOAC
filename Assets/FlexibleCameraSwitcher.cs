using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class FlexibleCameraSwitcher : MonoBehaviour
{

    static List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();
    public static CinemachineVirtualCamera activeCamera;

    public static bool IsActiveCamera(CinemachineVirtualCamera camera) {
        return camera == activeCamera;
    }


    public static void SwitchCamera(CinemachineVirtualCamera camera) {
        if (camera == null) {
            Debug.Log("camera is null...");
        }

        if (camera.name == "MainCam") {
            camera.Priority = 15;
        }
        else camera.Priority = 10;


        activeCamera = camera;

        foreach(CinemachineVirtualCamera cam in cameras)
        {
            Debug.Log(camera.name + "..camera");
            if(cam != camera)
            {
                cam.Priority = 0;
            }

        }
    }

    public static void Register(CinemachineVirtualCamera camera) {
        cameras.Add(camera);
    }

    public static void UnRegister(CinemachineVirtualCamera camera)
    {
        cameras.Remove(camera);
    }
}
