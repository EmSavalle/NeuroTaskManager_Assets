using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Tablet : MonoBehaviour
{
    public List<Button> buttons;

    public bool ended,started;
    public bool ending,starting;

    public Vector3 startPosition,endPosition;
    public Vector3 startRotation,endRotation;
    public float moveSpeed,rotationSpeed;
    public bool changePosition,changeRotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonPressed(Button b){

    }

    public virtual IEnumerator StartTablet(){
        started=true;
        ended = false;
        starting = true;
        Quaternion targetRotation = Quaternion.Euler(endRotation);  
        while ( (Vector3.Distance(transform.localPosition, endPosition) > 0.01f && changePosition) || (changeRotation && Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f))  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            if(changePosition){
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            }
            if(changeRotation){
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // Wait for the next frame before continuing
            yield return null;
        }
        
        starting = false;
        yield break;
    }
    

    public virtual IEnumerator EndTablet(){
        ending = true;
        Quaternion targetRotation = Quaternion.Euler(startRotation);  
        while ( (Vector3.Distance(transform.localPosition, startPosition) > 0.01f && changePosition) || (changeRotation && Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f))  // 0.01 is tolerance for close enough
        {
            if(changePosition){
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, moveSpeed * Time.deltaTime);
            }
            if(changeRotation){
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            // Move towards the target
            
            
            // Wait for the next frame before continuing
            yield return null;
        }
        started = false;
        ended = true;
        ending = false;
        yield break;
    }
}
