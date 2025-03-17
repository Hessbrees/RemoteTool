using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteMergeInputs
{
    RemoteGUIContent remoteGUIContent;
    public RemoteMergeInputs(RemoteGUIContent remoteGUIContent)
    {
        this.remoteGUIContent = remoteGUIContent;
    }
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                break;
            case EventType.KeyDown:
                ProcessKeyDownEvent(currentEvent);
                break;
            case EventType.KeyUp:
                break;
            case EventType.MouseDrag:
            default:
                break;
        }
    }

    private void ProcessKeyDownEvent(Event currentEvent)
    {
        MergeWindowButtons(currentEvent);

        if (currentEvent.keyCode == KeyCode.UpArrow)
        {
            ChangeSelectedUp();
        }
        else if (currentEvent.keyCode == KeyCode.DownArrow)
        {
            ChangeSelectedDown();
        }
        else if (currentEvent.keyCode == KeyCode.LeftArrow)
        {
            ChangeSelectedType();
        }
        else if (currentEvent.keyCode == KeyCode.RightArrow)
        {
            ChangeSelectedType();
        }
    }

    private void ChangeSelectedType()
    {
        JsonLabel jsonLabel = remoteGUIContent.GetCurrentSelectedFromAll();

        if (jsonLabel == null)
        {
            remoteGUIContent.SelectJsonLabelNew(0);
            return;
        }

        int index = jsonLabel.index;

        if(jsonLabel.jsonLabelType == JsonLabelType.newLabel)
        {
            if (index >= remoteGUIContent.JTokenLabelListChanges.Count)
            {
                remoteGUIContent.SelectJsonLabelChanges(remoteGUIContent.JTokenLabelListChanges.Count - 1);
                return;
            }

            remoteGUIContent.SelectJsonLabelChanges(index);
        }
        else
        {
            if (index >= remoteGUIContent.JTokenLabelListNew.Count)
            {
                remoteGUIContent.SelectJsonLabelNew(remoteGUIContent.JTokenLabelListNew.Count - 1);
                return;
            }

            remoteGUIContent.SelectJsonLabelNew(index);
        }
    }
    private void ChangeSelectedUp()
    {
        JsonLabel jsonLabel = remoteGUIContent.GetCurrentSelectedFromAll();

        if(jsonLabel == null)
        {
            remoteGUIContent.SelectJsonLabelNew(0);
            return;
        }

        int index = jsonLabel.index - 1;

        ChangeSelectedJtoken(index, jsonLabel.jsonLabelType);
    }
    private void ChangeSelectedDown()
    {
        JsonLabel jsonLabel = remoteGUIContent.GetCurrentSelectedFromAll();

        if (jsonLabel == null)
        {
            remoteGUIContent.SelectJsonLabelNew(0);
            return;
        }

        int index = jsonLabel.index + 1;

        ChangeSelectedJtoken(index, jsonLabel.jsonLabelType);
    }
    private void ChangeSelectedJtoken(int index, JsonLabelType jsonLabelType)
    {
        if (jsonLabelType == JsonLabelType.newLabel)
        {
            if (index < 0)
            {
                index = remoteGUIContent.JTokenLabelListNew.Count - 1;
            }
            else if (index >= remoteGUIContent.JTokenLabelListNew.Count)
            {
                index = 0;
            }

            remoteGUIContent.SelectJsonLabelNew(index);
        }
        else
        {
            if (index < 0)
            {
                index = remoteGUIContent.JTokenLabelListChanges.Count - 1;
            }
            else if (index >= remoteGUIContent.JTokenLabelListChanges.Count)
            {
                index = 0;
            }

            remoteGUIContent.SelectJsonLabelChanges(index);
        }
    }

    private int GetSelectedIndex()
    {
        return 0;
    }
    private void MergeWindowButtons(Event currentEvent)
    {
        if (remoteGUIContent.IsAnySelected())
        {
            if (currentEvent.keyCode == KeyCode.Q)
            {
                remoteGUIContent.ShowMergedWindowChange();
            }
            else if (currentEvent.keyCode == KeyCode.W)
            {
                remoteGUIContent.ChangeClickedButton(MergeType.keepSource);
            }
            else if (currentEvent.keyCode == KeyCode.E)
            {
                remoteGUIContent.ChangeClickedButton(MergeType.keepTarget);
            }
            else if (currentEvent.keyCode == KeyCode.R)
            {
                remoteGUIContent.ChangeClickedButton(MergeType.tryMergeToSource);
            }
            else if (currentEvent.keyCode == KeyCode.T)
            {
                remoteGUIContent.ChangeClickedButton(MergeType.tryMergeToTarget);
            }
            else if (currentEvent.keyCode == KeyCode.Y)
            {
                remoteGUIContent.ChangeClickedButton(MergeType.keepEdited);
            }
            else if (currentEvent.keyCode == KeyCode.F)
            { 
                remoteGUIContent.ChangeClickedButtonAll(MergeType.keepSource);
            }
            else if (currentEvent.keyCode == KeyCode.G)
            {
                remoteGUIContent.ChangeClickedButtonAll(MergeType.keepSource);
            }
        }
        else if(remoteGUIContent.IsAnySelectedNew())
        {
            if (currentEvent.keyCode == KeyCode.W)
            {
                remoteGUIContent.EnableCurrentKey();
            }
            else if (currentEvent.keyCode == KeyCode.E)
            {
                remoteGUIContent.RemoveCurrentKey();
            }
        }

    }
}
