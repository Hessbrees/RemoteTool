using Newtonsoft.Json.Linq;
using UnityEngine;

public class JsonLabel
{
    public string name;
    public GUIStyle currentStyle;
    public int index;
    public JsonLabelType jsonLabelType;

    public JToken source;
    public JToken target;

    public bool isSelected;
    public bool isAccepted;
    public bool isRemoved;
    public MergeType mergeType;
    public string mergedKey;
    public string lastKey;
    public JsonLabel(string name, JToken source, JToken target)
    {
        this.name = name;
        this.source = source;
        this.target = target;
    }
    public void KeepAsSource()
    {
        mergedKey = source["value"].ToString();
    }
    public void KeepAsTarget()
    {
        mergedKey = target["value"].ToString();
    }
    public override string ToString()
    {
        return $"{name}";
    }
}
public enum JsonLabelType
{
    newLabel,
    changesLabel
}
