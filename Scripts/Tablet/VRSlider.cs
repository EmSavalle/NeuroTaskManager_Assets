using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSlider : MonoBehaviour
{
    public GameObject sphere,cylinder;
    // Start is called before the first frame update

    public Transform sliderHandle; // Assign the sphere here
    public Transform sliderBase; // Assign the cylinder here
    public float sliderLength = 1.0f; // Length of the cylinder
    private Vector3 sliderStartPos;
    private Vector3 sliderEndPos;
    public float sliderValue = 0f; // The slider value, from 0 to 100
    
    public Orientation orientation;
    void Start()
    {
        // Calculate the start and end points of the slider based on the cylinder's position and length
        if(orientation == Orientation.X){
            sliderStartPos = sliderBase.position - (-sliderBase.up * sliderLength);
            sliderEndPos = sliderBase.position + (-sliderBase.up * sliderLength);
        }
        else if(orientation == Orientation.Y){
            sliderStartPos = sliderBase.position - (sliderBase.right * sliderLength);
            sliderEndPos = sliderBase.position + (sliderBase.right * sliderLength);
        }
        else {
            sliderStartPos = sliderBase.position - (sliderBase.forward * sliderLength);
            sliderEndPos = sliderBase.position + (sliderBase.forward * sliderLength);
        }
        
    }

    void OnCollisionEnter(Collision collision)
    {
        // Detect collision point
        Vector3 collisionPoint = collision.contacts[0].point;

        // Clamp the collision point within the slider's length
        collisionPoint = ClampPositionOnSlider(collisionPoint);

        // Move the slider handle to the collision point along the cylinder axis
        if(orientation==Orientation.X){
            sliderHandle.position = new Vector3(collisionPoint.x,sliderHandle.position.y,sliderHandle.position.z);
        }
        if(orientation==Orientation.Y){
            sliderHandle.position = new Vector3(sliderHandle.position.x,collisionPoint.y,sliderHandle.position.z);
        }
        if(orientation==Orientation.Z){
            sliderHandle.position = new Vector3(sliderHandle.position.x,sliderHandle.position.y,collisionPoint.z);
        }
        

        // Calculate slider value as percentage (0 to 100%)
        sliderValue = CalculateSliderValue(collisionPoint);

        Debug.Log("Slider Value: " + sliderValue);
    }
    void OnTriggerEnter(Collider other)
    {
        // Get the position where the trigger interaction happened
        Vector3 triggerPoint = other.transform.position;

        // Clamp the trigger point within the slider's length
        triggerPoint = ClampPositionOnSlider(triggerPoint);
        if(orientation==Orientation.X){
            sliderHandle.position = new Vector3(triggerPoint.x,sliderHandle.position.y,sliderHandle.position.z);
        }
        if(orientation==Orientation.Y){
            sliderHandle.position = new Vector3(sliderHandle.position.x,triggerPoint.y,sliderHandle.position.z);
        }
        if(orientation==Orientation.Z){
            sliderHandle.position = new Vector3(sliderHandle.position.x,sliderHandle.position.y,triggerPoint.z);
        }

        // Calculate the slider value as percentage (0 to 100%)
        sliderValue = CalculateSliderValue(triggerPoint);

        Debug.Log("Slider Value: " + sliderValue);
    }
    void OnTriggerStay(Collider other)
    {
        // Get the position where the trigger interaction happened
        Vector3 triggerPoint = other.transform.position;

        // Clamp the trigger point within the slider's length
        triggerPoint = ClampPositionOnSlider(triggerPoint);

        if(orientation==Orientation.X){
            sliderHandle.position = new Vector3(triggerPoint.x,sliderHandle.position.y,sliderHandle.position.z);
        }
        if(orientation==Orientation.Y){
            sliderHandle.position = new Vector3(sliderHandle.position.x,triggerPoint.y,sliderHandle.position.z);
        }
        if(orientation==Orientation.Z){
            sliderHandle.position = new Vector3(sliderHandle.position.x,sliderHandle.position.y,triggerPoint.z);
        }

        // Calculate the slider value as percentage (0 to 100%)
        sliderValue = CalculateSliderValue(triggerPoint);

        Debug.Log("Slider Value: " + sliderValue);
    }
    // Clamp the collision point within the slider limits
    private Vector3 ClampPositionOnSlider(Vector3 collisionPoint)
    {
        if(orientation==Orientation.X){
            float clampedX = Mathf.Clamp(collisionPoint.x, sliderStartPos.x, sliderEndPos.x);
            return new Vector3(clampedX, collisionPoint.y, collisionPoint.z);
        }
        else if(orientation==Orientation.Y){
            float clampedY = Mathf.Clamp( sliderStartPos.y,collisionPoint.y, sliderEndPos.y);
            return new Vector3(collisionPoint.x, clampedY, collisionPoint.z);
        }
        else{
            float clampedZ = Mathf.Clamp( sliderStartPos.z,collisionPoint.z, sliderEndPos.z);
            return new Vector3(collisionPoint.x, collisionPoint.y,clampedZ);
        }
        
    }

    // Calculate the slider value as a percentage based on the handle's position
    private float CalculateSliderValue(Vector3 handlePos)
    {
        if(orientation==Orientation.X){
            float sliderPos = Mathf.InverseLerp(sliderStartPos.x, sliderEndPos.x, handlePos.x);
            return sliderPos * 100f;
        }
        else if(orientation==Orientation.Y){
            float sliderPos = Mathf.InverseLerp(sliderStartPos.y, sliderEndPos.y, handlePos.y);
            return sliderPos * 100f;
        }
        else{
            float sliderPos = Mathf.InverseLerp(sliderStartPos.z, sliderEndPos.z, handlePos.z);
            return sliderPos * 100f;
        }
    }
}
public enum Orientation {X,Y,Z};
