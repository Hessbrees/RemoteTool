using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
public class RemoteMergeWindow : EditorWindow
{
    RemoteToolService remoteToolService = new();
    RemoteGUIContent remoteGUIContent;

    List<JToken> sourceJTokenList = new();
    List<JToken> targetJTokenList = new();

    bool isLoadingFinished;
    bool isNewKeySend;
    string animatingText = "Loading...";
    int currentText = 0;

    EnviromentType sourceEnviromentType;
    EnviromentType targetEnviromentType;
    public async void Init(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType)
    {
        isLoadingFinished = false;

        this.sourceEnviromentType = sourceEnviromentType;
        this.targetEnviromentType = targetEnviromentType;

        EditorCoroutineUtility.StartCoroutine(ChangeText(), this);

        await remoteToolService.LoadConfigData(sourceEnviromentType, targetEnviromentType);

        isLoadingFinished = true;

        Setup();

        remoteGUIContent = new(sourceJTokenList, targetJTokenList,this);

        Repaint();
    }
    private void OnGUI()
    {
        if(!isLoadingFinished) AnimateLoading();

        if (isNewKeySend) return;

        if (remoteGUIContent == null) return;

        if(!remoteGUIContent.ShowMergedWindow)
        {
            remoteGUIContent.CreateChangesJsonList();
            remoteGUIContent.CreateNewJsonList();
        }

        remoteGUIContent.CreateTextAreas();
        remoteGUIContent.WindowButtons();
        
        remoteGUIContent.ProcessEvents(Event.current);
    }
    private void Setup()
    {
        TransformConfigToJsonList();
    }

    private void TransformConfigToJsonList()
    {
        JToken sourceValue = remoteToolService.GetLocationType(LocationType.source);

        foreach (var jToken in sourceValue.Children())
        {
            sourceJTokenList.Add(jToken);
        }

        JToken targetValue = remoteToolService.GetLocationType(LocationType.target);

        foreach (var jToken in targetValue.Children())
        {
            targetJTokenList.Add(jToken);
        }
    }
    public async void Commit()
    {
        Repaint();

        isLoadingFinished = false;

        EditorCoroutineUtility.StartCoroutine(ChangeText(), this);
        
        JArray value = (JArray)TransformJsonListToConfig();

        await remoteToolService.CreateBackupFiles(sourceEnviromentType, targetEnviromentType, "Merge");

        remoteToolService.PutConfigToSelectedEnviroment(value,targetEnviromentType);

        isNewKeySend = true;
        isLoadingFinished = true;
    }
    private JToken TransformJsonListToConfig()
    {
        JToken value = remoteToolService.GetLocationType(LocationType.target).DeepClone();

        foreach(var label in remoteGUIContent.JTokenLabelListNew)
        {
            if (label.isRemoved) continue;

            // remove schema from new keys
            label.source["schemaId"] = null;

            value.First.AddBeforeSelf(label.source);
        }

        foreach (var label in remoteGUIContent.JTokenLabelListChanges)
        {
            foreach(var child in value.Children())
            {
                if ((string)child["key"] == label.name)
                {
                    child["value"] = JToken.Parse(label.mergedKey);
                }
            }
        }

        Debug.Log(value.ToString());

        return value;
    }
    IEnumerator ChangeText()
    {
        while (!isLoadingFinished)
        {
            if (currentText > 2) currentText = 0;

            switch (currentText)
            {
                case 0:
                    animatingText = "Loading.";
                    break;
                case 1:
                    animatingText = "Loading..";
                    break;
                case 2:
                    animatingText = "Loading...";
                    break;
            }

            Repaint();

            currentText++;
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }
    private void AnimateLoading()
    {
        GUILayout.BeginArea(new Rect(800, 500, 500, 200));

        GUILayout.TextArea(animatingText,RemoteMergeStyleSetup.CreateTextGUIStyle(24));

        GUILayout.EndArea();
    }
    public void ForceRepaint()
    {
        Repaint();
    }

}
