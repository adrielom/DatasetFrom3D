using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoundingBoxWriter{
    public List<AABB> boxes = new List<AABB>();
    private Camera camera;

    public BoundingBoxWriter(Camera camera){
        boxes = new List<AABB>();
        this.camera = camera;
    }

    public void AddBoundingBox(string nameFile, Vector3 minPos, Vector3 maxPos){
        Vector3 minPosConverted = camera.WorldToScreenPoint(new Vector3(minPos.x, minPos.y, minPos.z));
        // minPosConverted.x -= camera.pixelRect.x;
        // minPosConverted.y -= camera.pixelRect.y;
        Vector3 maxPosConverted = camera.WorldToScreenPoint(new Vector3(maxPos.x, maxPos.y, maxPos.z));
        // maxPosConverted.x -= camera.pixelRect.x;
        // maxPosConverted.y -= camera.pixelRect.y;
        AddImage(nameFile, minPosConverted, maxPosConverted);
    }

    public void AddBoundingBox(string nameFile, Rect points){
        Vector2 minPos = points.min;
        Vector2 maxPos = points.max;
        AddImage(nameFile, minPos, maxPos);
    }
    private void AddImage(string nameFile, Vector2 minPos, Vector2 maxPos){
        Debug.Log("Min position is: " + minPos);
        Debug.Log("Max position is: " + maxPos);
        Debug.Log("Name of the file is: " + nameFile);
        boxes.Add(new AABB(nameFile, minPos, maxPos));
    }


    public void WriteToJson(){
        string output = JsonUtility.ToJson(this);
        System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/screenshots/" + "descriptor.json", output);
    }
    
}


[System.Serializable]
public class AABB{

    public Vector2 minPos;
    public Vector2 maxPos;
    public string nameFile;

    public AABB(string nameFile, Vector3 minPos, Vector3 maxPos){
        this.minPos = minPos;
        this.maxPos = maxPos;
        this.nameFile = nameFile;
    }
}