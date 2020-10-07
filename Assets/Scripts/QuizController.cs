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
        uiItem.GetComponent<RectTransform>().sizeDelta = new Vector2 (150, 20);
        uiItem.GetComponentInChildren<Text>().text = value;
        this.uiItem = uiItem;
    }
}

public class QuizController : MonoBehaviour
{
    public List<SelectAllItem> questionOneItems = new List<SelectAllItem>();

    [SerializeField]
    private GameObject uiItemPrefab;

    [SerializeField]
    private Button submitButton;

    private void Start()
    {
        InstantiateQuizElements();
    }

    public void InstantiateQuizElements()
    {
        var textComp = InstantiateTextObject("Which of the following are not parts of a typical cavity wall? Select all that apply:");
        textComp.transform.SetParent(gameObject.transform);
        //print(gameObject.GetComponent<RectTransform>().rect.width);
        textComp.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 120);
        questionOneItems.Add(new SelectAllItem("Brick Veneer", true, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Backing", true, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Wythes", true, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Gypsum board", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Carpet", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Reflective surface", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Insulation", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Ties", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Air spacer", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Water barrier", false, Instantiate(uiItemPrefab), gameObject));
        questionOneItems.Add(new SelectAllItem("Wall ties", false, Instantiate(uiItemPrefab), gameObject));
        submitButton.onClick.AddListener(() => TestSelectAllItemQuiz(questionOneItems));
    }

    private GameObject InstantiateTextObject(string text)
    {
        GameObject textObj = new GameObject(text.Substring(0, 10));
        textObj.AddComponent<RectTransform>();
        textObj.AddComponent<Text>();
        Text textComp = textObj.GetComponent<Text>();
        textComp.text = text;
        textComp.color = Color.black;
        textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textComp.fontSize = 18;
        return textObj;
    }

    public void TestSelectAllItemQuiz(List<SelectAllItem> items)
    {
        foreach(var item in items)
        {
            if(item.uiItem.GetComponent<Toggle>().isOn != item.applies)
            {
                item.uiItem.GetComponentInChildren<Text>().color = Color.red;
            }
        }
    }
}