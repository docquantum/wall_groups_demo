using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] objects = null;

    [SerializeField]
    private Camera camera = null;

    [SerializeField]
    private GameObject objUIPrefab = null;

    [SerializeField]
    private GameObject mainUIPrefab = null;

    [SerializeField] [Range(0f, 1f)]
    private float disableOpacity = .3f;

    private GameObject objUIInst;
    private GameObject mainUIInst;

    private Dictionary<GameObject, bool> objDict;

    private Dictionary<string, GameObject> combinedDict;

    private HashSet<string> tags;

    public static bool test = false;

    private List<Toggle> toggleList;

    private string lastSelTag = "";

    [SerializeField] [Range(0f, 0.01f)]
    private float mouseSensitivity = 0.005f;
    private Vector3 lastMousePosition = Vector3.zero;

    void Start()
    {
        if (camera == null)
            camera = Camera.main;

        if (mainUIPrefab == null)
            Debug.LogError("No main UI prefab configured!");
        else
        {
            InitializeUI();
        }

        if (objects.Length == 0)
            Debug.LogError("No objects configured!");
        else
            InitializeObjects();

        MainUIController();
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
     * 4. Create combined meshes for use later
     */
    void InitializeObjects()
    {
        tags = new HashSet<string>();
        objDict = new Dictionary<GameObject, bool>();
        foreach (var obj in objects)
        {
            MeshRenderer[] meshComponents = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshItem in meshComponents)
            {
                if (meshItem.gameObject.GetComponent<MeshCollider>() == null)
                    meshItem.gameObject.AddComponent<MeshCollider>();
                objDict.Add(meshItem.gameObject, true);
                if (!tags.Contains(meshItem.tag))
                    tags.Add(meshItem.tag);
            }
        }
        GenerateCombinedMeshGroups();
    }

    void GenerateCombinedMeshGroups()
    {
        combinedDict = new Dictionary<string, GameObject>();
        foreach (var tag in tags)
        {
            GameObject combinedObject = new GameObject($"Combined {tag} Object");
            combinedObject.AddComponent<MeshFilter>();
            combinedObject.AddComponent<MeshRenderer>();
            List<MeshFilter> allMeshFilters = new List<MeshFilter>();
            foreach (var keyval in objDict)
            {
                if (keyval.Key.tag == tag)
                {
                    allMeshFilters.AddRange(keyval.Key.GetComponentsInChildren<MeshFilter>());
                }
            }

            Mesh combinedMesh = MeshCombiner.CombineMeshesFromMeshFilters(allMeshFilters.ToArray(), true, true);
            combinedMesh.name = $"Combined {tag} mesh";
            combinedObject.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
            combinedObject.SetActive(false);
            combinedDict.Add(tag, combinedObject);
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
            else
            {
                ClearSelectedObject();
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
        camera.GetComponent<CameraController>().ClearClickedObject();
        camera.GetComponent<CameraController>().SetClickedObject(combinedDict[obj.tag]);
        lastSelTag = obj.tag;
        MainUIController();
    }

    void ClearSelectedObject()
    {
        camera.GetComponent<CameraController>().ClearClickedObject();
        lastSelTag = "";
        MainUIController();
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
        if(!mainUIInst.GetComponentInChildren<EventSystem>().IsPointerOverGameObject())
            camera.transform.position += camera.transform.forward * Input.mouseScrollDelta.y * 0.4f; //TODO: magic numbers
    }
}
