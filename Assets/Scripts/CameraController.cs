﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{

    private GameObject clickedObject;
    [SerializeField]
    private Material highlightMaterial;

    private void Start()
    {
        if (highlightMaterial == null)
            highlightMaterial = CreateHighlightMat();
    }

    public void SetClickedObject(GameObject obj)
    {
        clickedObject = obj;
    }

    public void ClearClickedObject()
    {
        clickedObject = null;
    }

    void OnPostRender()
    {
        if (clickedObject == null)
            return;

        GameObject obj = clickedObject;
        highlightMaterial.SetPass(0);
        Component[] meshes = obj.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter m in meshes)
        {
            Graphics.DrawMeshNow(m.sharedMesh, m.transform.localToWorldMatrix);
        }
    }

    Material CreateHighlightMat()
    {
        Material material = new Material(Shader.Find("Unlit/Transparent Color"));

        Color color = Color.green;
        color.a = .2f;
        material.color = color;

        return material;
    }
}