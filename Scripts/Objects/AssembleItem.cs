using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AssembleItem : Item
{
    public Connector myConnector;
    public Connector connectedConnector;

    public float positionTolerance = 0.01f;
    public float rotationTolerance = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        itemType = ItemType.UNCOMPLETEDITEM;
    }

    // Update is called once per frame
    void Update()
    {
        if(itemType == ItemType.SUBITEM){
            RealignConnection();
        }
    }
    public List<Connector> connectors; // List of connectors attached to the object
    public int hierarchyLevel; // The hierarchy level of the object (lower level means it gets merged)
    //
    public void RealignConnection(){
        
        // Get the position and rotation of the two connectors
        Transform thisTransform = myConnector.transform;
        Transform otherTransform = connectedConnector.transform;
        AssembleItem parentItem = gameObject.GetComponentInParent<AssembleItem>();
        // Calculate the necessary position and rotation for alignment
        Vector3 positionOffset = otherTransform.position - thisTransform.position;

        // Move the parent AssemblableItem of the current Connector
        if (positionOffset.magnitude > positionTolerance)
        {
            parentItem.transform.position += positionOffset;
        }        

        // Align the two AssemblableItem rotations
        Quaternion targetRotation = Quaternion.FromToRotation(thisTransform.right, -otherTransform.right);
        parentItem.transform.rotation = targetRotation * parentItem.transform.rotation;
        
        AlignRotationWithoutXAxis(thisTransform, otherTransform);
    }
    private void AlignRotationWithoutXAxis(Transform thisTransform, Transform otherTransform)
    {
        AssembleItem parentItem = gameObject.GetComponentInParent<AssembleItem>();
        // Get the forward directions of both connectors
        Vector3 forwardThis = thisTransform.forward;
        Vector3 forwardOther = otherTransform.forward;

        // Project the forward direction onto the YZ plane (lock the X axis)
        forwardThis.x = 0;
        forwardOther.x = 0;

        // Normalize the vectors to avoid any scaling issues
        forwardThis.Normalize();
        forwardOther.Normalize();

        // Calculate the target rotation that aligns the forward directions
        Quaternion targetRotation = Quaternion.FromToRotation(forwardThis, -forwardOther);
        float angleDifference = Quaternion.Angle(parentItem.transform.rotation, targetRotation * parentItem.transform.rotation);

        // Apply the rotation to the parent AssemblableItem, but only around the YZ axes
        if (angleDifference > rotationTolerance){
            parentItem.transform.rotation = targetRotation * parentItem.transform.rotation;
        }
        
    }
    public Quaternion FaceEachOtherUP(GameObject objA, GameObject objB)
    {
        // Get the direction along the red arrow (X-axis) of both objects
        Vector3 redArrowA = objA.transform.right;  // X-axis is the 'right' vector
        Vector3 redArrowB = objB.transform.right;

        // Calculate the dot product to see if they are facing each other along the red arrow (X-axis)
        float dotProduct = Vector3.Dot(redArrowA, -redArrowB);

        // If the dot product is close to 1, they are facing each other
        Debug.Log("Objects are not facing each other along the red axis (X-axis).");
        // Calculate the rotation to align them
        Quaternion targetRotation = Quaternion.LookRotation(-redArrowB, objA.transform.up);
        Quaternion currentRotation = objA.transform.rotation;

        // Determine the rotation difference
        Quaternion rotationDifference = targetRotation * Quaternion.Inverse(currentRotation);

        return rotationDifference;
        
    }
    // Called when an object connects with another
    public void MergeWith(AssembleItem other)
    {
        if (other.hierarchyLevel < this.hierarchyLevel)
        {
            // The other object becomes part of this object
            foreach (var connector in other.connectors)
            {
                if(!connector.connected){
                    this.connectors.Add(connector);
                    connector.transform.SetParent(this.transform); // Set other connectors as children
                }
            }
            //Disable other object
            other.connectors = new List<Connector>();
            other.itemType=ItemType.SUBITEM;
            other.GetComponent<Rigidbody>().useGravity=false;
            other.GetComponent<XRGrabInteractable>().enabled = false;
            //Destroy(other.gameObject); // Remove the other object after merging
        }
        else
        {
            // This object becomes part of the other object
            other.MergeWith(this);
            itemType=ItemType.SUBITEM;
        }
        CheckCompletion();
    }
    public void CheckCompletion(){
        bool completed = true;
        foreach(Connector c in connectors){
            if(!c.connected){
                completed = false;
            }
        }
        if(completed && itemType==ItemType.UNCOMPLETEDITEM){
            itemType=ItemType.COMPLETEITEM;
        }
    }
}
