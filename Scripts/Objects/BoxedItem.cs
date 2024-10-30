using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
public class BoxedItem : Item
{
    public List<BoxParameters> parameters;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    
    new void Update(){
        base.Update();
    }
}

[Serializable]
public struct BoxParameters{
    public string name;
    public float value;
}
