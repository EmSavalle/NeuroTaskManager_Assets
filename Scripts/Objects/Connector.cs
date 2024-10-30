using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public string connectorType; // The type of connector for matching
    //public Transform snapPoint;  // Where the objects should snap (child object)

    public bool connected;
    private void OnTriggerEnter(Collider other)
    {
        Connector otherConnector = other.GetComponent<Connector>();
        if (otherConnector != null && otherConnector.connectorType == this.connectorType && !connected && !otherConnector.connected)
        {
            // Get the parent AssembledObject of both connectors
            AssembleItem thisObject = this.GetComponentInParent<AssembleItem>();
            AssembleItem otherObject = otherConnector.GetComponentInParent<AssembleItem>();
            if(thisObject.hierarchyLevel>otherObject.hierarchyLevel){    
                connected = true;
                otherConnector.connected=true;
                if (thisObject != null && otherObject != null)
                {
                    // Snap objects together
                    SnapObjects(thisObject, otherObject, otherConnector);

                    // Merge objects after snapping
                    thisObject.MergeWith(otherObject);
                    
                    // Create FixedJoint to maintain the connection
                    //CreateFixedJoint(thisObject, otherObject);
                }
                otherObject.myConnector = otherConnector;
                otherObject.connectedConnector = this;
            }
            else{
                thisObject.myConnector = this;
                thisObject.connectedConnector = otherConnector;
            }
        }
    }

    private void SnapObjects(AssembleItem thisObject, AssembleItem otherObject, Connector otherConnector)
    {
        // Get the difference in position and rotation between the connectors
        Transform otherSnapPoint = otherConnector.transform;

        // Calculate the position and rotation offsets
        Vector3 positionOffset = otherSnapPoint.position - this.transform.position;
        Quaternion rotationOffset = Quaternion.Inverse(this.transform.rotation) * otherSnapPoint.rotation;

        // Move and rotate the other object to align with this object
        otherObject.transform.position -= positionOffset; 
        otherObject.transform.rotation *= rotationOffset;

        // Optional: Set parent to snap object into position
        otherObject.transform.SetParent(thisObject.transform); 
        otherObject.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        otherObject.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        thisObject.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        thisObject.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
    private void CreateFixedJoint(AssembleItem parentObject, AssembleItem childObject)
    {
        // Check if the parent object has a Rigidbody (if not, add one)
        Rigidbody parentRb = parentObject.GetComponent<Rigidbody>();
        if (parentRb == null)
        {
            parentRb = parentObject.gameObject.AddComponent<Rigidbody>();
            parentRb.isKinematic = true; // Make it static to prevent unwanted movements
        }

        // Attach a FixedJoint to the child object
        Rigidbody childRb = childObject.GetComponent<Rigidbody>();
        if (childRb == null)
        {
            childRb = childObject.gameObject.AddComponent<Rigidbody>();
        }

        FixedJoint fixedJoint = childObject.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = parentRb; // Connect the child object to the parent
        fixedJoint.breakForce = Mathf.Infinity; // Optional: Set to a high value to prevent breaking
    }
}
