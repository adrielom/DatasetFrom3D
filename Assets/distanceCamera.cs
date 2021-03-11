using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class distanceCamera : MonoBehaviour
{
    public Camera camera;
    public TextMeshProUGUI text;
    BoundingBoxWriter writer;
    public SkinnedMeshRenderer meshRenderer;
    // Get mesh origin and farthest extent (this works best with simple convex meshes)
    Vector3 origin;
    Vector3 extent;
    

    // Start is called before the first frame update
    void Start()
    {
        writer = new BoundingBoxWriter(camera);
        Debug.Log(Area());
        origin = Camera.main.WorldToScreenPoint(new Vector3(meshRenderer.bounds.min.x, meshRenderer.bounds.max.y, 0f));
        extent = Camera.main.WorldToScreenPoint(new Vector3(meshRenderer.bounds.max.x, meshRenderer.bounds.min.y, 0f));
    }

    void Update() {
        text.text = $"Area is: {Area()}\nDistance camera: {(meshRenderer.transform.position.z - transform.position.z).ToString("f3")}";
    }

    
    public float Area()
    {
        float area = (extent.x - origin.x) * (origin.y - extent.y) * meshRenderer.transform.position.z * -1;
        return area;
        // Create rect in screen space and return - does not account for camera perspective
        // return new Rect(origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
    }
        

}
