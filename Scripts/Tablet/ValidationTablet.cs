using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidationTablet : Tablet
{
    public ValidationButton vb;
    public bool validated;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        
    }
    
    public bool GetValidation(){
        if(validated){
            vb.isTriggered=false;
            validated = false;
            return true;
        }
        return false;
    }
    
    
}
