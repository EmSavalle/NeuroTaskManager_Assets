using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MovementReplayer : MonoBehaviour
{
    // Transform references for the GameObjects to replay movements on
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public bool replay;
    public string fileName;

    // Structure to store the loaded movement data
    [Serializable]
    public class MovementData
    {
        public float timeStamp;
        public Vector3 headPosition;
        public Quaternion headRotation;
        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosition;
        public Quaternion rightHandRotation;
    }

    // List to store the loaded movement data
    public List<MovementData> loadedMovements = new List<MovementData>();

    private bool isReplaying = false;

    // Coroutine to replay the recorded data
    IEnumerator ReplayMovements()
    {
        float startTime = Time.time;

        for (int i = 0; i < loadedMovements.Count - 1; i++)
        {
            MovementData currentFrame = loadedMovements[i];
            MovementData nextFrame = loadedMovements[i + 1];

            // Time between current frame and next frame
            float timeDelta = nextFrame.timeStamp - currentFrame.timeStamp;

            float elapsedTime = 0f;
            while (elapsedTime < timeDelta)
            {
                elapsedTime += Time.deltaTime;

                // Interpolate position and rotation between frames
                float t = elapsedTime / timeDelta;

                head.position = Vector3.Lerp(currentFrame.headPosition, nextFrame.headPosition, t);
                head.rotation = Quaternion.Lerp(currentFrame.headRotation, nextFrame.headRotation, t);

                leftHand.position = Vector3.Lerp(currentFrame.leftHandPosition, nextFrame.leftHandPosition, t);
                leftHand.rotation = Quaternion.Lerp(currentFrame.leftHandRotation, nextFrame.leftHandRotation, t);

                rightHand.position = Vector3.Lerp(currentFrame.rightHandPosition, nextFrame.rightHandPosition, t);
                rightHand.rotation = Quaternion.Lerp(currentFrame.rightHandRotation, nextFrame.rightHandRotation, t);

                yield return null;  // Wait until the next frame
            }
        }

        Debug.Log("Replay finished!");
    }

    // Function to load CSV data into the movement list
    public void LoadCSV(string filePath)
    {
        loadedMovements.Clear();
        using (StreamReader reader = new StreamReader(filePath))
        {
            // Skip header line
            reader.ReadLine();

            // Read the rest of the file line by line
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(';');

                // Parse the data from CSV
                MovementData data = new MovementData
                {
                    timeStamp = float.Parse(values[0]),
                    headPosition = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])),
                    headRotation = new Quaternion(float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]), float.Parse(values[7])),
                    leftHandPosition = new Vector3(float.Parse(values[8]), float.Parse(values[9]), float.Parse(values[10])),
                    leftHandRotation = new Quaternion(float.Parse(values[11]), float.Parse(values[12]), float.Parse(values[13]), float.Parse(values[14])),
                    rightHandPosition = new Vector3(float.Parse(values[15]), float.Parse(values[16]), float.Parse(values[17])),
                    rightHandRotation = new Quaternion(float.Parse(values[18]), float.Parse(values[19]), float.Parse(values[20]), float.Parse(values[21]))
                };

                // Add the data to the list
                loadedMovements.Add(data);
            }
        }

        Debug.Log($"Loaded {loadedMovements.Count} frames from CSV.");
    }

    // Start replay when the 'R' key is pressed
    void Update()
    {
        if(replay && !isReplaying)
        {
            LoadCSV(fileName);
            replay = false;
            StartCoroutine(ReplayMovements());
            isReplaying = true;
        }
    }
}
