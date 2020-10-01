using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{

    private Descriptions descriptions;

    [SerializeField]
    private GameObject[] objects;

    [SerializeField]
    private Camera camera;

    [SerializeField]
    private GameObject objUIPrefab;

    [SerializeField]
    private GameObject mainUIPrefab;

    [SerializeField] [Range(0f, 1f)]
    private float disableOpacity = .3f;

    private GameObject objUIInst;
    private GameObject mainUIInst;

    private Dictionary<GameObject, bool> objDict;

    private float totalCost = 0.0f;

    private float totalR = 0.0f;

    void Start()
    {
        descriptions = GetComponent<Descriptions>();
        if (camera == null)
            camera = Camera.main;
        if (objects.Length == 0)
            Debug.LogError("No objects configured!");
        else
        {
            InitializeObjects();
            CalculateCostAndR();
        }
        if (objUIPrefab == null)
            Debug.LogError("No object UI prefab configured!");
        if (mainUIPrefab == null)
            Debug.LogError("No main UI prefab configured!");
        else
        {
            mainUIInst = Instantiate(mainUIPrefab);
            MainUIController();
        } 
    }

    void Update()
    {
        ObjectSelector();
        CameraRotator();
    }

    /**
     * 1. Traverse through all objects in object list.
     * 2. For every subobject with a mesh, give a mesh collider to allow raycasting
     * 3. Also add the related game object to the object dictionary to allow for enable/disable of object
     */
    void InitializeObjects()
    {
        objDict = new Dictionary<GameObject, bool>();
        foreach (var obj in objects)
        {
            MeshRenderer[] meshComponents = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshItem in meshComponents)
            {
                if (meshItem.gameObject.GetComponent<MeshCollider>() == null)
                    meshItem.gameObject.AddComponent<MeshCollider>();
                objDict.Add(meshItem.gameObject, true);
            }
        }
    }

    /**
     * Calculates the total cost and R value given all the objects currently enabled in the scene. 
     */ 
    void CalculateCostAndR()
    {
        float newCost = 0f;
        float newR = 0f;
        foreach(var obj in objDict)
        {
            if(obj.Value)
            {
                TagObject tagObj = descriptions.GetStructByTag(obj.Key.tag);
                newCost += tagObj.cost;
                newR += tagObj.rVal;
            }
        }
        totalCost = newCost;
        totalR = newR;
        MainUIController();
    }

    /**
     * Controls the main UI
     * (or currently, causses it to update)
     * 
     * TODO: Refactor to be the controller:
     *      handle user input based on event system or something
     */ 
    void MainUIController()
    {
        if (mainUIInst != null)
            mainUIInst.GetComponentInChildren<Text>().text = $"Cost: ${totalCost:F2}\nR: {totalR}";
    }

    void CreateObjectUI(GameObject obj)
    {
        if(objUIInst != null)
            Destroy(objUIInst);
        TagObject tagObj = descriptions.GetStructByTag(obj.tag);
        objUIInst = Instantiate(objUIPrefab);
        objUIInst.GetComponentInChildren<Image>().gameObject.transform.position = camera.WorldToScreenPoint(obj.transform.position);
        objUIInst.GetComponentInChildren<Text>().text = $"{tagObj.tag}\nCost: ${tagObj.cost:F2}\t R: {tagObj.rVal}\n{tagObj.description}";
    }

    void ObjectSelector()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                camera.GetComponent<CameraController>().SetClickedObject(hit.collider.gameObject);
                CreateObjectUI(hit.collider.gameObject);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ToggleObjectExist(hit.collider.gameObject);
            }
        }
    }

    /**
     * Finds the object in the object dictionary
     * If the object is currently enabled:
     *      set the opacity to the disable opacity specified.
     * else:
     *      set the opacity to 100%.
     * Once done, flip the current state and recalculate R & Cost.
     */ 
    void ToggleObjectExist(GameObject obj)
    {
        bool state = objDict[obj];

        foreach (var meshItem in obj.GetComponentsInChildren<MeshRenderer>())
        {
            foreach (var mat in meshItem.materials)
            {
                Color newCol = mat.color;
                if (state)
                    newCol.a = disableOpacity;
                else
                    newCol.a = 1f;
                mat.color = newCol;
            }
        }
        objDict[obj] = !state;
        CalculateCostAndR();
    }

    void CameraRotator()
    {
        // mouse orbit around object (middle click?)
        // Also, maybe keyboard orbit?
        // Could also add UI controls.
        // Maybe mobile support if we get there.
    }
}
