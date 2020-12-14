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

    public void AddBoundingBox (Vector3 minPos, Vector3 maxPos) {
        Vector3 minPosConverted = camera.WorldToScreenPoint(new Vector3(minPos.x, minPos.y, minPos.z));
        Vector3 maxPosConverted = camera.WorldToScreenPoint(new Vector3(maxPos.x, maxPos.y, maxPos.z));
        AddImage(minPosConverted, maxPosConverted);
    }

    private void AddImage(string nameFile, Vector3 minPos, Vector3 maxPos){
        Debug.Log("Min position is: " + minPos);
        Debug.Log("Max position is: " + maxPos);
        Debug.Log("Name of the file is: " + nameFile);
        boxes.Add(new AABB(nameFile, minPos, maxPos));
    }

    private void AddImage (Vector3 minPos, Vector3 maxPos) {
        Debug.Log("Min position is: " + minPos);
        Debug.Log("Max position is: " + maxPos);
        boxes.Add(new AABB(minPos, maxPos));
    }


    public void WriteToJson(){
        string output = JsonUtility.ToJson(this);
        System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/screenshots/" + "descriptor.json", output);
    }
    
}


[System.Serializable]
public class AABB{

    public Vector3 minPos;
    public Vector3 maxPos;
    public string nameFile;

    public AABB(Vector3 minPos, Vector3 maxPos)
    {
        this.minPos = minPos;
        this.maxPos = maxPos;
    }

    public AABB(string nameFile, Vector3 minPos, Vector3 maxPos){
        this.minPos = minPos;
        this.maxPos = maxPos;
        this.nameFile = nameFile;
    }
}