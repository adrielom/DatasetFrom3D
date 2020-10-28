using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ImageProcessing {
        
    public class ScreenShot : MonoBehaviour
    {
        public int resWidth = 2550;
        public int resHeight = 3300;

        private bool takeHiResShot = false;
        public Camera camera;
        public Image image;
        public GameObject target;
        
        [SerializeField]
        bool showBounds = true;

        int offsetGrade = 30;
        int maxGrade = 350;
        private float timeChanger = 5;
        private BoundingBoxWriter writer;
        private Background background;

        private Renderer meshRenderer;

        // [SerializeField]
        // private Material mat;

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


        // void OnPostRender()
        // {
        //     if (!mat)
        //     {
        //         Debug.LogError("Please Assign a material on the inspector");
        //         return;
        //     }
        //      GL.PushMatrix();
        //     mat.SetPass(0);
        //     GL.LoadPixelMatrix();
        //     GL.Color(Color.red);

        //     GL.Begin(GL.TRIANGLES);
        //     GL.Vertex3(0, 0, 0);
        //     GL.Vertex3(0, Screen.height / 2, 0);
        //     GL.Vertex3(Screen.width / 2, Screen.height / 2, 0);
        //     GL.End();

        //     GL.PopMatrix();
        // }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            background = new Background(image);
            meshRenderer = target.GetComponent<MeshRenderer>();
            StartCoroutine(background.GetTexture());
            writer = new BoundingBoxWriter(camera);
            StartCoroutine(RotateAroundIt());

        }

        void LateUpdate()
        {
            camera.transform.LookAt(target.transform.position);
            // BoundsToScreenRect();
            timeChanger -= Time.deltaTime;
            if (timeChanger <= 0) {
                timeChanger = 5;
                StartCoroutine(background.GetTexture());
            }

        }


        public Rect? BoundsToScreenRect()
        {

            if(!showBounds) return null;
            // Get mesh origin and farthest extent (this works best with simple convex meshes)
            Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(meshRenderer.bounds.min.x, meshRenderer.bounds.max.y, 0f));
            Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(meshRenderer.bounds.max.x, meshRenderer.bounds.min.y, 0f));
            
            // Create rect in screen space and return - does not account for camera perspective
            return new Rect(origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
        }
        
        private void OnDrawGizmos()
        {
            if (!showBounds) return;

            if (meshRenderer != null)
                Gizmos.DrawWireCube(meshRenderer.bounds.center, meshRenderer.bounds.size);
            Gizmos.color = Color.red;
        }

        public void ResetCamerasTransform () {
            //* Reposition it 
        }

        public IEnumerator RotateAroundIt()
        {
            yield return new WaitUntil(() => background.sprite != null);

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
                    
            writer.WriteToJson();

            Debug.Log("Done");
        }

        


        IEnumerator Shoot(Vector3 rotation)
        {
            target.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            RenderTexture rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            var filenameSection = filename.Split('/');
            writer.AddBoundingBox(filenameSection[filenameSection.Length - 1], meshRenderer.bounds.min, meshRenderer.bounds.max);
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
}