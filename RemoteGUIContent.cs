using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
public class RemoteGUIContent
{
    RemoteMergeStyleSetup styleData = new();
    RemoteJTokenWritter remoteJTokenWritter;
    RemoteMergeInputs inputs;
    RemoteMergeWindow window;

    Vector2 scrollPosNew;
    Vector2 scrollPosChanges;
    Vector2 scrollPosSourceTarget;
    Vector2 scrollPosMerged;

    List<JToken> sourceJTokenList = new();
    List<JToken> targetJTokenList = new();

    List<JsonLabel> jTokenLabelListNew = new();
    List<JsonLabel> jTokenLabelListChanges = new();

    public List<JsonLabel> JTokenLabelListNew => jTokenLabelListNew;
    public List<JsonLabel> JTokenLabelListChanges => jTokenLabelListChanges;

    private bool showMergedWindow = false;
    public bool ShowMergedWindow => showMergedWindow;
    MergeType mergeType;
    bool isRemoved;
    public RemoteGUIContent(List<JToken> sourceJTokenList, List<JToken> targetJTokenList, RemoteMergeWindow window)
    {
        this.window = window;
        this.sourceJTokenList = sourceJTokenList;
        this.targetJTokenList = targetJTokenList;

        styleData.SetupStyles();
        remoteJTokenWritter = new(styleData);
        inputs = new RemoteMergeInputs(this);

        SetupJTokenList();
    }

    // setup lists with new and changes labels
    private void SetupJTokenList()
    {
        int newIndex = 0;
        int changesIndex = 0;

        foreach (var source in sourceJTokenList)
        {
            bool isNew = true;

            foreach (var target in targetJTokenList)
            {
                if (source["key"].ToString() == target["key"].ToString())
                {

                    if (source["value"].ToString() != target["value"].ToString())
                    {
                        JsonLabel jsonLabel = new JsonLabel(source["key"].ToString(), source, target);

                        jsonLabel.currentStyle = styleData.defaultLabelStyle;
                        jsonLabel.index = changesIndex;
                        jsonLabel.jsonLabelType = JsonLabelType.changesLabel;
                        jTokenLabelListChanges.Add(jsonLabel);
                        changesIndex++;
                    }

                    isNew = false;
                }
            }

            if (isNew)
            {
                JsonLabel jsonLabel = new JsonLabel(source["key"].ToString(), source, null);

                jsonLabel.currentStyle = styleData.defaultLabelStyle;
                jsonLabel.index = newIndex;
                jsonLabel.jsonLabelType = JsonLabelType.newLabel;
                jTokenLabelListNew.Add(jsonLabel);
                newIndex++;
            }
        }
    }
    public void ShowMergedWindowChange()
    {
        showMergedWindow = !showMergedWindow;

        window.Repaint();
    }
    public void WindowButtons()
    {
        if (IsAnySelected())
        {
            GUILayout.BeginArea(new Rect(EditorGUIUtility.currentViewWidth - 1200, 950, 500, 60));

            if (GUILayout.Button("Merged window (q)", styleData.defaultButtonStyle, GUILayout.ExpandWidth(false)))
            {
                ShowMergedWindowChange();
            }

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(EditorGUIUtility.currentViewWidth - 1000, 950, 600, 60));
            GUILayout.BeginHorizontal();

            foreach (var type in Enum.GetValues(typeof(MergeType)))
            {
                if ((MergeType)type == MergeType.none) continue;

                if ((MergeType)type == mergeType)
                {
                    if (GUILayout.Button(type.ToString() + GetButtonText((MergeType)type), styleData.selectedButtonStyle))
                    {
                        ChangeClickedButton((MergeType)type);
                    }
                }
                else
                {
                    if (GUILayout.Button(type.ToString() + GetButtonText((MergeType)type), styleData.defaultButtonStyle))
                    {
                        ChangeClickedButton((MergeType)type);
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        else if (IsAnySelectedNew())
        {
            WindowButtonNew();
        }

        WindowButtonsAllLabels();
        CommitButton();
    }
    private void WindowButtonNew()
    {
        GUILayout.BeginArea(new Rect(EditorGUIUtility.currentViewWidth - 1000, 950, 600, 60));
        GUILayout.BeginHorizontal();

        if (isRemoved)
        {
            if (GUILayout.Button("Enable key (w)", styleData.defaultButtonStyle))
            {
                EnableCurrentKey();
            }
            else if (GUILayout.Button("Remove key (e)", styleData.selectedButtonStyle)) { }

        }
        else
        {
            if (GUILayout.Button("Enable key (w)", styleData.selectedButtonStyle)) { }
            else if (GUILayout.Button("Remove key (e)", styleData.defaultButtonStyle))
            {
                RemoveCurrentKey();
            }
        }


        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    private void CommitButton()
    {
        GUILayout.BeginArea(new Rect(EditorGUIUtility.currentViewWidth - 300, 950, 150, 200));

        if (IsAllAccepted())
        {
            if (GUILayout.Button("Commit to target enviroment", styleData.defaultCommitButtonStyle))
            {
                window.Commit();
            }
        }
        else
        {
            if (GUILayout.Button("Commit to target enviroment", styleData.disabledCommitButtonStyle))
            {

            }
        }

        GUILayout.EndArea();
    }
    private void WindowButtonsAllLabels()
    {
        GUILayout.BeginArea(new Rect(EditorGUIUtility.currentViewWidth - 1000, 970, 600, 60));
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("keepAllSource" + GetButtonTextAll(MergeType.keepSource), styleData.defaultButtonStyle))
        {
            ChangeClickedButtonAll(MergeType.keepSource);
        }

        if (GUILayout.Button("keepAllTarget" + GetButtonTextAll(MergeType.keepTarget), styleData.defaultButtonStyle))
        {
            ChangeClickedButtonAll(MergeType.keepTarget);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    public void ProcessEvents(Event currentEvent)
    {
        inputs.ProcessEvents(currentEvent);
    }
    public void RemoveCurrentKey()
    {
        isRemoved = true;

        GetCurrentSelectedNew().isRemoved = true;

        window.Repaint();
    }
    public void EnableCurrentKey()
    {
        isRemoved = false;

        GetCurrentSelectedNew().isRemoved = false;

        window.Repaint();
    }
    public void ChangeClickedButton(MergeType type)
    {
        GetCurrentSelected().mergeType = type;
        GetCurrentSelected().isAccepted = true;

        mergeType = type;

        window.Repaint();
    }
    public void ChangeClickedButtonAll(MergeType type)
    {
        foreach (var label in jTokenLabelListChanges)
        {
            label.mergeType = type;
            label.isAccepted = true;

            if(type == MergeType.keepSource)
            {
                label.KeepAsSource();
            }
            else if(type == MergeType.keepTarget)
            {
                label.KeepAsTarget();
            }
        }

        mergeType = type;

        RefreshSelected();

        window.Repaint();
    }
    private string GetButtonText(MergeType type)
    {
        switch (type)
        {
            case MergeType.none:
                return "";
            case MergeType.keepSource:
                return " (w)";
            case MergeType.keepTarget:
                return " (e)";
            case MergeType.tryMergeToSource:
                return " (r)";
            case MergeType.tryMergeToTarget:
                return " (t)";
            case MergeType.keepEdited:
                return " (y)";
            default:
                return "";
        }
    }
    private string GetButtonTextAll(MergeType type)
    {
        switch (type)
        {
            case MergeType.none:
                return "";
            case MergeType.keepSource:
                return " (f)";
            case MergeType.keepTarget:
                return " (g)";
            default:
                return "";
        }
    }
    public void CreateNewJsonList()
    {
        GUILayout.BeginArea(new Rect(20, 20, styleData.backgroundLabelWidth + 20 * styleData.backgroundLabelBorder, 2000));

        GUILayout.Label("New keys: " + jTokenLabelListNew.Count);

        scrollPosNew = EditorGUILayout.BeginScrollView(scrollPosNew, false, false,
            GUILayout.Width(styleData.backgroundLabelWidth), GUILayout.Height(900));

        foreach (var jsonLabel in jTokenLabelListNew)
        {
            CreateJsonLabel(jsonLabel);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    public void CreateChangesJsonList()
    {
        GUILayout.BeginArea(new Rect(300, 20, styleData.backgroundLabelWidth + 20 * styleData.backgroundLabelBorder, 2000));

        GUILayout.Label("Changed keys: " + jTokenLabelListChanges.Count);

        scrollPosChanges = EditorGUILayout.BeginScrollView(scrollPosChanges, false, false,
            GUILayout.Width(styleData.backgroundLabelWidth), GUILayout.Height(900));

        foreach (var jsonLabel in jTokenLabelListChanges)
        {
            CreateJsonLabel(jsonLabel);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    private void CreateJsonLabel(JsonLabel jsonLabel)
    {
        if (GUILayout.Button(jsonLabel.ToString(), jsonLabel.currentStyle))
        {
            SelectJsonLabel(jsonLabel);
        }
    }

    private void RefreshSelected()
    {
        JsonLabel current = GetCurrentSelected();

        if(current == null)
        {
            ResetJsonNewLabelStyle();
            ResetJsonChangesLabelStyle();
        }
        else
        {
            SelectJsonLabel(current);
        }
    }

    // reset all selected entity on lists and find new on list
    public void SelectJsonLabel(JsonLabel jsonLabel)
    {
        ResetJsonNewLabelStyle();
        ResetJsonChangesLabelStyle();

        if (jsonLabel.currentStyle == styleData.defaultLabelStyle ||
            jsonLabel.currentStyle == styleData.acceptedLabelStyle || 
            jsonLabel.currentStyle == styleData.removedLabelStyle)
        {
            jsonLabel.currentStyle = styleData.selectedLabelStyle;
            jsonLabel.isSelected = true;
        }
        else if (jsonLabel.currentStyle == styleData.selectedLabelStyle)
        {
            if (jsonLabel.isAccepted)
            {
                jsonLabel.currentStyle = styleData.acceptedLabelStyle;
            }
            else if(jsonLabel.isRemoved)
            {
                jsonLabel.currentStyle = styleData.removedLabelStyle;
            }
            else
            {
                jsonLabel.currentStyle = styleData.defaultLabelStyle;
            }

            jsonLabel.isSelected = false;
        }

        mergeType = jsonLabel.mergeType;
        isRemoved = jsonLabel.isRemoved;

        window.Repaint();
    }
    public void SelectJsonLabelNew(int index)
    {
        if (index < 0 || index >= jTokenLabelListNew.Count) return;

        SelectJsonLabel(jTokenLabelListNew[index]);
    }
    public void SelectJsonLabelChanges(int index)
    {
        if (index < 0 || index >= jTokenLabelListChanges.Count) return;

        SelectJsonLabel(jTokenLabelListChanges[index]);
    }
    public bool IsAnySelected()
    {
        foreach (var jsonLabel in jTokenLabelListNew)
        {
            if (jsonLabel.isSelected) return false;
        }

        foreach (var jsonLabel in jTokenLabelListChanges)
        {
            if (jsonLabel.isSelected) return true;
        }

        return false;
    }
    public bool IsAnySelectedNew()
    {
        foreach (var jsonLabel in jTokenLabelListNew)
        {
            if (jsonLabel.isSelected) return true;
        }

        return false;
    }
    public bool IsAllAccepted()
    {
        foreach (var jsonLabel in jTokenLabelListChanges)
        {
            if (!jsonLabel.isAccepted) return false;
        }

        return true;
    }
    public JsonLabel GetCurrentSelectedFromAll()
    {
        foreach (var jsonLabel in jTokenLabelListNew)
        {
            if (jsonLabel.isSelected) return jsonLabel;
        }

        foreach (var jsonLabel in jTokenLabelListChanges)
        {
            if (jsonLabel.isSelected) return jsonLabel;
        }

        return null;
    }
    private JsonLabel GetCurrentSelectedNew()
    {
        foreach (var jsonLabel in jTokenLabelListNew)
        {
            if (jsonLabel.isSelected) return jsonLabel;
        }

        return null;
    }
    private JsonLabel GetCurrentSelected()
    {
        foreach (var jsonLabel in jTokenLabelListChanges)
        {
            if (jsonLabel.isSelected) return jsonLabel;
        }

        return null;
    }
    private void ResetJsonNewLabelStyle()
    {
        foreach (var jsonLabel in jTokenLabelListNew)
        {
            if (jsonLabel.isRemoved)
            {
                jsonLabel.currentStyle = styleData.removedLabelStyle;
            }
            else
            {
                jsonLabel.currentStyle = styleData.defaultLabelStyle;
            }

            jsonLabel.isSelected = false;
        }
    }
    private void ResetJsonChangesLabelStyle()
    {
        foreach (var jsonLabel in jTokenLabelListChanges)
        {
            if (jsonLabel.isAccepted)
            {
                jsonLabel.currentStyle = styleData.acceptedLabelStyle;
            }
            else
            {
                jsonLabel.currentStyle = styleData.defaultLabelStyle;
            }

            jsonLabel.isSelected = false;
        }
    }

    private void ShowTextAreaSource(JsonLabel jsonLabel, bool isTargetNull)
    {
        if (showMergedWindow)
        {
            GUILayout.BeginArea(new Rect(20, 20, 600, 2000));
        }
        else
        {
            GUILayout.BeginArea(new Rect(600, 20, 600, 2000));
        }

        GUILayout.Label("Source key ");

        scrollPosSourceTarget = EditorGUILayout.BeginScrollView(scrollPosSourceTarget, false, false,
        GUILayout.Width(600), GUILayout.Height(900));

        if (isTargetNull)
        {
            GUILayout.TextArea(jsonLabel.source["value"].ToString(), styleData.mergedTextArea);
        }
        else
        {
            remoteJTokenWritter.CreateColoredTextAreaSource(jsonLabel);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    private void ShowTextAreaMerged(JsonLabel jsonLabel)
    {
        GUILayout.BeginArea(new Rect(650, 20, 600, 2000));

        GUILayout.Label("Merged key ");

        scrollPosMerged = EditorGUILayout.BeginScrollView(scrollPosMerged, false, false,
        GUILayout.Width(600), GUILayout.Height(900));

        switch (jsonLabel.mergeType)
        {
            case MergeType.none:
                break;
            case MergeType.keepSource:
                jsonLabel.mergedKey = GUILayout.TextArea(jsonLabel.source["value"].ToString(), styleData.mergedTextArea);
                jsonLabel.lastKey = jsonLabel.source["value"].ToString();
                break;
            case MergeType.keepTarget:
                jsonLabel.mergedKey = GUILayout.TextArea(jsonLabel.target["value"].ToString(), styleData.mergedTextArea);
                jsonLabel.lastKey = jsonLabel.target["value"].ToString();
                break;
            case MergeType.tryMergeToSource:
                jsonLabel.mergedKey = GUILayout.TextArea(remoteJTokenWritter.MergedToSource.ToString(), styleData.mergedTextArea);
                jsonLabel.lastKey = remoteJTokenWritter.MergedToSource.ToString();
                break;
            case MergeType.tryMergeToTarget:
                jsonLabel.mergedKey = GUILayout.TextArea(remoteJTokenWritter.MergedToTarget.ToString(), styleData.mergedTextArea);
                jsonLabel.lastKey = remoteJTokenWritter.MergedToTarget.ToString();
                break;
            case MergeType.keepEdited:
                jsonLabel.mergedKey = GUILayout.TextArea(jsonLabel.mergedKey, styleData.mergedTextArea);
                break;
        }

        if (jsonLabel.mergeType != MergeType.keepEdited && jsonLabel.mergeType != MergeType.none)
        {
            if (jsonLabel.mergedKey != jsonLabel.lastKey)
            {
                jsonLabel.mergeType = MergeType.keepEdited;
                mergeType = MergeType.keepEdited;
            }
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    private void ShowTextAreaTarget(JsonLabel jsonLabel)
    {
        GUILayout.BeginArea(new Rect(1280, 20, 600, 2500));

        GUILayout.Label("Target key ");

        scrollPosSourceTarget = EditorGUILayout.BeginScrollView(scrollPosSourceTarget, false, false,
        GUILayout.Width(600), GUILayout.Height(900));

        remoteJTokenWritter.CreateColoredTextAreaTarget(jsonLabel);
        //GUILayout.TextArea(jsonLabel.target["value"].ToString());

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    public void CreateTextAreas()
    {
        foreach (var jsonLabel in jTokenLabelListNew)
        {
            if (jsonLabel.isSelected)
            {
                ShowTextAreaSource(jsonLabel, true);

                return;
            }
        }

        foreach (var jsonLabel in jTokenLabelListChanges)
        {
            if (jsonLabel.isSelected)
            {
                ShowTextAreaSource(jsonLabel, false);

                if (jsonLabel.target == null) break;

                ShowTextAreaTarget(jsonLabel);

                if (showMergedWindow)
                {
                    ShowTextAreaMerged(jsonLabel);
                }
                break;
            }
        }
    }

}

public enum MergeType
{
    none,
    keepSource,
    keepTarget,
    tryMergeToSource,
    tryMergeToTarget,
    keepEdited
}
