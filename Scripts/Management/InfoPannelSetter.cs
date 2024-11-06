using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPannelSetter : MonoBehaviour
{
    public TMP_Text infoText;
    
    public bool isMonitoring;
    public TaskType tracking;
    public TaskManager tm;

    public ConveyorBelt conveyorBelt;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isMonitoring){
            switch(tracking){
                case TaskType.BOXING:
                    UpdateBox();
                    break;
                case TaskType.COLORSHAPE:
                    UpdateColorShape();
                    break;
                case TaskType.GONOGO:
                    UpdateSort();
                    break;
                case TaskType.MATCHING:
                    UpdateMatch();
                    break;
                case TaskType.NBACK:
                    UpdateNBack();
                    break;
            }
        }
    }
    public void StartMonitoring(Task t){
        tracking = t.taskType;
        isMonitoring = true;
    }
    public void StopMonitoring(){
        
        isMonitoring=false;
    }
    public static readonly Dictionary<int, string> ColorCode
        = new Dictionary<int, string>
    {
        { 1, "Green" },
        { 2, "Red" }
    };
    public static readonly Dictionary<ItemType, string> ItemTypeCode
        = new Dictionary<ItemType, string>
    {
        { ItemType.ITEMA, "Color - Green" },
        { ItemType.ITEMB, "Color - Red" }
    };

    public void UpdateBox(){
        string info = "";
        int cptbox = 0;
        foreach(ValidationTray vt in conveyorBelt.vt){
            if(vt.type == ValidationTrayType.BOX && vt.activated){
                BoxingTray bt = (BoxingTray)vt;
                string box = "";
                switch(cptbox){
                    case 0:
                        box+="Left box ";
                        break;
                    case 1:
                        box+="Right box ";
                        break;
                    default:
                        box+="Box ";
                        break;
                }
                cptbox++;
                Box bb = bt.box.GetComponent<Box>();
                
                box +="requires "+bb.weightRequirement.ToString()+"kg.\n - Shapes: ";
                bool first = false;
                foreach(ItemShape itemShape in bb.itemsShapeGoals){
                    if(first){box+=" or ";}
                    box+=itemShape.ToString();
                    first = true;
                }
                box+="\n - Colors: ";
                first = false;
                foreach(ItemColor itemColor in bb.itemsColorGoals){
                    if(first){box+=" or ";}
                    box+=itemColor.ToString();
                    first = true;
                }
                if(bb.shapeRequirements.Count!=0 || bb.colorRequirements.Count!=0|| bb.weightRequirements.Count!=0 || bb.numberRequirement !=0){
                    box+="\nSpecial requirements!\n Box requires : \n";
                    if(bb.exactNumber && bb.numberRequirement != 0){
                        box+=" - EXACTLY "+bb.numberRequirement.ToString()+" items\n";
                    }
                    if(!bb.exactNumber && bb.numberRequirement != 0){
                        box+=" - UP TO "+bb.numberRequirement.ToString()+" items\n";
                    }
                    foreach(ShapeRequirement tp in bb.shapeRequirements){//Tuple<ItemShape,int,bool>
                        if(tp.exactNumber){
                            box+=" - EXACTLY "+tp.number.ToString()+" "+tp.itemShape.ToString()+"\n";
                        }
                        else {
                            box+=" - UP TO "+tp.number.ToString()+" "+tp.itemShape.ToString()+"\n";
                        }
                    }
                    foreach(ColorRequirement tp in bb.colorRequirements){//Tuple<ItemColor,int,bool>
                        if(tp.exactNumber){
                            box+=" - EXACTLY "+tp.number.ToString()+" "+tp.itemColor.ToString()+"\n";
                        }
                        else {
                            box+=" - UP TO "+tp.number.ToString()+" "+tp.itemColor.ToString()+"\n";
                        }
                    }
                }
                info+=box+"\n";
            }
        }
        infoText.text = info;
    }
    public void UpdateSort(){
        string info = "Requirements : \n";
        int cptbox = 0;
        foreach(ValidationTray vt in conveyorBelt.vt){
            if(vt.type == ValidationTrayType.BINS && vt.activated){
                string box = "The bin";
                box +="requires : ";
                bool first = false;
                foreach(Tuple<ItemShape,ItemColor,string> tp in vt.receiving){
                    if(first){
                        box+=" and ";
                    }
                    box+=tp.Item2.ToString()+" "+tp.Item1.ToString();
                    
                    bool isNumber = int.TryParse(tp.Item3, out _);
                    if(tp.Item3 != "0" && isNumber){
                        box+=" with number "+tp.Item3.ToString();
                    }
                    else {
                        box+=" with Letter "+tp.Item3.ToString();
                    }
                }
                
                info+=box+"\n";
            }
            cptbox++;
        }
        info+="Press the controller to have the object go in the bin";
        infoText.text = info;
    }
    public void UpdateMatch(){
        string info = "Place connecting cable that connects red dot without touching black ones\n";
        infoText.text = info;
    }
    public void UpdateNBack(){
        Task t = tm.currentTask;
        int nbackNumber = 0;
        foreach (NBack nbt in tm.nBackTasks){
            if(nbt.taskDifficulty == t.taskDifficulty){
                nbackNumber = nbt.nbackNumber;
            }
        }
        string info = "Ensure no repetition of same exact object in the next ";
        if(nbackNumber == 1){
            info +=" object";
        }
        else{
            info +=nbackNumber.ToString()+" objects";
        }
        info+="\n Remove any repeated object";
        infoText.text = info;
    }
    public void UpdateColorShape(){
        TaskDifficulty td = tm.currentDifficulty;
        ColorShapeTask cst=tm.colorShapeTasks[0];
        for (int j = 0; j < tm.colorShapeTasks.Count; j++){
            if(tm.colorShapeTasks[j].taskDifficulty==td){
                cst = tm.colorShapeTasks[j];
            }
        }
        string text = "";
        if(!cst.hasHyperDimension){
            text = "Sort by "+ cst.currentDimension.ToString();
            switch(cst.currentDimension){
                case ObjectDimension.COLOR:
                    text+="\n - Left Bin : "+ cst.colorSort.Item1;
                    text+="\n - Right Bin : "+ cst.colorSort.Item2;
                    break;
                case ObjectDimension.SHAPE:
                    text+="\n - Left Bin : "+ cst.shapeSort.Item1;
                    text+="\n - Right Bin : "+ cst.shapeSort.Item2;
                    break;
                case ObjectDimension.TEXT:
                    text+="\n - Left Bin : "+ cst.textSort.Item1;
                    text+="\n - Right Bin : "+ cst.textSort.Item2;
                    break;
            }
            
        }
        else if(cst.hasHyperDimension){
            ObjectDimension od = cst.currentDimension;
            string newSort="Initial sorting : "+od.ToString();
            switch(od){
                case ObjectDimension.COLOR:
                    List<ItemColor> colorSplit = new List<ItemColor>(cst.colorSorting.Keys);
                    text+="If object is color "+ colorSplit[0]+" sort by "+cst.colorSorting[colorSplit[0]]+"\n";
                    text+="If object is color "+ colorSplit[1]+" sort by "+cst.colorSorting[colorSplit[1]]+"\n";
                    
                    text+=" - Left Bin : "+ cst.shapeSort.Item1.ToString() + " or "+ cst.textSort.Item1.ToString();
                    text+=" - Right Bin : "+ cst.shapeSort.Item2.ToString() + " or "+ cst.textSort.Item2.ToString();
                    break;
                case ObjectDimension.TEXT:
                    List<ItemText> textSplit = new List<ItemText>(cst.textSorting.Keys);
                    text+="If text is a  "+ textSplit[0]+" sort by "+cst.textSorting[textSplit[0]]+"\n";
                    text+="If text is a  "+ textSplit[1]+" sort by "+cst.textSorting[textSplit[1]]+"\n";
                    text+=" - Left Bin : "+ cst.shapeSort.Item1.ToString() + " or "+ cst.colorSort.Item1.ToString();
                    text+=" - Right Bin : "+ cst.shapeSort.Item2.ToString() + " or "+ cst.colorSort.Item2.ToString();
                    break;
                case ObjectDimension.SHAPE:
                    List<ItemShape> shapeSplit = new List<ItemShape>(cst.shapeSorting.Keys);
                    text+="If object shape is "+ shapeSplit[0]+" sort by "+cst.shapeSorting[shapeSplit[0]]+"\n";
                    text+="If object shape is "+ shapeSplit[1]+" sort by "+cst.shapeSorting[shapeSplit[1]]+"\n";
                    text+=" - Left Bin : "+ cst.colorSort.Item1.ToString() + " or "+ cst.textSort.Item1.ToString();
                    text+=" - Right Bin : "+ cst.colorSort.Item2.ToString() + " or "+ cst.textSort.Item2.ToString();
                    break;

            }
        }
        infoText.text = text;
    }
}
