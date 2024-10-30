using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class QuestionnaireButton : Button
{
    public QuestionnaireTablet qt;
    public int value;

    public Color32 backupColor;
    public override void Select()
    {
        base.Select();
        qt.Select(value);
        gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color32(0,255,0,0);
    }public override void Unselect()
    {
        base.Unselect();
        gameObject.GetComponent<UnityEngine.UI.Image>().color = backupColor;
    }
}
