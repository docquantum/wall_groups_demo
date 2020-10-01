using System.Collections.Generic;
using UnityEngine;

public struct TagObject
{
    public string tag;
    public float cost;
    public float rVal;
    public string description;

    public TagObject(string tag, float cost, float rVal, string description)
    {
        this.tag = tag;
        this.cost = cost;
        this.rVal = rVal;
        this.description = description;
    }
}

public class Descriptions : MonoBehaviour
{
    private static readonly TagObject defaultTagObj = new TagObject("default", 0f, 0f, "Default Object");
    private static List<TagObject> tagStructs = new List<TagObject>();

    public void Awake()
    {
        InitializeTags();
    }

    private void InitializeTags()
    {
        tagStructs.Add(new TagObject("column", 534.98f, .5f, "This is a concrete column."));
        tagStructs.Add(new TagObject("left_floor", 5642.34f, 10.5f, "This is the left upper floor, made of rebar and asphalt."));
        tagStructs.Add(new TagObject("middle_floor", 1345.46f, 5.3f, "This is the middle upper floor, made of rebar and asphalt."));
        tagStructs.Add(new TagObject("right_floor", 7315.91f, 9.8f, "This is the right upper floor, made of rebar and asphalt."));
    }

    public TagObject GetStructByTag(string tag)
    {
        TagObject err = defaultTagObj;
        foreach (var obj in tagStructs)
        {
            if (obj.tag == tag)
                return obj;
        }
        return defaultTagObj;
    }

    public string GetDescriptionByTag(string tag)
    {
        return GetStructByTag(tag).description;
    }
    public float GetCostByTag(string tag)
    {
        return GetStructByTag(tag).cost;
    }

    public float GetRValByTag(string tag)
    {
        return GetStructByTag(tag).rVal;
    }
}
