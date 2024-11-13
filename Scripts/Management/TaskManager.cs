using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
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
    public InfoPannelSetter infoPannelSetter;
    public GameObject prefabItem;

    [Header("Experimentation")]
    public ParticipantInfos participantInfos;
    public LSLManager lSLManager;
    public List<ExperimentPart> experimentFirstPart;
    public List<ExperimentPart> experimentSecondPart;
    public bool taskOngoing,breakOngoing,questionnaireOngoing;

    [Header("User models")]
    public List<UserModel> models;
    public ConditionType currentCondition;
    public TaskType currentTaskType;
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
    public bool doSecondPart;

    public List<NBack> nBackTasks;
    public Task currentTask;
    [Header("ColorShapeTask")]
    public List<ObjectDimension> colorShapeDimensionEasy;
    public List<int> objectUntilChangeEasy;
    public List<ObjectDimension> colorShapeDimensionMedium;
    public List<int> objectUntilChangeMedium;
    public List<ObjectDimension> colorShapeDimensionHard;
    public List<int> objectUntilChangeHard;
    public List<ColorShapeTask> colorShapeTasks;
    public AudioClip soundColorShapeChange;
    public float changeColorShapeTime;

    [Header("Go NoGo tasks")]
    public List<GonoGoTask> gonoGoTasks;
    // Start is called before the first frame update
    void Start()
    {
        colorShapeTasks.Add(new ColorShapeTask(colorShapeDimensionEasy,TaskDifficulty.LOW, this));
        colorShapeTasks.Add(new ColorShapeTask(colorShapeDimensionMedium,TaskDifficulty.MEDIUM, this));
        colorShapeTasks.Add(new ColorShapeTask(colorShapeDimensionHard,TaskDifficulty.HIGH, this));
        for( int i = 0; i < tasks.Count; i++){
             
            if(tasks[i].items.used){
                Debug.Log("Setup tasks items "+tasks[i].taskType.ToString());
                tasks[i]=GenerateItemsTask(tasks[i]);
            }       
        }
        int relatedTask = -1;
        for (int i = 0; i < nBackTasks.Count; i++){
            for( int j = 0; j < tasks.Count; j++){
                if(tasks[j].taskType == TaskType.NBACK && tasks[j].taskDifficulty == nBackTasks[i].taskDifficulty){
                    relatedTask = j;
                }
                    
            }
            if(relatedTask!=-1){
                NBack nb = nBackTasks[i];
                
                var (nBackList, nBackCount, nBackIndicators) = GenerateNBackList(0, tasks[relatedTask].objects.Count-1, 200, 20, nb.nbackNumber);
                nb.numbers = nBackList;
                nb.nbacks = nBackIndicators;
                nBackTasks[i]=nb;
            }
            else{
                NBack nb = nBackTasks[i];
                
                var (nBackList, nBackCount, nBackIndicators) = GenerateNBackList(0, nb.maxNumber, 200, 20, nb.nbackNumber);
                nb.numbers = nBackList;
                nb.nbacks = nBackIndicators;
                nBackTasks[i]=nb;
            }
            
            
        }

    }
    public Task GenerateItemsTask(Task t){
        Items items = t.items;
        t.objects = new List<GameObject>();
        foreach(ItemShape itemShape in items.itemShape){
            foreach(ItemColor itemColor in items.itemColor){
                if(items.itemText.Count>0){
                    foreach(string itemText in items.itemText){
                        GameObject it = Instantiate(prefabItem,Vector3.zero,Quaternion.identity);
                        Item i = it.GetComponent<Item>();
                        i.itemColor = itemColor;
                        i.itemShape = itemShape;
                        i.itemText = itemText;
                        i.SetUpItem();
                        it.SetActive(false);
                        //it.transform.position = new Vector3(1000,1000,1000);
                        t.objects.Add(it);
                    }
                }
                else{
                    GameObject it = Instantiate(prefabItem,Vector3.zero,Quaternion.identity);
                    Item i = it.GetComponent<Item>();
                    i.itemColor = itemColor;
                    i.itemShape = itemShape;
                    i.itemText = "0";
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
        currentTask = t;
        if(verbose){
            Debug.Log("Task started");
        }
        lSLManager.SendExperimentStep(ExperimentStep.TASKSTART);
        currentTaskType = t.taskType;
        participantInfos.StartNewTask(t,currentTaskType,currentCondition,currentDifficulty);
        taskOngoing=true;
        //1. Task setup
        int duration=task1stDuration;
        t.duration = task1stDuration;
        List<GameObject> objects=t.objects;
        List<string> objectGoal=t.objectGoal;
        bool continuousBelt=t.continuousBelt;
        float deliveryTime=t.deliveryTime;
        
        //Instantiate validation trays
        int nbValidationTrays=t.nbValidationTrays;
        ValidationTrayType validationType=t.validationType;
        prepTasks();
        //Initializing belt & spawner
        yield return StartCoroutine(belt.InitialyzeBelt(t));
        Debug.Log("Belt initialyzed");

        // 2. Instructions
        informationDisplay.text=t.initInstructions;//Display instructions
        informationDisplay.text+="\nTouch the green button to start the task.";
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

        string filePath = Application.dataPath+"Logs"+".txt";
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            writer.WriteLine("start task");
        }
        // Tablet initialization for counting task
        informationDisplay.text=t.duringInstructions+ "\n Touch the button when ready to start";
        
        if(t.taskType == TaskType.COUNTING){yield return StartCoroutine(cTablet.StartTablet());}
        if(t.taskType == TaskType.MATCHING){belt.StartDelivery(t.baseObjects);}
        //3. Task start
        if(recordMovements){
            movementRecorderCSV.StartRecording();
        }
        TaskStart = Time.time;
        infoPannelSetter.StartMonitoring(t);
        yield return new WaitForSeconds(3);
        yield return StartCoroutine(spawner.StartSpawning(t));
        while(spawner.spawning){
            yield return null;
        }
        if(verbose){Debug.Log("Spawning ended");}
        if(t.taskType == TaskType.MATCHING){belt.StopDelivery();}
        
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            writer.WriteLine("end task");
        }
        if(verbose){
            Debug.Log("Task ended");
        }
        infoPannelSetter.StopMonitoring();
        
        if(t.taskType == TaskType.COUNTING){yield return StartCoroutine(cTablet.EndTablet());}
        yield return StartCoroutine(belt.DeinitialyseBelt(t));
        while(belt.deintialysingBelt){yield return null;}
        taskOngoing=false;
        
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            writer.WriteLine("uniti task");
        }
        if(recordMovements){
            movementRecorderCSV.StopRecording();
        }
        participantInfos.EndTask();
        Debug.Log("Waiting for clearance");
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            writer.WriteLine("Waiting for clearance");
        }
        yield return StartCoroutine(belt.EmptyBelt());
        while(!belt.isEmpty()){
            yield return new WaitForSeconds(Time.deltaTime);
        }
        
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            writer.WriteLine("belt cleared");
        }
        belt.spawner.stopper.SetActive(true);
        
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            writer.WriteLine("end task");
        }
        
        lSLManager.SendExperimentStep(ExperimentStep.TASKEND);
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

            currentTaskType = tt;
            currentDifficulty = t.taskDifficulty;
            currentCondition = ConditionType.CALIBRATION;

            //Task
            if(verbose){Debug.Log("Paradigm - Start Task");}
            yield return StartCoroutine(StartTask(t));
            string filePath = Application.dataPath+"Logs"+".txt";
            using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
            {
                writer.WriteLine("Prequestionnaire");
            }
            if(ep.nasaQ || ep.stfaQ || ep.compQ){
                lSLManager.SendExperimentStep(ExperimentStep.QUESTIONNAIRESTART);
                belt.DeinitialyseBelt(t);
                using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
                {
                    writer.WriteLine("Belt");
                }
                yield return StartCoroutine(qTablet.StartTablet());
                using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
                {
                    writer.WriteLine("tablet started");
                }
            }
            if(ep.nasaQ){
                yield return StartCoroutine(StartQuestionnaire(nasa,currentTaskType,currentCondition,currentDifficulty));
            }if(ep.stfaQ){
                yield return StartCoroutine(StartQuestionnaire(stfa,currentTaskType,currentCondition,currentDifficulty));
            }if(ep.compQ){
                yield return StartCoroutine(StartQuestionnaire(comp,currentTaskType,currentCondition,currentDifficulty));
            }
            if(ep.nasaQ || ep.stfaQ || ep.compQ){
                
                yield return StartCoroutine(qTablet.EndTablet());
                lSLManager.SendExperimentStep(ExperimentStep.QUESTIONNAIREEND);
            }
            //Break
            if(ep.postBreak){
                if(verbose){Debug.Log("Paradigm - Start Break");}
                yield return StartCoroutine(StartBreak());
            }
        }
        participantInfos.SaveQuestionnaireResultsToFile();
        participantInfos.SaveTaskResultsToFile();
        //Second part
        //Fatigue & stress Questionnaire
        if(doSecondPart){
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

                currentTaskType = tt;
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

    public IEnumerator UpdateColorShapeTask(){
        ColorShapeTask cst; 
        List<ObjectDimension> ods;
        int currentTasColor= -1;
        switch(currentDifficulty){
            case TaskDifficulty.LOW:
                cst = colorShapeTasks[0];
                currentTasColor=0;
                ods = colorShapeDimensionEasy;
                break;
            case TaskDifficulty.MEDIUM:
                cst = colorShapeTasks[1];
                currentTasColor=1;
                ods = colorShapeDimensionMedium;
                break;
            case TaskDifficulty.HIGH:
                cst = colorShapeTasks[2];
                currentTasColor=2;
                ods = colorShapeDimensionHard;
                break;
            default:
                cst = colorShapeTasks[0];
                currentTasColor=0;
                ods = colorShapeDimensionEasy;
                break;
        }
        cst.nbObjectSinceChange+=1;
        if(cst.nbObjectSinceChange>=cst.currentObjectsUntilChange){
            cst.nbObjectSinceChange=0;
            ObjectDimension od = cst.currentDimension;
            ObjectDimension newOd = od;
            if(ods.Count>1){
                while(newOd==od){
                    newOd = ods[UnityEngine.Random.Range(0,ods.Count)];
                }
                cst.currentDimension = newOd;
            }
            if(cst.swapMiniParameter){
                // Randomize the selection of the hyper parameter
                if(UnityEngine.Random.Range(0, 2)==0){
                    // Swap values in textSorting
                    var tempText = cst.textSorting[ItemText.NUMBER];
                    cst.textSorting[ItemText.NUMBER] =cst.textSorting[ItemText.LETTER];
                    cst.textSorting[ItemText.LETTER] = tempText;
                }
                if(UnityEngine.Random.Range(0, 2)==0){
                    // Swap values in shapeSorting
                    var tempShape = cst.shapeSorting[ItemShape.CUBE];
                    cst.shapeSorting[ItemShape.CUBE] = cst.shapeSorting[ItemShape.SPHERE];
                    cst.shapeSorting[ItemShape.SPHERE] = tempShape;
                }
                if(UnityEngine.Random.Range(0, 2)==0){
                    // Swap values in colorSorting
                    var tempColor = cst.colorSorting[ItemColor.RED];
                    cst.colorSorting[ItemColor.RED] = cst.colorSorting[ItemColor.GREEN];
                    cst.colorSorting[ItemColor.GREEN] = tempColor;
                }
            }
            infoPannelSetter.UpdateColorShape();
            if(soundColorShapeChange != null){
                AudioSource aso = gameObject.GetComponent<AudioSource>();
                if(aso != null){
                    aso.PlayOneShot(soundColorShapeChange);
                }
            }
            yield return new WaitForSeconds(changeColorShapeTime);
        }
        colorShapeTasks[currentTasColor]=cst;
        yield break;
    }

    public void prepTasks(){
        switch(currentTaskType){
            case TaskType.GONOGO:
                GonoGoTask g; int indGo=0;
                Task t; int indTa=0;
                for (int i = 0; i < gonoGoTasks.Count; i++){
                    if(gonoGoTasks[i].taskDifficulty == currentDifficulty){
                        indGo = i;
                    }
                }
                for (int i = 0; i < tasks.Count; i++){
                    if(tasks[i].taskDifficulty == currentDifficulty && tasks[i].taskType == TaskType.GONOGO){
                        indTa = i;
                    }
                }
                t = tasks[indTa];
                g = gonoGoTasks[indGo];
                Items it = tasks[indTa].items;
                List<ItemColor> itc = it.itemColor;
                List<ItemShape> its = it.itemShape;
                List<string> itt = it.itemText;
                g.aimedColor = itc[UnityEngine.Random.Range(0,itc.Count)];
                g.aimedShape = its[UnityEngine.Random.Range(0,its.Count)];
                string aimt="0";
                if(itt.Count>0){
                    aimt = itt[UnityEngine.Random.Range(0,itt.Count)];
                }
                 
                bool isNumber = int.TryParse(aimt, out _);
                g.aimedText = isNumber ? ItemText.NUMBER : ItemText.LETTER;
                List<GameObject> objects = t.objects;
                for(int j = 0; j < objects.Count; j++){
                    GameObject go = objects[j];
                    bool isTarget = true;
                    if(g.objectDimensions.Contains(ObjectDimension.COLOR) && go.GetComponent<Item>().itemColor != g.aimedColor){
                        isTarget = false;
                    }
                    if(g.objectDimensions.Contains(ObjectDimension.SHAPE) && go.GetComponent<Item>().itemShape != g.aimedShape){
                        isTarget = false;
                    }
                    bool isGoNumber = int.TryParse(go.GetComponent<Item>().itemText, out _);
                    if(g.objectDimensions.Contains(ObjectDimension.TEXT) && ((isGoNumber && g.aimedText != ItemText.NUMBER)|| (!isGoNumber && g.aimedText != ItemText.LETTER))){
                        isTarget = false;
                    }
                    go.GetComponent<Item>().target = isTarget;
                    objects[j]=go;
                }
                t.objects=objects;
                tasks[indTa]=t;
                gonoGoTasks[indGo]=g;
                break;
        }
    }
}


[Serializable]
public struct Task{
    public string name;
    public string initInstructions;
    public string duringInstructions;
    public TaskType taskType;
    public TaskDifficulty taskDifficulty;
    public int duration;
    public Items items;
    public List<GameObject> objects;
    public List<GameObject> baseObjects;
    public List<string> objectGoal;
    public bool continuousBelt;
    public float deliveryTime;
    public float beltSpeed;
    public bool noFailure;
    public bool blocker;
    public bool clearForSpawn;
    
    public int nbValidationTrays;
    public ValidationTrayType validationType;
    public void Initialize(){
        items = new Items();
        items.Initialize();
    }
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
    public string name;
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
    public List<string> itemText;

    public bool used;
    public void Initialize(){
        itemShape = new List<ItemShape>();
        itemColor = new List<ItemColor>();
        itemText = new List<string>();
        used = false;
    }
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

public enum TaskType {SORTING,MATCHING,ASSEMBLY,QUALITY,COUNTING,NBACK,PREVALIDATION,COLORSHAPE,GONOGO};

public enum TaskDifficulty {NONE,LOW, MEDIUM,HIGH};


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

public enum SpawnerType {CONTINUOUS,BATCH}

[Serializable]
public struct ColorShapeTask{
    public TaskDifficulty taskDifficulty;
    public List<ObjectDimension> hyperDimension;

    // Fixed parameters, which bin to put in
    public Tuple<ItemColor,ItemColor> colorSort;
    public Tuple<ItemShape,ItemShape> shapeSort;
    public Tuple<ItemText,ItemText> textSort;
    //Swappable parameters, hyper parameters decision
    /*public Tuple<ItemColor,ItemColor> colorSortHyper;
    public Tuple<ItemShape,ItemShape> shapeSortHyper;
    public Tuple<ItemText,ItemText> textSortHyper;*/

    public ObjectDimension currentDimension;
    public List<int> objectsUntilChange;
    public int currentObjectsUntilChange;
    public bool test;
    public bool swapMiniParameter;
    public bool hasHyperDimension;
    public Dictionary<ItemColor, string> colorSorting;
    public Dictionary<ItemShape, string> shapeSorting;
    public Dictionary<ItemText, string> textSorting;
    public int nbObjectSinceChange;
    public bool displayTextHelper;
    public  ColorShapeTask(List<ObjectDimension> objectDimensions, TaskDifficulty diff, TaskManager tm) { 
        textSorting = new Dictionary<ItemText, string>();
        textSorting[ItemText.NUMBER] = "COLOR";
        textSorting[ItemText.LETTER] = "SHAPE";
        
        shapeSorting = new Dictionary<ItemShape, string>();
        shapeSorting[ItemShape.CUBE] = "COLOR";
        shapeSorting[ItemShape.SPHERE] = "TEXT";
        
        colorSorting = new Dictionary<ItemColor, string>();
        colorSorting[ItemColor.RED] = "TEXT";
        colorSorting[ItemColor.GREEN] = "SHAPE";
        
        hyperDimension = objectDimensions;
        nbObjectSinceChange=0;
        displayTextHelper=false;
        if(diff==TaskDifficulty.LOW){
            taskDifficulty = TaskDifficulty.LOW;
            objectsUntilChange = tm.objectUntilChangeEasy;
            currentDimension = ObjectDimension.COLOR;
            swapMiniParameter=false;
            hasHyperDimension=false;
        }
        else if (diff == TaskDifficulty.MEDIUM){
            currentDimension = ObjectDimension.COLOR;
            taskDifficulty = TaskDifficulty.MEDIUM;
            objectsUntilChange = tm.objectUntilChangeMedium;
            swapMiniParameter=false;
            hasHyperDimension=true;
        }
        else{
            currentDimension = ObjectDimension.TEXT;
            taskDifficulty = TaskDifficulty.HIGH;
            objectsUntilChange = tm.objectUntilChangeHard;
            swapMiniParameter=true;
            hasHyperDimension=true;
        }
        currentObjectsUntilChange = objectsUntilChange[UnityEngine.Random.Range(0, objectsUntilChange.Count)];
        colorSort = new Tuple<ItemColor, ItemColor>(ItemColor.RED,ItemColor.GREEN);
        shapeSort = new Tuple<ItemShape, ItemShape>(ItemShape.CUBE,ItemShape.SPHERE);
        textSort = new Tuple<ItemText, ItemText>(ItemText.LETTER, ItemText.NUMBER);
        /*colorSortHyper = new Tuple<ItemColor, ItemColor>(ItemColor.RED,ItemColor.GREEN);
        shapeSortHyper = new Tuple<ItemShape, ItemShape>(ItemShape.SPHERE,ItemShape.CUBE);
        textSortHyper = new Tuple<ItemText, ItemText>(ItemText.NUMBER, ItemText.LETTER);*/
        test = true;

    }
}
[Serializable]
public struct GonoGoTask{
    public TaskDifficulty taskDifficulty;
    public List<ObjectDimension> objectDimensions;

    public ItemColor aimedColor;
    public ItemShape aimedShape;
    public ItemText aimedText;
    public int percentageGo;
}

public enum ItemText {NUMBER,LETTER};
[Serializable]
public enum ObjectDimension {COLOR,SHAPE,TEXT,NONE};
public enum ExperimentStep {TASKSTART,QUESTIONNAIRESTART,TASKEND,QUESTIONNAIREEND}