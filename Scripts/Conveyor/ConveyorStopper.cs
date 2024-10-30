using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorStopper : MonoBehaviour
{
    public ConveyorBelt cb;
    // Start is called before the first frame update
    private List<Collider> objectsInCollider = new List<Collider>(); 
    private bool isOccupied = false; 

    private void OnTriggerEnter(Collider other)
    {
        
        objectsInCollider.Add(other); // Add the object to the list
        isOccupied = true;
        cb.stopperTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        objectsInCollider.Remove(other); // Remove the object from the list
        if(objectsInCollider.Count == 0){
            isOccupied = false;
            if(cb.beltResumeEmpty){
                cb.stopperTriggered = false;
            }
            
        }
    }
}
