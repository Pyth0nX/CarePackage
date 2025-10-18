using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;
using UnityEngine.Events;
using System.ComponentModel;
namespace OpenDialogue
{
    [System.Serializable]
    [NodeWidth(400)]
    public class Frame : Node
    {

        // Use this for initialization
        protected override void Init()
        {
            base.Init();

        }
        [Input] public Frame a;
        [Input] public Frame b;
        [Output] public Frame one;
        [Output] public Frame two;
        [Output] public Frame three;

        public string dialogue;
        public string[] randomPossibleOptions;
        public bool random;
        public Settings settings;
        public string[] options;

        public bool toOtherGraph;
        public DialogueGraph graphTo;

        public bool isEvent;
        public bool skip;
        public bool root;
        public InventoryAction inventoryAction;
        public ChoiceAction choiceAction;
        public ValueAction valueAction;
        public string[] codeToRun;
        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "one")
            {
                return one;
            }
            else if (port.fieldName == "two")
            {
                return two;
            }
            else if (port.fieldName == "three")
            {
                return three;
            }
            return a;
        }
        public virtual Frame GetNext(int option)
        {
            foreach (NodePort n in Ports)
            {
                if (option == 0 && n.fieldName == "one")
                {
                    return (Frame)n.Connection.node;
                }
                else if (option == 1 && n.fieldName == "two")
                {
                    return (Frame)n.Connection.node;
                }
                else if (option == 2 && n.fieldName == "three")
                {
                    return (Frame)n.Connection.node;
                }
            }
            return null;
        }
        public bool checkConditions(Condition[] conditions, Item[] inventory, int[] choices, ValueTracker valueTracker)
        {
            List<bool> results = new List<bool>();
            foreach (Condition condition in conditions)
            {
                results.Add(checkCondition(condition, inventory, choices, valueTracker));
            }
            foreach (bool result in results)
            {
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }
        public bool checkCondition(Condition condition, Item[] inventory, int[] choices, ValueTracker valueTracker)
        {
            if (condition.conditionType == ConditionType.None)
            {
                return true;
            }
            else if (condition.conditionType == ConditionType.InventoryCheck)
            {
                bool contains = false;
                foreach (Item item in inventory)
                {
                    if (item.id == condition.itemCheck.id)
                    {
                        contains = true;
                    }
                }
                return contains;
            }
            else if (condition.conditionType == ConditionType.Choice)
            {
                foreach (int i in condition.choiceConditionData.choiceIs)
                {
                    if (choices[condition.choiceConditionData.choiceReferenced] == i)
                    {
                        return true;
                    }
                }
            }
            else if (condition.conditionType == ConditionType.Value)
            {
                if (condition.valueComparison.comparisonType == comparisonTypes.greaterThan)
                {
                    if (valueTracker.GetValue(condition.valueComparison.key) > condition.valueComparison.compareTo)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (condition.valueComparison.comparisonType == comparisonTypes.lessThan)
                {
                    if (valueTracker.GetValue(condition.valueComparison.key) < condition.valueComparison.compareTo)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (condition.valueComparison.comparisonType == comparisonTypes.greaterThanOrEqual)
                {
                    if (valueTracker.GetValue(condition.valueComparison.key) >= condition.valueComparison.compareTo)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (condition.valueComparison.comparisonType == comparisonTypes.lessThanOrEqual)
                {
                    if (valueTracker.GetValue(condition.valueComparison.key) <= condition.valueComparison.compareTo)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    [System.Serializable]
    public class InventoryAction
    {
        public ActionTypes actionType;
        public Item item;
    }
    public enum ActionTypes
    {
        None,
        Add,
        Remove

    }
    [System.Serializable]
    public class Settings
    {
        public NameOptions nameOptions;
        public AudioOptions audioOptions;
        public GraphicOptions graphicOptions;
        public ConditionList[] conditions;
        public ConditionList ifSkipOneConditions;
    }
    [System.Serializable]
    public class ConditionList
    {
        public Condition[] conditions;
    }
    [System.Serializable]
    public class NameOptions
    {
        public string p1Name;
        public string p2Name;
        public string p3Name;
        public bool showNameBlock1;
        public bool showNameBlock2;
        public bool showNameBlock3;
    }
    [System.Serializable]
    public class AudioOptions
    {
        public AudioClip backgroundMusic;
        public AudioClip effect;
    }
    [System.Serializable]
    public class GraphicOptions
    {
        public backgrounds background;
        public characterImages portrait1;
        public characterImages portrait2;
        public characterImages portrait3;
        public Sprite eventImage;
    }

    [System.Serializable]
    public class Condition
    {
        public ConditionType conditionType;
        public ChoiceConditionData choiceConditionData;
        public Item itemCheck;
        public ValueComparison valueComparison;
    }
    public enum ConditionType
    {
        None,
        InventoryCheck,
        Choice,
        Value
    }
    [System.Serializable]
    public class ChoiceConditionData
    {
        public int choiceReferenced;
        public int[] choiceIs;
    }
    [System.Serializable]
    public class ChoiceAction
    {
        public ChoiceActionTypes choiceActionType;
        public int choiceIndex;
        public int setTo;
    }
    public enum ChoiceActionTypes
    {
        none,
        set
    }
    public enum characterImages
    {
        image
    }
    public enum backgrounds
    {
        image
    }
    [System.Serializable]
    public class ValueComparison
    {
        public valueKey key;
        public comparisonTypes comparisonType;
        public int compareTo;
    }
    public enum comparisonTypes
    {
        greaterThan,
        lessThan,
        greaterThanOrEqual,
        lessThanOrEqual
    }
    [System.Serializable]
    public class ValueAction
    {
        public valueActionTypes valueActionType;
        public valueKey key;
        public int param;
    }
    public enum valueActionTypes
    {
        none,
        change,
        set
    }

}
