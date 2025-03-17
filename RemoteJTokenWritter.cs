using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoteJTokenWritter
{
    RemoteMergeStyleSetup styleData;

    JToken mergedToSource;
    JToken mergedToTarget;

    public JToken MergedToSource => mergedToSource;
    public JToken MergedToTarget => mergedToTarget;

    string jsonPath;

    public RemoteJTokenWritter(RemoteMergeStyleSetup styleData)
    {
        this.styleData = styleData;
    }
    private void WriteJToken(JSonText jSonText)
    {
        if (jSonText.source.ToString() == jSonText.target.ToString())
        {
            GUILayout.TextArea(AddSpace(jSonText.currentSpace) + jSonText.source.ToString(), styleData.defaultTextArea);
        }
        else
        {
            if (jSonText.isTarget)
            {
                GUILayout.TextArea(AddSpace(jSonText.currentSpace) + jSonText.target.ToString(), styleData.changedTextArea);
            }
            else
            {
                GUILayout.TextArea(AddSpace(jSonText.currentSpace) + jSonText.source.ToString(), styleData.changedTextArea);
            }
        }
    }
    private void WriteJProperty(JSonText jSonText)
    {
        JProperty sourceProperty = (JProperty)jSonText.source;
        JProperty targetProperty = (JProperty)jSonText.target;

        if (sourceProperty.Value.ToString() == targetProperty.Value.ToString())
        {
            string text = AddSpace(jSonText.currentSpace) + jSonText.source.ToString();
            text = text.Replace(System.Environment.NewLine, "\n" + AddSpace(jSonText.currentSpace));

            GUILayout.TextArea(text, styleData.defaultTextArea);
        }
        else
        {
            if (jSonText.isTarget)
            {
                string text = AddSpace(jSonText.currentSpace) + jSonText.target.ToString();
                text = text.Replace(System.Environment.NewLine, "\n" + AddSpace(jSonText.currentSpace));
                GUILayout.TextArea(text, styleData.changedTextArea);
            }
            else
            {
                string text = AddSpace(jSonText.currentSpace) + jSonText.source.ToString();
                text = text.Replace(System.Environment.NewLine, "\n" + AddSpace(jSonText.currentSpace));
                GUILayout.TextArea(text, styleData.changedTextArea);
            }
        }
    }
    private void WriteJProperty(JProperty jProperty, int space)
    {
        if (jProperty == null) return;

        GUILayout.TextArea(AddSpace(space) + jProperty.Value.ToString(), styleData.changedTextArea);
    }

    private void CheckDeepestJObject(JSonText jSonText)
    {
        if (jSonText.isTarget)
        {
            DeepestJObjectLoop(jSonText,
                jSonText.target.Children().ToList(),
                jSonText.source.Children().ToList());
        }
        else
        {
            DeepestJObjectLoop(jSonText,
                jSonText.source.Children().ToList(),
                jSonText.target.Children().ToList());
        }
    }

    private void DeepestJObjectLoop(JSonText jSonText, List<JToken> checkList, List<JToken> compareList)
    {
        JProperty currentJProperty = null;

        for (int i = 0; i < checkList.Count; i++)
        {
            if (checkList[i].Type == JTokenType.Property)
            {
                jsonPath += "." + ((JProperty)checkList[i]).Name.ToString();
            }

            if (checkList[i].Type == JTokenType.Property && currentJProperty == null)
                currentJProperty = (JProperty)checkList[i];

            for (int j = 0; j < compareList.Count; j++)
            {
                if (checkList[i].Type != compareList[j].Type) continue;

                if (!TryCreateJPropertyStruct(checkList[i], compareList[j], ref currentJProperty)) continue;

                JSonText nestedJson;

                if (jSonText.isTarget)
                {
                    nestedJson = CreateNestedJSonText(
                            jSonText,
                            compareList[j],
                            checkList[i]);
                }
                else
                {
                    nestedJson = CreateNestedJSonText(
                            jSonText,
                            checkList[i],
                            compareList[j]);
                }

                if (jSonText.value.Type == JTokenType.Array)
                {
                    if (!jSonText.currentArrayWrittenList.Contains(checkList[i].ToString()))
                    {
                        if (i >= compareList.Count)
                        {
                            AddToMergedKey(checkList[i], jSonText.isTarget);
                            GUILayout.TextArea(AddSpace(jSonText.currentSpace) + checkList[i].ToString(), styleData.changedTextArea);
                            jSonText.currentArrayWrittenList.Add(checkList[i].ToString());
                        }
                        else if (i == j)
                        {
                            jSonText.currentArrayWrittenList.Add(checkList[i].ToString());

                            if (nestedJson.source.ToString() == nestedJson.target.ToString())
                            {
                                GUILayout.TextArea(AddSpace(nestedJson.currentSpace) + nestedJson.source.ToString(), styleData.defaultTextArea);
                            }
                            else
                            {
                                CheckJToken(nestedJson, true);

                                if(jSonText.isTarget)
                                {
                                    AddToMergedKey(nestedJson.target, jSonText.isTarget);
                                }
                                else
                                {
                                    AddToMergedKey(nestedJson.source, jSonText.isTarget);
                                }
                                
                            }
                        }
                    }
                }
                else
                {
                    CheckJToken(nestedJson, false);
                }
            }

            if (jSonText.value.Type == JTokenType.Array)
            {
                if (!jSonText.currentArrayWrittenList.Contains(checkList[i].ToString()))
                {
                    GUILayout.TextArea(AddSpace(jSonText.currentSpace) + checkList[i].ToString(), styleData.changedTextArea);
                    jSonText.currentArrayWrittenList.Add(checkList[i].ToString());

                }
            }

            WriteCurrentJProperty(ref currentJProperty, jSonText.currentSpace);

            if (checkList[i].Type == JTokenType.Property)
            {
                jsonPath = jsonPath.Remove(jsonPath.Length - ((JProperty)checkList[i]).Name.ToString().Length -1);
            }
        }

    }

    private void CheckDeepestJProperty(JSonText jSonText)
    {
        JProperty jProperty = jSonText.value as JProperty;

        if (jSonText.isTarget)
        {
            DeepestJPropertyLoop(jSonText,
                jSonText.target.Children().ToList(),
                jSonText.source.Children().ToList());
        }
        else
        {
            DeepestJPropertyLoop(jSonText,
                jSonText.source.Children().ToList(),
                jSonText.target.Children().ToList());
        }
    }


    // Create if jproperties are equals => write new property with default style
    private bool TryCreateJPropertyStruct(JToken check, JToken compare, ref JProperty currentJProperty)
    {
        if (check.Type == JTokenType.Property &&
            compare.Type == JTokenType.Property)
        {
            if (IsEqualsProperty(check, compare))
            {
                currentJProperty = null;

                return true;
            }

            return false;
        }

        return true;
    }
    private bool IsEqualsProperty(JToken source, JToken target)
    {
        if (source is JProperty sourceProp)
        {
            if (target is JProperty targetProp)
            {
                if (sourceProp.Name == targetProp.Name) return true;
            }
        }

        return false;
    }
    private void WriteCurrentJProperty(ref JProperty currentJProperty, int space)
    {
        WriteJProperty(currentJProperty, space);

        currentJProperty = null;
    }
    private void DeepestJPropertyLoop(JSonText jSonText, List<JToken> checkList, List<JToken> compareList)
    {
        JProperty currentJProperty = null;

        for (int i = 0; i < checkList.Count; i++)
        {
            if (checkList[i].Type == JTokenType.Property)
            {
                jsonPath += "." + ((JProperty)checkList[i]).Name.ToString();
            }

            if (checkList[i].Type == JTokenType.Property && currentJProperty == null)
                currentJProperty = (JProperty)checkList[i];

            for (int j = 0; j < compareList.Count; j++)
            {
                if (checkList[i].Type != compareList[j].Type) continue;

                if (!TryCreateJPropertyStruct(checkList[i], compareList[j], ref currentJProperty)) continue;

                JSonText nestedJson;

                if (jSonText.isTarget)
                {
                    nestedJson = CreateNestedJSonText(
                    jSonText,
                    compareList[j],
                    checkList[i]);
                }
                else
                {
                    nestedJson = CreateNestedJSonText(
                    jSonText,
                    checkList[i],
                    compareList[j]);
                }

                CheckJProperty(nestedJson);
            }

            WriteCurrentJProperty(ref currentJProperty, jSonText.currentSpace);

            if (checkList[i].Type == JTokenType.Property)
            {
                jsonPath = jsonPath.Remove(jsonPath.Length - ((JProperty)checkList[i]).Name.ToString().Length -1);
            }
        }
    }

    // check initial token in key
    // complete check type map
    // initialJToken > JArray    > JToken
    //
    //               > JObject   > JArray
    //                           > JObject
    //                           > JProperty
    //               // Check property value and write or check childrens properties
    //               > JProperty > JArray
    //                           > JObject
    //                           > String/Int/Bool - write parent JProperty
    //
    //               > String/Int/Bool            - write initial token
    private void CheckJToken(JSonText jSonText, bool isArray)
    {
        if (jSonText.value.Type == JTokenType.Array)
        {
            GUILayout.TextArea(AddSpace(jSonText.currentSpace - 4) + "[", styleData.defaultTextArea);

            CheckDeepestJObject(jSonText);

            GUILayout.TextArea(AddSpace(jSonText.currentSpace - 4) + "]", styleData.defaultTextArea);
        }
        else if (jSonText.value.Type == JTokenType.Object)
        {
            jSonText.currentSpace += 4;

            GUILayout.TextArea(AddSpace(jSonText.currentSpace - 4) + "{", styleData.defaultTextArea);

            CheckDeepestJObject(jSonText);

            GUILayout.TextArea(AddSpace(jSonText.currentSpace - 4) + "}", styleData.defaultTextArea);
        }
        else if (jSonText.value.Type == JTokenType.String || jSonText.value.Type == JTokenType.Integer || jSonText.value.Type == JTokenType.Boolean)
        {
            if (!isArray)
            {
                WriteJToken(jSonText);
                return;
            }
        }
        else if (jSonText.value.Type == JTokenType.Property)
        {
            if (((JProperty)jSonText.source).Value.ToString() == ((JProperty)jSonText.target).Value.ToString())
            {
                WriteJProperty(jSonText);
            }
            else
            {
                if (IsChildrenValue(jSonText.value.Children().First()))
                {
                    WriteJProperty(jSonText);

                    CheckDeepestJProperty(jSonText);
                }
                else
                {
                    GUILayout.TextArea(AddSpace(jSonText.currentSpace) + "\"" + ((JProperty)jSonText.value).Name + "\"" + ":", styleData.defaultTextArea);

                    CheckDeepestJProperty(jSonText);
                }
            }
            //currentPropertiesLoop.(((JProperty)jSonText.value).Value.ToString());
        }
    }

    private bool IsChildrenValue(JToken jToken)
    {
        if (jToken.Type == JTokenType.String || jToken.Type == JTokenType.Integer || jToken.Type == JTokenType.Boolean)
        {
            return true;
        }
        return false;
    }
    private string AddSpace(int space)
    {
        if (space <= 0) return "";

        string text = "";

        for (int i = 0; i < space; i++)
        {
            text += " ";
        }

        return text;
    }
    private void CheckJProperty(JSonText jSonText)
    {
        if (jSonText.value.Type == JTokenType.Array)
        {
            GUILayout.TextArea(AddSpace(jSonText.currentSpace - 4) + "[", styleData.defaultTextArea);
            CheckDeepestJObject(jSonText);
            GUILayout.TextArea(AddSpace(jSonText.currentSpace - 4) + "]", styleData.defaultTextArea);
        }
        else if (jSonText.value.Type == JTokenType.Object)
        {
            jSonText.currentSpace += 4;

            GUILayout.TextArea(AddSpace(jSonText.currentSpace - 4) + "{", styleData.defaultTextArea);
            CheckDeepestJObject(jSonText);
            GUILayout.TextArea(AddSpace(jSonText.currentSpace - 4) + "}", styleData.defaultTextArea);
        }
    }

    private JSonText CreateNestedJSonText(JSonText jSonText, JToken source, JToken target)
    {
        if (jSonText.isTarget)
        {
            return new(source, target, target, true, jSonText.currentSpace);
        }
        else
        {
            return new(source, target, source, false, jSonText.currentSpace);
        }
    }
    private void AddToMergedKey(JToken jToken, bool isTarget)
    {
        if (isTarget)
        {
            JToken current = mergedToSource.SelectToken(jsonPath);

            if (current == null)
            {
                Debug.Log(jsonPath);
                return;
            }

            if (!current.Contains(jToken))
                    current.Last.AddAfterSelf(jToken);
        }
        else
        {
            JToken current = mergedToTarget.SelectToken(jsonPath);

            if(current == null)
            {
                Debug.Log(jsonPath);
                return;
            }

            if (!current.Contains(jToken))
                    current.Last.AddAfterSelf(jToken);
        }
    }
    public void CreateColoredTextAreaSource(JsonLabel jsonLabel)
    {
        jsonPath = "$";
        mergedToTarget = jsonLabel.target["value"].DeepClone();

        JSonText jSonText = new(
            jsonLabel.source["value"],
            jsonLabel.target["value"],
            jsonLabel.source["value"], false, 0);

        CheckJToken(jSonText, false);

        int extraSpace = AddExtraScrollLines(jsonLabel);

        if (extraSpace < 0)
        {
            AddExtraLines(Mathf.Abs(extraSpace));
        }
    }
    private void AddExtraLines(int amount)
    {
        string text ="";

        for(int i = 1; i < amount; i++)
        {
            text += "\n";
        }

        GUILayout.TextArea(text, styleData.defaultTextArea);
    }
    public void CreateColoredTextAreaTarget(JsonLabel jsonLabel)
    {
        jsonPath = "$";
        mergedToSource = jsonLabel.source["value"].DeepClone();

        JSonText jSonText = new(
            jsonLabel.source["value"],
            jsonLabel.target["value"],
            jsonLabel.target["value"], true, 0);

        CheckJToken(jSonText, false);

        int extraSpace = AddExtraScrollLines(jsonLabel);

        if (extraSpace > 0)
        {
            AddExtraLines(extraSpace);
        }
    }
    private int AddExtraScrollLines(JsonLabel jsonLabel)
    {
        if (jsonLabel.source == null || jsonLabel.target == null) return 0;

        int sourceLenght = jsonLabel.source.ToString().Split('\n').Length;
        int targetLenght = jsonLabel.target.ToString().Split('\n').Length;

        return sourceLenght - targetLenght;
    }
    // check deepest values in source and target
    // value is source or target depends of list
    private struct JSonText
    {
        public JToken source;
        public JToken target;
        public JToken value;
        public bool isTarget;
        public int currentSpace;

        public List<string> currentArrayWrittenList;

        public bool isArray;

        public JSonText(JToken source, JToken target, JToken value, bool isTarget, int currentSpace)
        {
            this.source = source;
            this.target = target;
            this.value = value;
            this.isTarget = isTarget;
            this.currentSpace = currentSpace;

            currentArrayWrittenList = new();
            isArray = false;
        }
    }
}
