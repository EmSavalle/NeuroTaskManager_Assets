using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class Item : MonoBehaviour
{
    public ItemType itemType;

    public bool disableGrab;
    public bool enableGrab;
    public bool isGrabbed;

    public XRGeneralGrabTransformer xRGeneralGrabTransformer;
    public XRGrabInteractable xRGrabInteractable;
    public Rigidbody selfRigidbody;
    public int itemNumber;
    public ItemShape itemShape;
    public ItemColor itemColor;

    public GameObject sphere,cube;
    public GameObject thisObject;
    public bool setup;
    public bool nback;
    // Start is called before the first frame update
    void Start()
    {
        if(xRGeneralGrabTransformer==null){
            xRGeneralGrabTransformer=gameObject.GetComponent<XRGeneralGrabTransformer>();
        }
        if(xRGrabInteractable==null){
            xRGrabInteractable=gameObject.GetComponent<XRGrabInteractable>();
        }
    }
    public void SetUpItem(){
        string textureName="";
        if(itemShape == ItemShape.CUBE){
            thisObject= Instantiate(cube,transform.position,transform.rotation);
            thisObject.transform.parent=transform;
            thisObject.transform.localPosition = new Vector3(0,0,0);
            switch(itemColor){
                case ItemColor.WHITE:
                    textureName+="White";
                    break;
                case ItemColor.RED:
                    textureName+="Red";
                    break;
                case ItemColor.GREEN:
                    textureName+="Green";
                    break;
                default: 
                    break;

            }
            Texture2D texture = Resources.Load<Texture2D>(textureName);  
            Material m = Resources.Load("Materials/"+textureName) as Material;
            Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            newMaterial.mainTexture = texture;

            // Step 3: Assign the new material to the object's renderer
            Renderer objectRenderer = thisObject.GetComponent<Renderer>();
            if(objectRenderer == null){objectRenderer = thisObject.GetComponentInChildren<Renderer>();}
            if (objectRenderer != null)
            {
                //objectRenderer.material = newMaterial;
                objectRenderer.material=m;
            }
            else{
                Debug.Log("No renderer");
            }
            SetDescendantText(transform,itemNumber);
        }
        else if(itemShape == ItemShape.SPHERE){
            thisObject= Instantiate(sphere,transform.position,transform.rotation);
            thisObject.transform.parent=transform;
            thisObject.transform.localPosition = new Vector3(0,0,0);
            textureName="Sphere";
            if(itemNumber != 0){
                textureName+=itemNumber.ToString();
            }
            switch(itemColor){
                case ItemColor.WHITE:
                    textureName+="White";
                    break;
                case ItemColor.RED:
                    textureName+="Red";
                    break;
                case ItemColor.GREEN:
                    textureName+="Green";
                    break;
                default: 
                    break;

            }

            Texture2D texture = Resources.Load<Texture2D>(textureName+"");
            Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")); 
            Material m = Resources.Load("Materials/"+textureName, typeof(Material)) as Material;
            newMaterial.mainTexture = texture; // Assign the texture to the material

            // Step 3: Assign the new material to the object's renderer
            Renderer objectRenderer = thisObject.GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                //objectRenderer.material = newMaterial;
                objectRenderer.material=m;
            }
        }

    }
    private void SetDescendantText(Transform parent, int number)
    {
        foreach (Transform child in parent)
        {
            TMP_Text text = child.GetComponent<TMP_Text>();
            if (text != null)
            {
                if(number!=0){
                    text.text = number.ToString();
                }
                else{
                    text.text = "";
                }
            }
            else{
                SetDescendantText(child, number);
            }
            
        }
    }
    // Update is called once per frame
    public void Update()
    {
        if(setup){
            setup = false;
            SetUpItem();
        }
        if(selfRigidbody == null){
            selfRigidbody=gameObject.GetComponent<Rigidbody>();
        }
        if(selfRigidbody != null){
            isGrabbed = (selfRigidbody.angularDrag==0);
        }
        if(xRGeneralGrabTransformer==null){
            xRGeneralGrabTransformer=gameObject.GetComponent<XRGeneralGrabTransformer>();
        }
        if(xRGrabInteractable==null){
            xRGrabInteractable=gameObject.GetComponent<XRGrabInteractable>();
        }
        if(xRGrabInteractable != null){
            isGrabbed=xRGrabInteractable.interactorsSelecting.Count!=0;
        }
        
        if(disableGrab){
            disableGrab = false;
            Ungrab();
        }
        if(enableGrab){
            enableGrab = false;
            Regrab();
        }
    }
    public void Ungrab(){
        xRGrabInteractable.enabled = false;
        xRGeneralGrabTransformer.enabled = false;

    }
    public void Regrab(){
        xRGrabInteractable.enabled = true;
        xRGeneralGrabTransformer.enabled = true;

    }
}

public enum ItemType {ITEMA,ITEMB,ITEMC,UNCOMPLETEDITEM,COMPLETEITEM,SUBITEM,RECEIVER,CONNECTINGCABLE,COMPLETEDRECEIVER,BOXEDITEM,NONE}

public enum ItemShape {SPHERE,CUBE};
public enum ItemColor {WHITE,RED,GREEN};