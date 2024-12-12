using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class QuestionnaireButton : Button
{
    public QuestionnaireTablet qt;
    public int value;

    public Color32 backupColor;
    public float lastSelect;
    public float selectdisable = 0.5f;
    public override void Select()
    {
        if (Time.time > selectdisable + lastSelect)
        {
            lastSelect = Time.time;
            base.Select();
            qt.Select(value);
            if (value != 0) { gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color32(0, 255, 0, 0); }
        }
    }public override void Unselect()
    {
        base.Unselect();
        gameObject.GetComponent<UnityEngine.UI.Image>().color = backupColor;
    }
}
