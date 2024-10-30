using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskManager : MonoBehaviour
{
    public List<Task> tasks = new List<Task>();
    public bool onGoingTask;

    // Task parameters
    public int task1stDuration;
    public int task2ndDuration;
    public TMP_Text informationDisplay;
    private float TaskStart;
    private bool instructionsDone = false;

    [Header("Task tools")]
    public ValidationTablet vTablet;
    public QuestionnaireTablet qTablet;
    public CountingTablet cTablet;
    //Objects parameters
    public ConveyorBelt belt;
    public Spawner spawner;
    public float lastDelivery;
    public List<GameObject> bins = new List<GameObject>();
    public InfoPannelSetter infoPannelSetter;
    public GameObject prefabItem;

    [Header("Experimentation")]
    public ParticipantInfos participantInfos;
    public List<ExperimentPart> experimentFirstPart;
    public List<ExperimentPart> experimentSecondPart;
    public bool taskOngoing,breakOngoing,questionnaireOngoing;

    [Header("User models")]
    public List<UserModel> models;
    public ConditionType currentCondition;
    public TaskType currentTask;
    public TaskDifficulty currentDifficulty;
    public Questionnaire nasa,stfa,comp;
    public int nbValidationTask;
    public MovementRecorderCSV movementRecorderCSV;
    public bool recordMovements;
    [Header("Testing tool")]
    public bool instructions;
    public bool validationtablet;
    public bool launchTasks;
    public bool launchingTasks;
    public bool clearBatch;
    public bool verbose;

    public List<BoxingTask> boxingTasks;
    public List<NBack> nBackTasks;
    // Start is called before the first frame update
    void Start()
    {
        for( int i = 0; i < tasks.Count; i++){
             
            if(tasks[i].taskType == TaskType.BOXING){
                Debug.Log("Setup tasks items "+tasks[i].taskType.ToString());
                Task tt = tasks[i];
                foreach(BoxingTask boxingTask in boxingTasks){
                    if(boxingTask.taskDifficulty == tasks[i].taskDifficulty){
                        tt.boxingTask = boxingTask;
                    }
                }
                tt.items.itemShape = tt.boxingTask.itemShapes;
                tt.items.itemColor = tt.boxingTask.itemColors;
                tt.items.itemNumber = tt.boxingTask.itemNumbers;
                tasks[i]=GenerateItemsTask(tt);
            }    
            else if(tasks[i].items.used){
                Debug.Log("Setup tasks items "+tasks[i].taskType.ToString());
                tasks[i]=GenerateItemsTask(tasks[i]);
            }       
        }
        for (int i = 0; i < nBackTasks.Count; i++){
            NBack nb = nBackTasks[i];
            
            var (nBackList, nBackCount, nBackIndicators) = GenerateNBackList(nb.minNumber, nb.maxNumber, 200, 20, nb.nbackNumber);
            nb.numbers = nBackList;
            nb.nbacks = nBackIndicators;
            nBackTasks[i]=nb;
        }

    }
    public Task GenerateItemsTask(Task t){
        Items items = t.items;
        t.objects = new List<GameObject>();
        foreach(ItemShape itemShape in items.itemShape){
            foreach(ItemColor itemColor in items.itemColor){
                foreach(int itemNumber in items.itemNumber){
                    GameObject it = Instantiate(prefabItem,Vector3.zero,Quaternion.identity);
                    Item i = it.GetComponent<Item>();
                    i.itemColor = itemColor;
                    i.itemShape = itemShape;
                    i.itemNumber = itemNumber;
                    i.SetUpItem();
                    it.SetActive(false);
                    //it.transform.position = new Vector3(1000,1000,1000);
                    t.objects.Add(it);
                }
            }   
        }
        return t;
    }

    // Update is called once per frame
    void Update()
    {
        if(clearBatch){
            clearBatch=false;
            StartCoroutine(belt.EmptyBelt());
        }
        if(launchTasks){
            launchTasks=false;
            launchingTasks=true;
            StartCoroutine(RunExperiment());
        }
    }

    public IEnumerator StartTask(Task t){
        if(verbose){
            Debug.Log("Task started");
        }
        currentTask = t.taskType;
        participantInfos.StartNewTask(t,currentTask,currentCondition);
        taskOngoing=true;
        //1. Task setup
        string name = t.Name;
        int duration=task1stDuration;
        t.duration = task1stDuration;
        List<GameObject> objects=t.objects;
        List<string> objectGoal=t.objectGoal;
        bool continuousBelt=t.continuousBelt;
        float deliveryTime=t.deliveryTime;
        MicroTaskEnd stop=t.stop;
        
        //Instantiate validation trays
        int nbValidationTrays=t.nbValidationTrays;
        ValidationTrayType validationType=t.validationType;
        List<ItemType> traysReceivers = t.traysReceivers; 

        //Initializing belt & spawner
        yield return StartCoroutine(belt.InitialyzeBelt(t));
        Debug.Log("Belt initialyzed");

        // 2. Instructions
        informationDisplay.text=t.initInstructions;//Display instructions
        instructionsDone = false;
        //Present tablet to stop instructions
        if(validationtablet){
            yield return StartCoroutine(vTablet.StartTablet());
            while(!instructionsDone){
                instructionsDone=vTablet.GetValidation();
                yield return new WaitForSeconds(Time.deltaTime);
            }
            yield return StartCoroutine(vTablet.EndTablet());

        }

        // Tablet initialization for counting task
        informationDisplay.text=t.duringInstructions;
        
        if(t.taskType == TaskType.COUNTING){yield return StartCoroutine(cTablet.StartTablet());}
        if(t.taskType == TaskType.MATCHING){belt.StartDelivery(t.baseObjects);}
        if(t.taskType == TaskType.BOXING){belt.StartBoxing(t.baseObjects);}
        //3. Task start
        if(recordMovements){
            movementRecorderCSV.StartRecording();
        }
        TaskStart = Time.time;
        infoPannelSetter.StartMonitoring(t);
        yield return StartCoroutine(spawner.StartSpawning(t));
        while(spawner.spawning){
            yield return null;
        }
        if(verbose){Debug.Log("Spawning ended");}
        if(t.taskType == TaskType.MATCHING){belt.StopDelivery();}
        if(t.taskType == TaskType.BOXING){belt.StopBoxing();}
        if(verbose){
            Debug.Log("Task ended");
        }
        infoPannelSetter.StopMonitoring();
        
        if(t.taskType == TaskType.COUNTING){yield return StartCoroutine(cTablet.EndTablet());}
        yield return StartCoroutine(belt.DeinitialyseBelt(t));
        while(belt.deintialysingBelt){yield return null;}
        taskOngoing=false;
        
        if(recordMovements){
            movementRecorderCSV.StopRecording();
        }
        participantInfos.EndTask();
        Debug.Log("Waiting for clearance");
        yield return StartCoroutine(belt.EmptyBelt());
        while(belt.spawner.CheckClearedBatch()){
            yield return new WaitForSeconds(Time.deltaTime);
        }
        
        belt.spawner.stopper.SetActive(true);
        
        Debug.Log("Clearance attained");

        yield break;
    }
    
    public IEnumerator StartBreak(){
        breakOngoing = true;
        if(verbose){
            Debug.Log("Break started");
        }
        // Instructions
        informationDisplay.text="Break Time !\nTake your time to breath. If you need the VR headset removed, please ask the experimenter.\n Click validate when you are ready to continue";//Display instructions
        instructionsDone = false;
        //Present tablet to stop instructions
        yield return StartCoroutine(vTablet.StartTablet());
        while(!instructionsDone){
            instructionsDone=vTablet.GetValidation();
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return StartCoroutine(vTablet.EndTablet());

        if(verbose){
            Debug.Log("Break ended");
        }
        breakOngoing = false;
        yield break;
    }
    public IEnumerator StartQuestionnaire(Questionnaire q,TaskType tt, ConditionType ct, TaskDifficulty td){
        if(verbose){Debug.Log("Questionnaire " +q.Name + " started");}
        questionnaireOngoing = true;
        yield return StartCoroutine(qTablet.LaunchQuestionnaire(q));
        QuestionnaireResults questionnaireResults = new QuestionnaireResults();
        questionnaireResults.conditionType=ct;
        questionnaireResults.taskType=tt;
        questionnaireResults.taskDifficulty=td;
        questionnaireResults.questionnaireType=q.questionnaireType;
        questionnaireResults.questionnaireAnswers = qTablet.answers;
        participantInfos.questionnaireResults.Add(questionnaireResults);
        questionnaireOngoing = false;
        if(verbose){Debug.Log("Questionnaire " +q.Name + " ended");}
    }

    public IEnumerator RunExperiment(){
        if(verbose){Debug.Log("Paradigm - Start XP");}
        //First part
        foreach(ExperimentPart ep in experimentFirstPart){
            //Task definition
            TaskType tt = ep.taskType;
            Task t = new Task();
            foreach (Task check in tasks){
                if(check.taskType==tt && check.taskDifficulty == ep.taskDifficulty){
                    t=check;
                }
            }

            currentTask = tt;
            currentDifficulty = t.taskDifficulty;
            currentCondition = ConditionType.CALIBRATION;

            //Task
            if(verbose){Debug.Log("Paradigm - Start Task");}
            yield return StartCoroutine(StartTask(t));



            //Break
            if(ep.postBreak){
                if(verbose){Debug.Log("Paradigm - Start Break");}
                yield return StartCoroutine(StartBreak());
            }
        }
        //Second part
        //Fatigue & stress Questionnaire
        
        //Model calibration
        if(verbose){Debug.Log("Paradigm - Start Calibration");}
        CalibrateModel();
        if(verbose){Debug.Log("Paradigm - Setup Validation Tasks");}

        SetValidationTasks(nbValidationTask);
        
        if(verbose){Debug.Log("Paradigm - Start Tablet");}
        
        currentCondition = ConditionType.PREVALIDATION;
        //Questionnaire
        yield return StartCoroutine(qTablet.StartTablet());
        if(verbose){Debug.Log("Paradigm - Start stfa");}
        yield return StartCoroutine(StartQuestionnaire(stfa,TaskType.PREVALIDATION,ConditionType.PREVALIDATION,TaskDifficulty.NONE));
        if(verbose){Debug.Log("Paradigm - End Tablet");}
        yield return StartCoroutine(qTablet.EndTablet());

        
        if(verbose){Debug.Log("Paradigm - Start Validation");}
        foreach(ExperimentPart ep in experimentSecondPart){
            //Task definition
            TaskType tt = ep.taskType;
            Task t = new Task();
            foreach (Task check in tasks){
                if(check.taskType==tt){
                    t=check;
                }
            }

            currentTask = tt;
            currentDifficulty = t.taskDifficulty;
            currentCondition = ConditionType.VALIDATION;
            //Task
            if(verbose){Debug.Log("Paradigm - Start Task");}
            yield return StartCoroutine(StartTask(t));


            if(verbose){Debug.Log("Paradigm - Start Tablet");}
            //Questionnaire
            yield return StartCoroutine(qTablet.StartTablet());
            
            //NASA Questionnaire
            if(verbose){Debug.Log("Paradigm - Start NASA");}
            if(ep.nasaQ){yield return StartCoroutine(StartQuestionnaire(nasa,tt,ConditionType.VALIDATION,ep.taskDifficulty));}
            
            //Fatigue & stress Questionnaire
            if(verbose){Debug.Log("Paradigm - Start stfa");}
            if(ep.stfaQ){yield return StartCoroutine(StartQuestionnaire(stfa,tt,ConditionType.VALIDATION,ep.taskDifficulty));}
            
            //Comparison Questionnaire
            if(verbose){Debug.Log("Paradigm - Start Comb");}
            if(ep.compQ){yield return StartCoroutine(StartQuestionnaire(comp,tt,ConditionType.VALIDATION,ep.taskDifficulty));}

            if(verbose){Debug.Log("Paradigm - End Tablet");}
            yield return StartCoroutine(qTablet.EndTablet());

            //Break
            if(ep.postBreak){
                if(verbose){Debug.Log("Paradigm - Start Break");}
                yield return StartCoroutine(StartBreak());
            }
        }
        yield break;
    }

    public void CalibrateModel(){
        foreach (UserModel um in models){
            um.ComputeSolution(participantInfos.taskResults);
        }
    }

    public void SetValidationTasks(int taskNumber){
        foreach(UserModel um in models){
            List<TaskType> taskTypes = um.ProvideTasks(taskNumber);
            foreach (TaskType tt in taskTypes){
                ExperimentPart ep = new ExperimentPart();
                ep.conditionType = ConditionType.VALIDATION;
                ep.taskType = tt;
                experimentSecondPart.Add(ep);
            }
        }
    }

    public static (List<int> generatedList, int nBackCount, List<bool> nBackIndicators) GenerateNBackList(int a, int b, int X, float percentage, int n)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100.");
        }

        // Calculate Y as a percentage of X
        int Y = Math.Max(1, (int)((percentage / 100) * (X - n))); // Ensure at least 1 n-back item

        // Initialize the list with random values and the indicator list with false values
        System.Random random = new System.Random();
        List<int> result = new List<int>(new int[X]);
        List<bool> nBackIndicators = new List<bool>(new bool[X]);
        
        for (int i = 0; i < X; i++)
        {
            result[i] = random.Next(a, b + 1); // b is exclusive, hence +1
            nBackIndicators[i] = false; // Initialize all indicators as false
        }

        // Keep track of used positions
        HashSet<int> usedPositions = new HashSet<int>();

        // Insert Y n-back items
        for (int i = 0; i < Y; i++)
        {
            while (true)
            {
                // Choose a position to insert a matching item n back
                int position = random.Next(n, X); // n to X - 1

                // Check if the position and position-n are already used
                if (!usedPositions.Contains(position) && !usedPositions.Contains(position - n))
                {
                    result[position] = result[position - n]; // Match the item n positions back
                    usedPositions.Add(position);             // Mark the current position as used
                    usedPositions.Add(position - n);         // Mark the n-back position as used
                    nBackIndicators[position] = true;        // Mark this position as part of the n-back task
                    break;                                   // Exit the loop if a valid position is found
                }
            }
        }

        return (result, Y, nBackIndicators); // Return the list, the count of n-back values, and the indicator list
    }

}


[Serializable]
public struct Task{
    public string Name;
    public string initInstructions;
    public string duringInstructions;
    public TaskType taskType;
    public TaskDifficulty taskDifficulty;
    public int duration;
    public Items items;
    public List<GameObject> objects;
    public List<GameObject> baseObjects;
    public List<string> objectGoal;
    public int batchAmount;
    public bool continuousBelt;
    public bool continuousBatch;
    public float deliveryTime;
    public MicroTaskEnd stop;
    public bool noFailure;
    
    public int nbValidationTrays;
    public ValidationTrayType validationType;
    public List<ItemType> traysReceivers; //Length should be equal to nbValidationTrays
    public BoxingTask boxingTask;
}

[Serializable]
public struct Questionnaire{
    public string Name;
    public QuestionnaireType questionnaireType;
    public List<string> questions;
    public List<string> tmin;
    public List<string> tmax;
    public List<bool> withSlider;
}

[Serializable]
public struct ExperimentPart{
    public ConditionType conditionType;
    public TaskType taskType;
    public TaskDifficulty taskDifficulty;
    public int taskDuration;
    public bool nasaQ,stfaQ,compQ;
    public bool postBreak;
}
[Serializable]
public struct Items{
    public List<ItemShape> itemShape;
    public List<ItemColor> itemColor;
    public List<int> itemNumber;

    public bool used;
}

[Serializable]
public struct ItemsRequirements{
    public RequirementType requirementType;
    public ItemShape itemShape;
    public ItemColor itemColor;
    public int itemNumber;
    public bool exactValue;
}
public enum RequirementType {COLOR,SHAPE,NUMBER};
public enum ConditionType {CALIBRATION,VALIDATION,PREVALIDATION};
public enum MicroTaskEnd {NONE,BUTTONPRESS,DELIVERY};

public enum TaskType {SORTING,MATCHING,ASSEMBLY,QUALITY,BOXING,COUNTING,NBACK,PREVALIDATION};

public enum TaskDifficulty {NONE,LOW,LOWMEDIUM, MEDIUM, MEDIUMHIGH,HIGH};

[Serializable]
public struct BoxingTask{
    public TaskDifficulty taskDifficulty;

    public List<ItemShape> itemShapes;
    public List<ItemColor> itemColors;
    public List<int> itemNumbers;

    public List<BoxingRequirements> boxingRequirements;
}
[Serializable]
public struct BoxingRequirements{
    public List<ItemShape> itemShapes;
    public List<ItemColor> itemColors;
    public int weight;

    public List<ShapeRequirement> shapesRequirements;
    public List<ColorRequirement> colorRequirements;
    public List<WeightRequirement> weightRequirements;
    public List<NumberRequirement> numberRequirements;
}
[Serializable]
public struct ShapeRequirement{
    public ItemShape itemShape;
    public int number;
    public bool exactNumber;
}
[Serializable]
public struct ColorRequirement{
    public ItemColor itemColor;
    public int number;
    public bool exactNumber;
}
[Serializable]
public struct WeightRequirement{
    public int weight;
    public int number;
    public bool exactNumber;
}
[Serializable]
public struct NumberRequirement{
    public int number;
    public bool exactNumber;
}

[Serializable]
public struct NBack{
    public TaskDifficulty taskDifficulty;
    public int nbackNumber;
    public int minNumber,maxNumber;
    public List<int> numbers;
    public List<bool> nbacks;
}