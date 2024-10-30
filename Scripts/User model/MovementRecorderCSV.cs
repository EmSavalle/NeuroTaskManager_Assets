using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MovementRecorderCSV : MonoBehaviour
{
    // Transform references for head and hands
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public bool isRecording;

    public ParticipantInfos participantInfos;
    public TaskManager taskManager;
    // Structure to store position and rotation data
    [System.Serializable]
    public class MovementData
    {
        public Vector3 headPosition;
        public Quaternion headRotation;
        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosition;
        public Quaternion rightHandRotation;
        public float timeStamp;
    }

    // List to store movement data
    private List<MovementData> recordedMovements = new List<MovementData>();

    // Update is called once per frame
    void Update()
    {
        if(isRecording){
            RecordMovement();
        }
        
    }
    public void StartRecording(){
        recordedMovements = new List<MovementData>();
        isRecording = true;
    }
    public void StopRecording(){
        SaveDataToCSV();
        isRecording = false;
    }
    void RecordMovement()
    {
        // Create a new data object
        MovementData newData = new MovementData
        {
            headPosition = head.position,
            headRotation = head.rotation,
            leftHandPosition = leftHand.position,
            leftHandRotation = leftHand.rotation,
            rightHandPosition = rightHand.position,
            rightHandRotation = rightHand.rotation,
            timeStamp = Time.time
        };

        // Add the new data to the list
        recordedMovements.Add(newData);
    }

    // Method to save data to a CSV file
    public void SaveDataToCSV()
    {
        String filePath = "Logs/Movements"+participantInfos.participantId+"_"+taskManager.currentTask.ToString()+"_"+taskManager.currentCondition.ToString()+"_"+taskManager.currentDifficulty.ToString()+".csv";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write CSV header
            writer.WriteLine("Timestamp,HeadPosX,HeadPosY,HeadPosZ,HeadRotX,HeadRotY,HeadRotZ,HeadRotW,LeftHandPosX,LeftHandPosY,LeftHandPosZ,LeftHandRotX,LeftHandRotY,LeftHandRotZ,LeftHandRotW,RightHandPosX,RightHandPosY,RightHandPosZ,RightHandRotX,RightHandRotY,RightHandRotZ,RightHandRotW");

            // Write each recorded movement to the CSV
            foreach (var data in recordedMovements)
            {
                writer.WriteLine($"{data.timeStamp};" +
                    $"{data.headPosition.x};{data.headPosition.y};{data.headPosition.z};" +
                    $"{data.headRotation.x};{data.headRotation.y};{data.headRotation.z};{data.headRotation.w};" +
                    $"{data.leftHandPosition.x};{data.leftHandPosition.y};{data.leftHandPosition.z};" +
                    $"{data.leftHandRotation.x};{data.leftHandRotation.y};{data.leftHandRotation.z};{data.leftHandRotation.w};" +
                    $"{data.rightHandPosition.x};{data.rightHandPosition.y};{data.rightHandPosition.z};" +
                    $"{data.rightHandRotation.x};{data.rightHandRotation.y};{data.rightHandRotation.z};{data.rightHandRotation.w}");
            }
        }
        recordedMovements = new List<MovementData>();
        Debug.Log($"Data saved to CSV at {filePath}");
    }

    
}
