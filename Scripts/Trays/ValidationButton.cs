using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidationButton : Button
{
    public ValidationTablet vt;
    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();   
    }
    public override void Select()
    {
        base.Select();
        base.Unselect();
        vt.validated=true;

    }
}
