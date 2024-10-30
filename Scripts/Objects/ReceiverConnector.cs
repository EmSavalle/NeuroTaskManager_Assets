using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ReceiverConnector : Item
{
    public string connectorType; // The type of connector for matching
    //public Transform snapPoint;  // Where the objects should snap (child object)
    public ReceiverItem receiverItem;
    public bool touchingConnector;
    public Cable connectorTouched;
    public bool mainConnector;
    public bool badConnector;
    private void OnTriggerEnter(Collider other)
    {
        Item i = other.gameObject.GetComponent<Item>();
        if(i == null && other.transform.parent != null){
            i = other.transform.parent.gameObject.GetComponent<Item>();
        }
        if(i != null){
            if(i.itemType == ItemType.CONNECTINGCABLE && !touchingConnector){
                Debug.Log("Receiver connector - touched !");
                touchingConnector = true;
                connectorTouched = other.gameObject.GetComponent<Cable>();
                if(connectorTouched == null){connectorTouched = other.transform.parent.gameObject.GetComponent<Cable>();}
                receiverItem.ConnectorTouched(connectorTouched);
            }
        }
        
    }
    private void OnTriggerExit(Collider other){
        Item i = other.gameObject.GetComponent<Item>();
        if(i == null && other.transform.parent != null){
            i = other.transform.parent.gameObject.GetComponent<Item>();
        }
        if(i != null){
            if(i.itemType == ItemType.CONNECTINGCABLE && touchingConnector){
                Debug.Log("Receiver connector - untouched !");
                touchingConnector = false;
                connectorTouched = null;
                receiverItem.ConnectorUntouched();
            }
        }
    }
    new void Update(){
        base.Update();
    }

}
