using UnityEngine;
using System.Collections;

[System.Serializable]
public class CameraView : MonoBehaviour {
    public Camera SideView;
    public Camera SlantView;
    public Camera FrontView;
    
    public static int sideNumber = 1; 
    public static int slantNumber = 1; 
    public static int frontNumber = 1; 
    public static string fileName = "_frame";
    public string destinationFolder;

    public void TakeScreenshot() {
        int number = sideNumber;
        if (SlantView.enabled) {
            number = slantNumber;
        }
        else if (FrontView.enabled) {
            number = frontNumber;
        }
        string name = number.ToString("D4");
        string camera = GetCameraName();
 
        while (System.IO.File.Exists(destinationFolder + camera + fileName + name + ".png"))
        {
            number++;
            name = number.ToString("D4");
        }
 
        if (SideView.enabled) {
            sideNumber = number + 1;
        }
        else if (FrontView.enabled) {
            frontNumber = number + 1;
        }
        else if (SlantView.enabled) {
            slantNumber = number + 1;
        }
 
        Application.CaptureScreenshot(destinationFolder + camera + fileName + name + ".png");
    }
    
    public string GetCameraName() {
        if (SideView.enabled) {
            return "side";
        }
        else if (FrontView.enabled) {
            return "front";
        }
        else if (SlantView.enabled) {
            return "slant";
        }
        else {
            return "";
        }
    }
    
    public void UseSideView() {
        SideView.enabled = true;
        SlantView.enabled = false;
        FrontView.enabled = false;
    }
    public void UseSlantView() {
        SideView.enabled = false;
        SlantView.enabled = true;
        FrontView.enabled = false;
    }
    public void UseFrontView() {
        SideView.enabled = false;
        SlantView.enabled = false;
        FrontView.enabled = true;
    }
}
