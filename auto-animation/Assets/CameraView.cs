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

    public string whichCamera = "camera";
    
    public FrameCapture Side;
    public FrameCapture Slant;
    public FrameCapture Front;
    public FrameCapture Tracking;
    
    public static int sideNumber = 1; 
    public static int slantNumber = 1; 
    public static int frontNumber = 1; 
    public static int trackingNumber = 1;
    public static int startNumber = 1;
    public static string fileName = "_frame";
    public string destinationFolder;
    public bool grabFrames = false;
    
    public void Start() {
        SideView = Side.GetComponent<Camera>();
        SlantView = Slant.GetComponent<Camera>();
        FrontView = Front.GetComponent<Camera>();
        TrackingView = Tracking.GetComponent<Camera>();

        // create the directory if it does not already exist
        if (System.IO.Directory.Exists(destinationFolder)) {
            System.IO.Directory.CreateDirectory(destinationFolder);
        }
    }

    public void TakeScreenshot() {
        int number = startNumber;
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
            startNumber = number + 1;
        }
    
        Debug.Log("Capture screenshot from camera " + whichCamera);
 
        Application.CaptureScreenshot(destinationFolder + whichCamera + fileName + name + ".png");
    }

    public void GrabFrameSet() {
        if (!grabFrames) {
            return;
        }
        // grab a screenshot from every camera angle
        Side.CaptureFrame(fileName, destinationFolder);
        Front.CaptureFrame(fileName, destinationFolder);
        Slant.CaptureFrame(fileName, destinationFolder);
        Tracking.CaptureFrame(fileName, destinationFolder);
    }
    
    public void UseSideView() {
        SideView.enabled        = true;
        SlantView.enabled       = false;
        FrontView.enabled       = false;
        TrackingView.enabled    = false;
        
        whichCamera = "side";
    }
    public void UseSlantView() {
        SideView.enabled        = false;
        SlantView.enabled       = true;
        FrontView.enabled       = false;
        TrackingView.enabled    = false;

        whichCamera = "slant";
    }
    public void UseFrontView() {
        SideView.enabled        = false;
        SlantView.enabled       = false;
        FrontView.enabled       = true;
        TrackingView.enabled    = false;

        whichCamera = "front";
    }
    public void UseTrackingView() {
        SideView.enabled        = false;
        SlantView.enabled       = false;
        FrontView.enabled       = false;
        TrackingView.enabled    = true;

        whichCamera = "tracking";
    }
}
