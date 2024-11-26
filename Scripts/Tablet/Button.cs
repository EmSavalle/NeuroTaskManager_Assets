using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Button : MonoBehaviour
{
    public bool isTriggered = false;

    [Header("Debug tools")]
    public bool fake;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(fake){
            fake = false;
            fakePress();
        }
    }
    public virtual void Select(){
        isTriggered = true;
    }
    public virtual void Unselect(){
        isTriggered = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name != "RayInteractor"){

            //Debug.Log("Triggered");
            Select();
        }
    }

    public void fakePress(){
        fake = false;
        Select();
    }
}
