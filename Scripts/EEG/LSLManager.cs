using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using System;
public class LSLManager : MonoBehaviour
{
    public TaskManager taskManager;
    public List<LSLStream> lSLStreams;
    public int experimentMarkerIndex=-1;
    public int maxSamples = 1000; // Maximum number of samples to store
    public Dictionary<ExperimentStep,int> stepConv = new Dictionary<ExperimentStep, int>();
    // Start is called before the first frame update
    void Start()
    {
        stepConv[ExperimentStep.TASKSTART]=1;
        stepConv[ExperimentStep.TASKEND]=2;
        stepConv[ExperimentStep.QUESTIONNAIRESTART]=3;
        stepConv[ExperimentStep.QUESTIONNAIREEND]=4;

        for (int i = 0; i < lSLStreams.Count; i++){
            LSLStream ls = lSLStreams[i];
            Debug.Log("Setting up :"+ls.streamName.ToString());
            if(ls.outStream){
                var hash = new Hash128();
                hash.Append(ls.streamName);
                hash.Append(ls.streamType);
                hash.Append(gameObject.GetInstanceID());
                StreamInfo streamInfo = new StreamInfo(ls.streamName, ls.streamType, ls.channelCount, LSL.LSL.IRREGULAR_RATE, channel_format_t.cf_string, hash.ToString());
                ls.outlet = new StreamOutlet(streamInfo);
                lSLStreams[i] = ls;
                if(ls.letType == LetType.TRIALMARKERS){
                    experimentMarkerIndex = i;
                }
            }
            else{
                StartCoroutine(InitializeInlet(ls, i));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator InitializeInlet(LSLStream ls, int index)
{
    Debug.Log("Looking for LSL stream: " + ls.streamName);

    while (true)
    {
        var results = LSL.LSL.resolve_stream("name", ls.streamName, 1, 5.0);

        if (results.Length > 0)
        {
            ls.inlet = new StreamInlet(results[0]);
            ls.receivedData = new List<float>(); // Initialize the queue
            lSLStreams[index] = ls;  // Update inlet in the list

            Debug.Log("Inlet created for stream: " + ls.streamName);

            // Start listening coroutine after initializing the inlet
            StartCoroutine(ListenToInlet(ls, index));
            break;
        }
        else
        {
            Debug.LogWarning("Stream not found: " + ls.streamName + ". Retrying...");
        }

        // Wait a moment before trying again
        yield return new WaitForSeconds(1.0f);
    }
}
    private IEnumerator ListenToInlet(LSLStream ls, int index)
    {
        Debug.Log("Listening to inlet for stream: " + ls.streamName);

        while (ls.inlet != null)
        {
            float[] sample = new float[ls.channelCount];
            double timestamp = ls.inlet.pull_sample(sample, 0.0f);

            if (timestamp != 0)
            {
                // Add each element of the sample to receivedData
                foreach (float value in sample)
                {
                    if(value != 0){
                        ls.receivedData.Add(value);
                    }
                }

                // Optional: Limit the number of stored samples
                if (ls.receivedData.Count > maxSamples * ls.channelCount)
                {
                    ls.receivedData.RemoveRange(0, ls.channelCount);
                }

                Debug.Log("Received data on stream " + ls.streamName + ": " + string.Join(", ", sample));
            }

            // Adjust this delay based on your data rate
            yield return new WaitForSeconds(0.01f);
        }
    }
    public void SendStringToOutlet(LSLStream ls, string data)
    {
        if (ls.outlet != null)
        {
            // Send the string as a sample
            ls.outlet.push_sample(new string[] { data });
            Debug.Log("Sent string: " + data);
        }
        else
        {
            Debug.LogWarning("Outlet is not initialized for stream: " + ls.streamName);
        }
    }
    public void SendExperimentStep(ExperimentStep experimentStep){
        if(experimentMarkerIndex != -1){
            string message = stepConv[experimentStep].ToString()+"_"+taskManager.currentTask.ToString()+"_"+taskManager.currentDifficulty.ToString()+"_"+taskManager.currentCondition.ToString();
            SendStringToOutlet(lSLStreams[experimentMarkerIndex],message);
        }
    }
}
[Serializable]
public struct LSLStream{
    public string streamName;
    public LetType letType;
    public string streamType;
    public int channelCount;
    public bool outStream;
    public bool workloadStream;
    public StreamOutlet outlet;
    public StreamInlet inlet;
    public List<float> receivedData; // Buffer to store latest received data
}
public enum LetType {TRIALMARKERS,TRIGGERPRESS,WORKLOAD};