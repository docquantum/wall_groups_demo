using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public static bool test = false;

    private List<Toggle> toggleList;

    private float totalCost = 0.0f;

    private float totalR = 0.0f;

    private string lastSelTag = "";

    [SerializeField] [Range(0f, 0.01f)]
    private float mouseSensitivity = 0.005f;
    private Vector3 lastMousePosition = Vector3.zero;

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
            //CalculateCostAndR();
        }
        if (objUIPrefab == null)
            Debug.LogError("No object UI prefab configured!");
        if (mainUIPrefab == null)
            Debug.LogError("No main UI prefab configured!");
        else
        {
            InitializeUI();
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
    
    void InitializeUI()
    {
        mainUIInst = Instantiate(mainUIPrefab);
        if(mainUIInst != null)
        {
            toggleList = mainUIInst.GetComponentsInChildren<Toggle>(true).ToList();
            List<Toggle> toRemove = new List<Toggle>();
            foreach (var toggle in toggleList)
            {
                if (toggle.tag != "Untagged")
                    toggle.onValueChanged.AddListener((change) => ToggleGroupExists(toggle.tag));
                else
                    toRemove.Add(toggle);
            }
            toggleList.RemoveAll((t) => toRemove.Contains(t));
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
        if (mainUIInst == null)
            return;

        string builtString = "";
        builtString += "Currently Enabled:";
        foreach(var t in toggleList)
        {
            if (t.isOn)
                builtString += $"\n - {t.tag}";
        }

        builtString += $"\nCurrently Selected:\n {lastSelTag}";
        mainUIInst.GetComponentInChildren<Text>().text = builtString;
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
                SelectAllObjects(hit.collider.gameObject);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ToggleGroupExists(hit.transform.gameObject);
            }
        }
    }

    void SelectAllObjects(GameObject obj)
    {
        camera.GetComponent<CameraController>().DestroyClickedObject();
        GameObject combinedObject = new GameObject($"Combined {obj.tag} Object");
        combinedObject.AddComponent<MeshFilter>();
        List<MeshFilter> allMeshFilters = new List<MeshFilter>();
        foreach (var keyval in objDict)
        {
            if (keyval.Key.tag == obj.tag)
            {
                allMeshFilters.AddRange(keyval.Key.GetComponentsInChildren<MeshFilter>());
            }
        }
       
        CombineInstance[] combine = new CombineInstance[allMeshFilters.Count];
        for (int i = 0; i < allMeshFilters.Count; i++)
        {
            combine[i].mesh = allMeshFilters[i].mesh;
            combine[i].transform = allMeshFilters[i].transform.localToWorldMatrix;
        }

        combinedObject.GetComponentInChildren<MeshFilter>().mesh.CombineMeshes(combine);
        combinedObject.SetActive(false);

        camera.GetComponent<CameraController>().SetClickedObject(combinedObject);
        lastSelTag = obj.tag;
        MainUIController();
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

    void ToggleGroupExists(string tag)
    {
        var tempList = new List<GameObject>();
        foreach (var keyval in objDict)
        {
            if (keyval.Key.tag == tag)
                tempList.Add(keyval.Key);
        }
        foreach (var obj in tempList)
        {
            var newState = !objDict[obj];
            obj.SetActive(newState);
            objDict[obj] = newState;
        }
        MainUIController();
    }

    void ToggleGroupExists(GameObject obj)
    {
        foreach (var toggle in toggleList)
        {
            if (toggle.tag == obj.tag)
                toggle.isOn = !toggle.isOn;
        }
    }

    void CameraRotator()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                camera.transform.RotateAround(Vector3.zero, new Vector3(delta.y * mouseSensitivity, delta.x * mouseSensitivity), delta.magnitude*0.5f); //TODO: magic numbers
                lastMousePosition = Input.mousePosition;
            }
            else
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                camera.transform.Translate(-delta.x * mouseSensitivity, -delta.y * mouseSensitivity, 0);
                lastMousePosition = Input.mousePosition;
            }

        }
        camera.transform.position += camera.transform.forward * Input.mouseScrollDelta.y * 0.4f; //TODO: magic numbers
    }
}
