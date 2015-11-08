using UnityEngine;
using System.Collections;

[System.Serializable]
public class CameraView : MonoBehaviour {
    [HideInInspector]
    public Camera SideView;
    [HideInInspector]
    public Camera SlantView;
    [HideInInspector]
    public Camera FrontView;
    [HideInInspector]
    public Camera TrackingView;
    public Camera MainView;

    public string whichCamera = "camera";
    
    public FrameCapture Side;
    public FrameCapture Slant;
    public FrameCapture Front;
    public FrameCapture Tracking;
    
    public static int sideNumber = 1; 
    public static int slantNumber = 1; 
    public static int frontNumber = 1; 
    public static int trackingNumber = 1;
    public static int mainNumber = 1;
    public static string fileName = "_frame";
    public string destinationFolder;
    
    public void Start() {
        SideView = Side.GetComponent<Camera>();
        SlantView = Slant.GetComponent<Camera>();
        FrontView = Front.GetComponent<Camera>();
        TrackingView = Tracking.GetComponent<Camera>();
    }

    public void TakeScreenshot() {
        int number = mainNumber;
        if (whichCamera == "side") {
            number = sideNumber;
        }
        else if (whichCamera == "front") {
            number = frontNumber;
        }
        else if (whichCamera == "slant") {
            number = slantNumber;
        }
        else if (whichCamera == "tracking") {
            number = trackingNumber;
        }

        string name = number.ToString("D4");
 
        while (System.IO.File.Exists(destinationFolder + whichCamera + fileName + name + ".png")) {
            number++;
            name = number.ToString("D4");
        }
 
        if (whichCamera == "side") {
            sideNumber = number + 1;
        }
        else if (whichCamera == "front") {
            frontNumber = number + 1;
        }
        else if (whichCamera == "slant") {
            slantNumber = number + 1;
        }
        else if (whichCamera == "tracking") {
            trackingNumber = number + 1;
        }
        else {
            mainNumber = number + 1;
        }
    
        Debug.Log("Capture screenshot from camera " + whichCamera);
 
        Application.CaptureScreenshot(destinationFolder + whichCamera + fileName + name + ".png");
    }

    public void GrabFrameSet() {
        // grab a screenshot from every camera angle
        Side.CaptureFrame(fileName, destinationFolder);
        Front.CaptureFrame(fileName, destinationFolder);
        Slant.CaptureFrame(fileName, destinationFolder);
        Tracking.CaptureFrame(fileName, destinationFolder);
    }
    
    public void UseSideView() {
        MainView.transform.position = SideView.transform.position;
        MainView.transform.rotation = SideView.transform.rotation;
        
        whichCamera = "side";
    }
    public void UseSlantView() {
        MainView.transform.position = SlantView.transform.position;
        MainView.transform.rotation = SlantView.transform.rotation;

        whichCamera = "slant";
    }
    public void UseFrontView() {
        MainView.transform.position = FrontView.transform.position;
        MainView.transform.rotation = FrontView.transform.rotation;

        whichCamera = "front";
    }
    public void UseTrackingView() {
        MainView.transform.position = TrackingView.transform.position;
        MainView.transform.rotation = TrackingView.transform.rotation;

        whichCamera = "tracking";
    }
}
