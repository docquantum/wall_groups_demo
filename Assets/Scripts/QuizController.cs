using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct SelectAllItem
{
    public string value;
    public bool applies;
    public GameObject uiItem;

    public SelectAllItem(string value, bool applies, GameObject uiItem, GameObject parent)
    {
        this.value = value;
        this.applies = applies;
        uiItem.transform.SetParent(parent.transform);
        uiItem.GetComponent<RectTransform>().sizeDelta = new Vector2 (170, 20); //TODO: Magic Numbers!
        uiItem.GetComponentInChildren<Text>().text = value;
        this.uiItem = uiItem;
    }
}

public class QuizController : MonoBehaviour
{
    public List<SelectAllItem> questionOneItems = new List<SelectAllItem>();

    [SerializeField]
    private GameObject uiItemPrefab = null;

    [SerializeField]
    private Button submitButton = null;

    private bool submitted = false;

    private Rect resolution;

    private void Start()
    {
        resolution.x = Screen.width;
        resolution.y = Screen.height;
        InstantiateQuizElements();
        InvokeRepeating("OnScreenResize", 0, 0.25f);
    }

    private void OnScreenResize()
    {
        if(Screen.width != resolution.x || Screen.height != resolution.y)
        {
            resolution.x = Screen.width;
            resolution.y = Screen.height;
            gameObject.transform.parent.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, Screen.height / 2.5f);
        }
    }

    public void InstantiateQuizElements()
    {
        // points = pixels * 72 / 96
        // pixel = points / (72/Screen.dpi)
        var questionText = InstantiateTextObject("Which of the following are not parts of a typical cavity wall?\nSelect all that apply:", 18, Color.black);
        questionText.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 63); //TODO: Magic numbers!
        questionText.transform.SetParent(gameObject.transform);
        var noteText = InstantiateTextObject("Note: Correct answers are in red", 12, new Color(0.5f, 0f, 0f));
        noteText.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 33); //TODO: Magic numbers!
        noteText.transform.SetParent(gameObject.transform);
        gameObject.transform.parent.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, Screen.height / 2.5f);
        questionOneItems.Add(new SelectAllItem("Reflective surface", true, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Brick Veneer", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Backing", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Wythes", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Carpet", true, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Insulation", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Ties", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Air spacer", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Water barrier", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Wall ties", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Seismic Damper", true, Instantiate(uiItemPrefab), gameObject));
        submitButton.onClick.AddListener(() => TestSelectAllItemQuiz(questionOneItems));
    }

    private GameObject InstantiateTextObject(string text, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(text.Substring(0, 20));
        textObj.AddComponent<RectTransform>();
        textObj.AddComponent<Text>();
        Text textComp = textObj.GetComponent<Text>();
        textComp.text = text;
        textComp.color = color;
        textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textComp.fontSize = fontSize;
        return textObj;
    }

    public void TestSelectAllItemQuiz(List<SelectAllItem> items)
    {
        if (!submitted)
        {
            submitted = true;
            foreach (var item in items)
            {
                if (item.applies)
                {
                    item.uiItem.GetComponentInChildren<Text>().color = Color.red;
                }
            }
            submitButton.GetComponentInChildren<Text>().text = "Reset";
        } else
        {
            submitted = false;
            foreach (var item in items)
            {
                if (item.applies)
                {
                    item.uiItem.GetComponentInChildren<Text>().color = Color.black;
                }
            }
            submitButton.GetComponentInChildren<Text>().text = "Submit";
        }
        
    }
}