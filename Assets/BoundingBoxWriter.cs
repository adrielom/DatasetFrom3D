using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoundingBoxWriter{
    List<AABB> boxes = new List<AABB>();
    private Camera camera;

    public BoundingBoxWriter(Camera camera){
        boxes = new List<AABB>();
        this.camera = camera;
    }

    public void AddBoundingBox(string nameFile, Vector3 minPos, Vector3 maxPos){
        Vector3 minPosConverted = camera.WorldToScreenPoint(minPos);
        Vector3 maxPosConverted = camera.WorldToScreenPoint(maxPos);
        AddImage(nameFile, minPosConverted, maxPosConverted);
    }
    private void AddImage(string nameFile, Vector3 minPos, Vector3 maxPos){
        Debug.Log("Min position is: " + minPos);
        Debug.Log("Max position is: " + maxPos);
        Debug.Log("Name of the file is: " + nameFile);
        boxes.Add(new AABB(nameFile, minPos, maxPos));
    }

    
}


public class AABB{
    private Vector3 minPos;
    private Vector3 maxPos;
    private string nameFile;

    public AABB(string nameFile, Vector3 minPos, Vector3 maxPos){
        this.minPos = minPos;
        this.maxPos = maxPos;
        this.nameFile = nameFile;
    }
}