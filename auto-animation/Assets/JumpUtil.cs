using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

static class JumpUtil {
    public static string ArrayToString<T>(T[] arr, string separator = ", ") {
        return string.Join(separator, arr.Select(e => e.ToString()).ToArray());
    }
    
    public static Vector3[] minAABB3d(Vector3[] points) {
        // separate the points into their components
        float[] x_set = points.Select(v => v.x).Cast<float>().ToArray();
        float[] y_set = points.Select(v => v.y).Cast<float>().ToArray();
        float[] z_set = points.Select(v => v.z).Cast<float>().ToArray();
    
        // find the intervals for x, y, z
        float[] x_int = new float[2];
        float[] y_int = new float[2];
        float[] z_int = new float[2];

        x_int[0] = x_set.Min();
        y_int[0] = y_set.Min();
        z_int[0] = z_set.Min();
        
        x_int[1] = x_set.Max();
        y_int[1] = y_set.Max();
        z_int[1] = z_set.Max();

        Vector3[] corners =( 
            from x in x_int
            from y in y_int
            from z in z_int
            select new Vector3(x, y, z)).Cast<Vector3>().ToArray();
        return corners;
    }

    public static Vector3[] minAABB2d(Vector3[] points, float zvalue = 0.0f) {
        // This function is just here as a version which skips the calculations
        // with the z values to save time, specifying a given z value instead

        // separate the points into their components
        float[] x_set = points.Select(v => v.x).Cast<float>().ToArray();
        float[] y_set = points.Select(v => v.y).Cast<float>().ToArray();
    
        // find the intervals for x, y, z
        float[] x_int = new float[2];
        float[] y_int = new float[2];

        x_int[0] = x_set.Min();
        y_int[0] = y_set.Min();
        
        x_int[1] = x_set.Max();
        y_int[1] = y_set.Max();

        Vector3[] corners =( 
            from x in x_int
            from y in y_int
            select new Vector3(x, y, zvalue)).Cast<Vector3>().ToArray();
        return corners;
    }

    public static void Test3D() {
        Vector3[] pts = new Vector3[5];
        
        pts[0] = new Vector3(0, 0, 0);
        pts[1] = new Vector3(5, 0, 0);
        pts[2] = new Vector3(5, 6, 0);
        pts[3] = new Vector3(0, 4, 5);
        pts[4] = new Vector3(0, 2, 3);
        
        Vector3[] res = minAABB3d(pts);
        string[] outstr = res.Select(v=>v.ToString()).ToArray();
        Debug.Log("Test3D Should be (0, 0, 0) -> (5, 6, 5) corners, len 8 (" + res.Length + "): " + string.Join(", ", outstr));
    }
    public static void Test2D() {
        Vector3[] pts = new Vector3[5];
        
        pts[0] = new Vector3(0, 0, 0);
        pts[1] = new Vector3(5, 0, 0);
        pts[2] = new Vector3(5, 6, 0);
        pts[3] = new Vector3(0, 4, 5);
        pts[4] = new Vector3(0, 2, 3);
        
        Vector3[] res = minAABB2d(pts);
        string[] outstr = res.Select(v=>v.ToString()).ToArray();
        Debug.Log("Test3D Should be (0, 0, 0) -> (5, 6, 0) corners, len 4 (" + res.Length + "): " + string.Join(", ", outstr));
    }
}
