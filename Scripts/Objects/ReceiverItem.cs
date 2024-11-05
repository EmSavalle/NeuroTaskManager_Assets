using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class ReceiverItem : Item
{
    public ParticipantInfos participantInfos;
    public List<ReceiverConnector> myConnectors;
    public List<ReceiverConnector> badConnectors;
    public List<GameObject> bases;
    public GameObject badConnectorHolder;
    public GameObject goodConnectorHolder;
    public ReceiverConnector mainConnector;
    public Cable touchingCable;
    public int nbTouched;

    public bool isConnected;
    public bool isValidated;

    public bool awaitingForConnection = false;
    // Start is called before the first frame update
    void Start()
    {
        if(goodConnectorHolder == null){
            for(int i = 0; i < gameObject.transform.childCount; i++){
                if(gameObject.transform.GetChild(i).name == "Bad connectors"){
                    badConnectorHolder = gameObject.transform.GetChild(i).gameObject;
                }
                else if(gameObject.transform.GetChild(i).name == "Good connectors"){
                    goodConnectorHolder = gameObject.transform.GetChild(i).gameObject;
                }
            }
            
        }
        //Initialyze connectors
        myConnectors = new List<ReceiverConnector>();
        badConnectors = new List<ReceiverConnector>();
        for(int i = 0; i < goodConnectorHolder.transform.childCount; i++){
            ReceiverConnector rc = goodConnectorHolder.transform.GetChild(i).GetComponent<ReceiverConnector>();
            myConnectors.Add(rc);
        
        }
        for(int i = 0; i < badConnectorHolder.transform.childCount; i++){
            ReceiverConnector rc = badConnectorHolder.transform.GetChild(i).GetComponent<ReceiverConnector>();
            badConnectors.Add(rc);
        }
        isConnected = false;
        if(mainConnector == null){
            foreach(ReceiverConnector rc in myConnectors){
                if(rc.mainConnector){
                    mainConnector = rc;
                }
            }
        }
        if(bases.Count == 0){
            for (int i = 0; i < gameObject.transform.childCount; i++){
                if(gameObject.transform.GetChild(i).name == "Grid"){
                    bases = GetAllChildren(gameObject.transform.GetChild(i));
                }
            }
        }
        if(participantInfos==null){
            participantInfos=GameObject.Find("--- Management ---").GetComponent<ParticipantInfos>();
        }
    }

    // Update is called once per frame

    new void Update(){
        base.Update();
        if(awaitingForConnection){
            CompleteConnection();
        }
    }
    public List<GameObject> GetAllChildren(Transform parent, List<GameObject> transformList = null)
     {
         if (transformList == null) transformList = new List<GameObject>();
         
         foreach (Transform child in parent) {
             transformList.Add(child.gameObject);
             GetAllChildren(child, transformList);
         }
         return transformList;
     }
    public void ConnectorTouched(Cable touching){
        if(itemType != ItemType.COMPLETEDRECEIVER){
            if(touchingCable == null){
                touchingCable=touching;
            }
            if(touchingCable == touching){
                nbTouched+=1;
                awaitingForConnection=true;
            }
        }
    }
    public void ConnectorUntouched(){
        if(itemType != ItemType.COMPLETEDRECEIVER){
            nbTouched --;
            if(nbTouched == 0){
                touchingCable = null;
                awaitingForConnection = false;
            }
        }
        
    }
    public bool CheckConnection(bool validate){
        bool isGood=true;
        foreach(ReceiverConnector rc in myConnectors){
            if(!rc.touchingConnector || rc.connectorTouched == null){
                isGood = false;
            }
        }
        foreach(ReceiverConnector rc in badConnectors){
            if(rc.touchingConnector){
                isGood = false;
            }
        }
        if(validate){
            if(isGood){
                participantInfos.TaskSuccess();
            }
            else{
                participantInfos.TaskError();
            }
        }
        return isGood;
    }
    public void CompleteConnection(){
        Debug.Log("Receiver item - connected !");
        if(touchingCable.isGrabbed){
            awaitingForConnection = true;
        }
        else{
            awaitingForConnection = false;
            isConnected = true;
            itemType = ItemType.COMPLETEDRECEIVER;
            isValidated = CheckConnection(false);
            //TODO define alignment/snapping object
            GameObject contactPoint = touchingCable.contactPoint;
            Vector3 diffPosition = FindClosestObject(bases,contactPoint);
            if(mainConnector == null){
                mainConnector = myConnectors[0].GetComponent<ReceiverConnector>();
            }
            float diffHeight = mainConnector.gameObject.transform.position.y - contactPoint.transform.position.y;
            Vector3 newPos = new Vector3(contactPoint.transform.position.x+diffPosition.x,contactPoint.transform.position.y+diffHeight,contactPoint.transform.position.z+diffPosition.z);
            touchingCable.gameObject.transform.position = newPos;
            touchingCable.transform.parent = gameObject.transform;
            Rigidbody touched = touchingCable.gameObject.GetComponent<Rigidbody>();
            if(touched!= null){
                touched.useGravity = false;
                touched.velocity = Vector3.zero;
            }
            /*XRGeneralGrabTransformer xRGeneralGrabTransformer=touchingCable.gameObject.GetComponent<XRGeneralGrabTransformer>();
            XRGrabInteractable xRGrabInteractable=touchingCable.gameObject.GetComponent<XRGrabInteractable>();
            //Destroying connecting object utilities
            if(xRGeneralGrabTransformer==null){                
                Destroy(xRGeneralGrabTransformer);
            }
            if(xRGrabInteractable==null){
                
                Destroy(xRGrabInteractable);
            }

            Destroy(touched);
            for (int i = 0; i < touchingCable.gameObject.transform.childCount; i++){
                Rigidbody rb = touchingCable.gameObject.transform.GetChild(i).GetComponent<Rigidbody>();
                if(rb!=null){Destroy(rb);}
            }
            // Check if the GameObject has a BoxCollider and remove it
            BoxCollider boxCollider = touchingCable.gameObject.GetComponent<BoxCollider>();
            Destroy(boxCollider);
            for (int i = 0; i < touchingCable.gameObject.transform.childCount; i++){
                BoxCollider bc = touchingCable.gameObject.transform.GetChild(i).GetComponent<BoxCollider>();
                if(bc!=null){Destroy(bc);}
            }
            touchingCable.Ungrab();*/
            itemType = ItemType.COMPLETEDRECEIVER;
        }
    }
    public void AlignToNearestRightAngle(GameObject rotate)
    {
        // Get current rotation in Euler angles
        Vector3 currentRotation = rotate.transform.eulerAngles;

        // Align each axis (X, Y, Z) to the nearest 90-degree multiple
        currentRotation.x = Mathf.Round(currentRotation.x / 90) * 90;
        currentRotation.y = Mathf.Round(currentRotation.y / 90) * 90;
        currentRotation.z = Mathf.Round(currentRotation.z / 90) * 90;

        // Apply the aligned rotation back to the GameObject
        rotate.transform.eulerAngles = currentRotation;
    }
    public List<Connector> connectors; // List of connectors attached to the object
    public int hierarchyLevel; // The hierarchy level of the object (lower level means it gets merged)
    //
    
    public Vector3 FindClosestObject(List<GameObject> objectsList, GameObject comparisonObject)
    {
        if (objectsList == null || objectsList.Count == 0 || comparisonObject == null)
        {
            Debug.LogError("Invalid input: objectsList is empty or comparisonObject is null.");
            return Vector3.zero;
        }

        GameObject closestObject = null;
        float closestDistance = Mathf.Infinity;

        Vector3 comparisonPosition = comparisonObject.transform.position;

        // Loop through the list to find the closest object
        foreach (GameObject obj in objectsList)
        {
            float distance = Vector3.Distance(obj.transform.position, comparisonPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = obj;
            }
        }

        // Return the positional difference between the closest object and the comparison object
        if (closestObject != null)
        {
            return closestObject.transform.position - comparisonPosition;
        }
        else
        {
            return Vector3.zero;
        }
    }
}
