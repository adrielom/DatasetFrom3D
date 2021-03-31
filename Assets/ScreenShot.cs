using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace ImageProcessing {

    public enum WriteBoxesMode
    {
        YOLO,
        YOLO2,
        TRADITIONAL,
        ALL
    }

    [Serializable]
    public struct CameraConfigs
    {
        [SerializeField]
        public bool rotateX;
        [SerializeField]
        [Range(-180,180)]
        public float minX;
        [SerializeField]
        [Range(-180, 180)]
        public float maxX;
        [SerializeField]
        public bool rotateY;
        [SerializeField]
        [Range(-180, 180)]
        public float minY;
        [SerializeField]
        [Range(-180, 180)]
        public float maxY;
        [SerializeField]
        public bool rotateZ;
        [SerializeField]
        [Range(-180, 180)]
        public float minZ;
        [SerializeField]
        [Range(-180, 180)]
        public float maxZ;
        [Range(-1000,1000)]
        [SerializeField]
        public float minDistanceRange;
        [Range(0, 1000)]
        [SerializeField]
        public float maxDistanceRange;
    }

    public class ScreenShot : MonoBehaviour
    {
        public int resWidth = 2550;
        public int resHeight = 3300;

        private bool takeHiResShot = false;
        public Camera camera;
        public Light light;
        public Image image;
        public GameObject target;
        
        [SerializeField]
        bool showBounds = true;
        int offsetGrade = 30;
        int maxGrade = 350;
        private float timeChanger;
        public float initialTimeChanger = 2;
        private BoundingBoxWriter writer;
        private Background background;

        private SkinnedMeshRenderer meshRendererSolo;

        [SerializeField]
        private List<SkinnedMeshRenderer> meshRenders = new List<SkinnedMeshRenderer>();

        [SerializeField]
        private List<int> objectClasses = new List<int>();
        [SerializeField]
        int objectClass = 0;

        [SerializeField]
        private CameraConfigs cameraConfigs; 


        private List<Rect> currentRects = new List<Rect>();
        private Rect currentRect = new Rect(0, 0, 0, 0);

        private Texture2D texture;

        private GUIStyle borderStyle;

        [Range(1,1000000)]
        [SerializeField]
        private int NUMBER_OF_PHOTOS = 10;

        [SerializeField]
        private WriteBoxesMode mode = WriteBoxesMode.YOLO;
        [SerializeField]
        bool withNoise = false;
        [SerializeField]
        [Range(0,1)]
        float minNoise = 0f;
        [SerializeField]
        [Range(0,1)]
        float maxNoise = 0.5f;

        FilmGrain filmGrain;



        #region "ORBIT_VARIABLES"
        float rotZAxis;
        float rotYAxis;
        float rotXAxis;
        float originalRotX;
        float originalRotY;
        float originalRotZ;
        float distance;
        #endregion

        bool finished = false;
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
            timeChanger = initialTimeChanger;
            background = new Background(image);
            finished = false;
            meshRendererSolo = target.GetComponentInChildren<SkinnedMeshRenderer>();
            GameObject.Find("Grain").GetComponent<Volume>().profile.TryGet<FilmGrain>(out filmGrain);

            try {
                StartCoroutine(background.GetTexture());
            } catch (Exception e) {
                Debug.Log(e);
            }
            writer = new BoundingBoxWriter(camera);
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0, 0, 1, 0.5f));
            texture.Apply();

            borderStyle = new GUIStyle();
            borderStyle.border = new RectOffset(2, 2, 2, 2);


            var initialPoint = camera.transform.position - target.transform.position;
            originalRotY = camera.transform.eulerAngles.y;
            originalRotX = camera.transform.eulerAngles.x;
            originalRotZ = camera.transform.eulerAngles.z;

            StartCoroutine(RoutineOfPhotos());




            // StartCoroutine(RotateAroundIt());
            //RepositionCameraTransform();
        }

        private void Update()
        {
            
        }

        //void LateUpdate()
        //{
        //    camera.transform.LookAt(target.transform.position);
        //    // BoundsToScreenRect();
        //    timeChanger -= Time.deltaTime;
        //    if (timeChanger <= 0) {
        //        timeChanger = initialTimeChanger;
        //        // StartCoroutine(background.GetTexture());
        //        StartCoroutine(RepositionCameraTransform());
        //        UpdateLightConfig();
        //        UpdateVolumeConfig();
        //    }

        //}

        private void OnDestroy()
        {
            if (!finished)
            {
                writer.WriteToJson();
            }
        }

        void OnGUI()
        {
            foreach(Rect currentRect in currentRects)
            {
                texture.SetPixel(0, 0, UnityEngine.Random.ColorHSV(0,1,0,1,0,1,0.5f,0.5f));
                GUI.skin.box.normal.background = texture;
                GUI.Box(currentRect, GUIContent.none);
            }
        }

        public IEnumerator RoutineOfPhotos()
        {
            for(int i = 0; i< NUMBER_OF_PHOTOS; i++)
            {
                UpdateLightConfig();
                UpdateVolumeConfig();
                yield return RepositionCameraTransformMode2();
                yield return new WaitForSeconds(0.2f);
                GC.Collect();
            }
            writer.WriteToJson();
            finished = true;

        }


        public Rect BoundsToScreenRect()
        {
            float x1 = float.MaxValue, y1 = float.MaxValue, x2 = 0.0f, y2 = 0.0f;

            foreach(SkinnedMeshRenderer meshRenderer in meshRenders)
            {
                Vector3[] vertices = meshRenderer.sharedMesh.vertices;
                Debug.Log("Mesh: " + meshRenderer.name);
                foreach (Vector3 vert in vertices)
                {
                    Vector2 tmp = WorldToGUIPoint(meshRenderer.transform.TransformPoint(vert));

                    if (tmp.x < x1) x1 = tmp.x;
                    if (tmp.x > x2) x2 = tmp.x;
                    if (tmp.y < y1) y1 = tmp.y;
                    if (tmp.y > y2) y2 = tmp.y;
                }
            }


            if (meshRenders.Count > 0)
            {
                

                Rect bbox = new Rect(x1, y1, x2 - x1, y2 - y1);
                return bbox;
            }
            else
            {
                return new Rect(0, 0, 0, 0);
            }
        }


        public List<Rect> DetermineRects()
        {
            List<Rect> rects = new List<Rect>();
            foreach (SkinnedMeshRenderer meshRenderer in meshRenders)
            {
                float x1 = float.MaxValue, y1 = float.MaxValue, x2 = 0.0f, y2 = 0.0f;
                Vector3[] vertices = meshRenderer.sharedMesh.vertices;
                Debug.Log("Mesh: " + meshRenderer.name);
                foreach (Vector3 vert in vertices)
                {
                    Vector2 tmp = WorldToGUIPoint(meshRenderer.transform.TransformPoint(vert));

                    if (tmp.x < x1) x1 = tmp.x;
                    if (tmp.x > x2) x2 = tmp.x;
                    if (tmp.y < y1) y1 = tmp.y;
                    if (tmp.y > y2) y2 = tmp.y;
                }
                Rect bbox = new Rect(x1, y1, x2 - x1, y2 - y1);
                rects.Add(bbox);
            }

            return rects;
        }

        
        

        public static Vector2 WorldToGUIPoint(Vector3 world)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
            screenPoint.y = (float)Screen.height - screenPoint.y;
            return screenPoint;
        }

        private void OnDrawGizmos()
        {
            if (!showBounds) return;

            if (meshRendererSolo != null)
                Gizmos.DrawWireCube(meshRendererSolo.bounds.center, meshRendererSolo.bounds.size);
            Gizmos.color = Color.red;
        }

        IEnumerator RepositionCameraTransform () {
            yield return background.GetTexture();
            Vector3 inictialPoint = target.transform.position - Vector3.one * Mathf.Clamp(UnityRandom.Range(cameraConfigs.minDistanceRange, cameraConfigs.maxDistanceRange) * Mathf.PerlinNoise(cameraConfigs.minDistanceRange, cameraConfigs.maxDistanceRange), cameraConfigs.minDistanceRange, cameraConfigs.maxDistanceRange);
            Vector3 randomVector = GetRandomVector();
            print($"Initial point is: {inictialPoint}");
            var a = inictialPoint - randomVector * UnityRandom.Range(1f, 5f) + Vector3.up * 25;
            
        
            camera.transform.position = a;
            //camera.transform.LookAt(target.transform.position, Vector3.up);
            yield return Shoot();
        }

        IEnumerator RepositionCameraTransformMode2()
        {
            yield return background.GetTexture();
            //Vector3 inictialPoint = target.transform.position - camera.transform.TransformDirection(0, 0, Mathf.Clamp(UnityRandom.Range(cameraConfigs.minDistanceRange, cameraConfigs.maxDistanceRange) * Mathf.PerlinNoise(cameraConfigs.minDistanceRange, cameraConfigs.maxDistanceRange), cameraConfigs.minDistanceRange, cameraConfigs.maxDistanceRange));
            //camera.transform.RotateAround(target.transform.position, Vector3.right, 5);
            //camera.transform.LookAt(target.transform);
            Orbit();
            yield return Shoot();
        }




        public void Orbit()
        {
            Quaternion toRotation = CalculateRotation();
            Quaternion rotation = toRotation;

            distance = UnityRandom.Range(cameraConfigs.minDistanceRange, cameraConfigs.maxDistanceRange);

            Vector3 negDistance = new Vector3(0, 0, -distance);
            Vector3 position = rotation * negDistance + target.transform.position;

            camera.transform.rotation = rotation;
            camera.transform.position = position;
        }

        private Quaternion CalculateRotation()
        {
            if (cameraConfigs.rotateX)
            {
                rotXAxis = ClampAngle(UnityRandom.Range(cameraConfigs.minX, cameraConfigs.maxX) + originalRotX, cameraConfigs.minX, cameraConfigs.maxX);
               
            }
            if (cameraConfigs.rotateY)
            {
                rotYAxis = ClampAngle(UnityRandom.Range(cameraConfigs.minY, cameraConfigs.maxY) + originalRotY, cameraConfigs.minY, cameraConfigs.maxY);
            }
            if (cameraConfigs.rotateZ)
            {
                rotZAxis = ClampAngle(UnityRandom.Range(cameraConfigs.minZ, cameraConfigs.maxZ) + originalRotZ, cameraConfigs.minZ, cameraConfigs.maxZ);
            }
            return Quaternion.Euler(cameraConfigs.rotateX ? rotXAxis : originalRotX, cameraConfigs.rotateY ? rotYAxis : originalRotY, cameraConfigs.rotateZ ? rotZAxis : originalRotZ);
        }


        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }



        public void UpdateVolumeConfig () {

            if (withNoise)
            {
                filmGrain.intensity.Override(UnityRandom.Range(minNoise, maxNoise));
                filmGrain.response.Override(UnityRandom.Range(minNoise, maxNoise));
            }
            else
            {
                filmGrain.intensity.Override(0);
                filmGrain.response.Override(0);
            }
            // volume.GetComponent<FilmGrain>().type.Override(UnityRandom.Range(0, 10));
        }

        public void UpdateLightConfig () {
            Vector3 inictialPoint = target.transform.position - Vector3.one * UnityRandom.Range(1, 5);
            light.transform.position = inictialPoint - GetRandomVector();
            light.intensity = UnityRandom.Range(0f, 5f);
            light.shadowStrength = UnityRandom.Range(0f, 1f);
        }

        Vector3 GetRandomVector () {
            return new Vector3(cameraConfigs.rotateX ? UnityRandom.Range(cameraConfigs.minX, cameraConfigs.maxX) : 0,
                cameraConfigs.rotateY ? UnityRandom.Range(cameraConfigs.minY, cameraConfigs.maxY) : 0,
                cameraConfigs.rotateZ ? UnityRandom.Range(cameraConfigs.minZ, cameraConfigs.maxZ) : 0);
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
            writer.AddBoundingBox(filenameSection[filenameSection.Length - 1], meshRendererSolo.bounds.min, meshRendererSolo.bounds.max);
            takeHiResShot = false;
            DestroyImmediate(screenShot);
            yield return new WaitForSeconds(0.2f);
        }

        IEnumerator Shoot()
        {
            RenderTexture rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(camera.pixelWidth, camera.pixelHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            var filenameSection = filename.Split('/');
            WriteBox(filenameSection[filenameSection.Length - 1]);
            takeHiResShot = false;
            Destroy(screenShot);
            
            yield return null;
        }


        private void WriteBox(string filename)
        {
            currentRects.Clear();
            switch (mode)
            {
                case WriteBoxesMode.TRADITIONAL:
                    currentRects.Add(BoundsToScreenRect());
                    writer.AddBoundingBox(filename, currentRects[0]); 
                    break;
                case WriteBoxesMode.YOLO:
                    currentRects.AddRange(DetermineRects());
                    writer.WriteBoxMultipleInJson(filename, currentRects, objectClasses);
                    break;
                case WriteBoxesMode.YOLO2:
                    currentRects.Add(BoundsToScreenRect());
                    writer.WriteBoxAloneInJson(filename, currentRects, objectClass);
                    break;
                case WriteBoxesMode.ALL:
                    currentRects.Add(BoundsToScreenRect());
                    writer.AddBoundingBox(filename, currentRects[0]);
                    currentRects.Clear();
                    currentRects.AddRange(DetermineRects());
                    writer.WriteBoxMultipleInJson(filename, currentRects, objectClasses);
                    currentRects.Clear();
                    currentRects.Add(BoundsToScreenRect());
                    writer.WriteBoxAloneInJson(filename, currentRects, objectClass);
                    break;
            }
        }

        // IEnumerator ShootByScreenshot(Vector3 rotation){
        //     yield return new WaitForSeconds(0.4f);
        //     target.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
        //     ScreenCapture.CaptureScreenshot()
        // }
    }
}