using System.Collections;
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
        clickedObject.SetActive(true);
    }

    public void DestroyClickedObject()
    {
        Destroy(clickedObject);
    }

    public void ClearClickedObject()
    {
        if(clickedObject != null)
            clickedObject.SetActive(false);
        clickedObject = null;
    }

    void OnPostRender()
    {
        if (clickedObject == null)
            return;

        GameObject obj = clickedObject;
        highlightMaterial.SetPass(0);
        clickedObject.GetComponentInChildren<MeshRenderer>().material = highlightMaterial;
        Component[] meshes = obj.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter m in meshes)
        {
            Graphics.DrawMeshNow(m.sharedMesh, m.transform.localToWorldMatrix);
        }
    }

    Material CreateHighlightMat()
    {
        //Material material = new Material(Shader.Find("Outlined/Silhouette Only"));
        Material material = new Material(Shader.Find("Unlit/Transparent Color"));

        //Texture2D tex = new Texture2D(1, 1);
        //tex.SetPixel(0, 0, (Color.red + Color.yellow) * 0.5f);
        //tex.Apply();
        Color color = (Color.red + Color.yellow) * 0.5f;
        color.a = 0.3f;
        material.SetColor("_Color", color);
        //material.SetFloat("_Cutoff", 0.5f);

        return material;
    }
}
