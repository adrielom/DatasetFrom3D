using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    public int resWidth = 2550;
    public int resHeight = 3300;

    private bool takeHiResShot = false;
    public Camera camera;
    public GameObject target;

    int offsetGrade = 30;
    int maxGrade = 350;

    private BoundingBoxWriter writer;

    [SerializeField]
    private Renderer meshTarget;

    public static string ScreenShotName(int width, int height)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/screenshots/";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png",
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            width, height,
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff"));
    }

    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        writer = new BoundingBoxWriter(camera);
        StartCoroutine(RotateAroundIt());

    }

    void LateUpdate()
    {
        camera.transform.LookAt(target.transform.position);

    }

    public IEnumerator RotateAroundIt()
    {
        for (int i = 0; i < maxGrade; i += offsetGrade)
        {
            for (int j = 0; j < maxGrade; j += offsetGrade)
            {
                for (int k = 0; k < maxGrade; k += offsetGrade)
                {
                    yield return Shoot(new Vector3(i, j, k));
                }
            }
        }
        Debug.Log("Done");
    }

    IEnumerator Shoot(Vector3 rotation)
    {
        target.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(resWidth, resHeight);
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
        writer.AddBoundingBox(filename, meshTarget.bounds.min, meshTarget.bounds.max);
        takeHiResShot = false;
        DestroyImmediate(screenShot);
        yield return new WaitForSeconds(0.2f);
    }

    // IEnumerator ShootByScreenshot(Vector3 rotation){
    //     yield return new WaitForSeconds(0.4f);
    //     target.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    //     ScreenCapture.CaptureScreenshot()
    // }
}