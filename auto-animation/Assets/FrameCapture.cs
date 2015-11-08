using UnityEngine;
using System.Collections;

public class FrameCapture : MonoBehaviour {
    [HideInInspector]
    public Camera viewCamera;
    [HideInInspector]
    public Texture2D text;
    [HideInInspector]
    public RenderTexture rtext;

    public int startNumber = 1;
    
    public void Start() {
        viewCamera = GetComponent<Camera>();
        // create the texture to be the same size as the render texture
        text = new Texture2D((int)viewCamera.pixelWidth, (int)viewCamera.pixelHeight, TextureFormat.RGB24, false);
        rtext = new RenderTexture((int)viewCamera.pixelWidth, (int)viewCamera.pixelHeight, 24);
    }

    public void CaptureFrame(string filename, string destPath = "") {
        int number = startNumber;
        string name = number.ToString("D4");
        
        while (System.IO.File.Exists(destPath 
                + this.name 
                + filename 
                + name 
                + ".png")) {
            ++number;
            name = number.ToString("D4");
        }
        
        startNumber = number + 1;

        // save what the last active was to maintain state after
        RenderTexture old = RenderTexture.active;
        
        // set the desired render texture as active and render the camera to it
        RenderTexture.active = rtext;
        viewCamera.targetTexture = rtext;
        viewCamera.Render();
        viewCamera.targetTexture = null;
        
        // read the pixels from active render texture to this texture
        text.ReadPixels(new Rect(0, 0, viewCamera.pixelWidth, viewCamera.pixelHeight), 0, 0);
        text.Apply();

        // restore last active texture
        RenderTexture.active = old;

        // save current texture to png
        byte[] bytes = text.EncodeToPNG();
        System.IO.File.WriteAllBytes(destPath 
            + this.name
            + filename 
            + name 
            + ".png"
            , bytes);
    }
}
