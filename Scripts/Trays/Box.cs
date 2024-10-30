using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Box : MonoBehaviour
{
    public List<GameObject> content;
    public int invalid;

    //public List<GoalsParameters> goals;
    public List<Tuple<ItemShape,ItemColor>> itemsGoals;
    public List<ItemShape> itemsShapeGoals;
    public List<ItemColor> itemsColorGoals;
    public List<ShapeRequirement> shapeRequirements;
    public List<ColorRequirement> colorRequirements;
    public List<WeightRequirement> weightRequirements;
    public int numberRequirement;
    public bool exactNumber;
    public int weightRequirement;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other){
        Item bi = other.gameObject.GetComponent<Item>();
        if(bi==null){
            bi = other.transform.parent.gameObject.GetComponent<Item>();
        }
        if(bi!=null){
            if(!CheckCorresponding(bi,true,true)){
                invalid+=1;
            }
            content.Add(other.gameObject);
        }
    }
    public void OnTriggerExit(Collider other){
        Item bi = other.gameObject.GetComponent<Item>();
        if(bi==null){
            bi = other.transform.parent.gameObject.GetComponent<Item>();
        }
        if(bi!=null){
            if(!CheckCorresponding(bi,true,true)){
                invalid-=1;
            }
            content.Remove(other.gameObject);
        }
    }
    public bool CheckCorresponding(Item boxedItem, bool updateValue, bool inverse){
        bool fit = false;
        ItemShape itemShape = boxedItem.itemShape;
        ItemColor itemColor = boxedItem.itemColor;
        int itemNumber = boxedItem.itemNumber;
        Tuple<ItemShape,ItemColor> tp = new Tuple<ItemShape, ItemColor>(itemShape,itemColor);
        
        if(itemsGoals.Contains(tp)){fit=true;}
        else if(itemsShapeGoals.Count==0 && itemsShapeGoals.Count==0){fit=true;}
        else if(itemsShapeGoals.Count!=0 &&itemsShapeGoals.Count==0 &&  itemsColorGoals.Contains(itemColor)){fit=true;}
        else if(itemsShapeGoals.Count==0 &&itemsShapeGoals.Count!=0 &&  itemsShapeGoals.Contains(itemShape)){fit=true;}
        else if(itemsShapeGoals.Count!=0 &&itemsColorGoals.Count!=0 &&  itemsShapeGoals.Contains(itemShape) && itemsColorGoals.Contains(itemColor)){fit=true;}
        
        
        
        
        if(fit){
            if(updateValue){
                if(inverse){
                    weightRequirement += itemNumber;
                }
                else{
                    weightRequirement -= itemNumber;
                }
            }
        }
        return fit;
        /*foreach(GoalsParameters goal in goals){
            bool correspond = false;
            foreach(BoxParameters boxParameters in boxedItem.parameters){
                if(boxParameters.name == goal.name){
                    if(goal.parameterType == ParameterType.REQUIREMENT && goal.goalValue == boxParameters.value){
                        correspond = true;
                    }
                    if(goal.parameterType == ParameterType.GOAL){
                        correspond = true;
                        goalParam = goal.name;
                        value = boxParameters.value;
                    }
                }
            }
            if(!correspond){
                fit = false;
            }
        }
        if(fit){
            if(updateValue){
                for (int i = 0; i < goals.Count; i++){
                    if(goals[i].name==goalParam){
                        GoalsParameters gp = goals[i];
                        if(inverse){
                            gp.currentValue -= value;
                        }
                        else{
                            gp.currentValue += value;
                        }
                        
                        goals[i] = gp;
                    }
                }
            }
            Tuple<string,float> ret = new Tuple<string, float>(goalParam,value);
            return ret;
        }
        else{
            Tuple<string,float> ret = new Tuple<string, float>("None",0f);
            return ret;
        }*/
        
    }

    public bool CheckValidity(bool clear){
        bool isValid = true;
        if(invalid>0){
            isValid=false;
        }
        foreach(ShapeRequirement tp in shapeRequirements){//Tuple<ItemShape,int,bool>
            int objCount = 0;
            foreach(GameObject c in content){
                Item i = c.GetComponent<Item>() ?? c.transform.parent.GetComponent<Item>();
                if(i.itemShape==tp.itemShape){
                    objCount+=1;
                }
            }
            if(objCount != tp.number&& tp.exactNumber){
                isValid = false;
            }
            else if(objCount > tp.number&& !tp.exactNumber){
                isValid = false;
            }
        }
        foreach(ColorRequirement tp in colorRequirements){//Tuple<ItemColor,int,bool>
            int objCount = 0;
            foreach(GameObject c in content){
                Item i = c.GetComponent<Item>() ?? c.transform.parent.GetComponent<Item>();
                if(i.itemColor==tp.itemColor){
                    objCount+=1;
                }
            }
            if(objCount != tp.number && tp.exactNumber){
                isValid = false;
            }
            if(objCount > tp.number && !tp.exactNumber){
                isValid = false;
            }
        }
        
        foreach(WeightRequirement tp in weightRequirements){//Tuple<ItemColor,int,bool>
            int objCount = 0;
            foreach(GameObject c in content){
                Item i = c.GetComponent<Item>() ?? c.transform.parent.GetComponent<Item>();
                if(i.itemNumber==tp.weight){
                    objCount+=1;
                }
            }
            if(objCount != tp.number && tp.exactNumber){
                isValid = false;
            }
            if(objCount > tp.number && !tp.exactNumber){
                isValid = false;
            }
        }
        if(numberRequirement != content.Count && numberRequirement != 0 && exactNumber){
            isValid = false;
        }
        if(numberRequirement < content.Count && numberRequirement != 0 && !exactNumber){
            isValid = false;
        }

        if(clear){
            while(content.Count>0){
                GameObject c0 = content[0];
                content.Remove(c0);
                Destroy(c0);
            }
        }
        return isValid;
    }
    public static readonly Dictionary<int, string> ColorCode
        = new Dictionary<int, string>
    {
        { 1, "Green" },
        { 2, "Red" }
    };
}

public enum ParameterType {REQUIREMENT,GOAL};