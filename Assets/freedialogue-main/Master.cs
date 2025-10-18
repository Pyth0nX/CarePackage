using NUnit.Framework;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using XNode;
namespace OpenDialogue
{
    public class Master : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        public DialogueGraph dialogueGraph;
        public List<Frame> frames;
        public Frame currentFrame;
        public Image background;
        public TMP_Text dialogue;
        public TMP_Text eventText;
        public string randomTextCache;
        public Image portrait1;
        public Image portrait2;
        public Image portrait3;
        public TMP_Text name1;
        public TMP_Text name2;
        public TMP_Text name3;
        public GameObject nameBlock1;
        public GameObject nameBlock2;
        public GameObject nameBlock3;
        public TMP_Text option1;
        public TMP_Text option2;
        public TMP_Text option3;
        public GameObject option1Block;
        public GameObject option2Block;
        public GameObject option3Block;
        public Image eventImage;
        public GameObject conversationPanel;
        public GameObject eventPanel;
        public ChoiceManager choiceManager;
        public EndingManager endingManager;
        public AudioSource backgroundSource;
        public AudioSource foregroundSource;
        public Inventory inventory;
        public ValueTracker valueTracker;
        public GameObject loadingScreen;
        public DialogueGraph[] graphsToLoad;
        public bool beganLoading;
        public bool loadedIntoRam;
        public bool initialized;

        // Update is called once per frame
        void Update()
        {
            if (!loadedIntoRam && !beganLoading)
            {
                StartCoroutine(loadAllFrames());
                beganLoading = true;
            }
            else if (!initialized && loadedIntoRam)
            {
                List<Node> newNodeList = new List<Node>();
                foreach (Frame f in dialogueGraph.nodes)
                {
                    newNodeList.Add(f);
                }
                Frame newRoot = (Frame) ScriptableObject.CreateInstance(typeof(Frame));
                Frame oldRoot = (Frame)ScriptableObject.CreateInstance(typeof(Frame));
                int newRootOldIndex = 0;
                foreach (Frame f in newNodeList)
                {
                    if (f.root)
                    {
                        newRoot = f;
                        oldRoot = (Frame)newNodeList[0];
                        newRootOldIndex = newNodeList.IndexOf(newRoot);
                    }

                }
                newNodeList[0] = newRoot;
                newNodeList[newRootOldIndex] = oldRoot;
                foreach (Frame f in newNodeList)
                {
                    frames.Add(f);
                }

                currentFrame = frames[0];
                initialized = true;
            }
            else if (initialized && loadedIntoRam)
            {
                {
                    ImageEnumManager iem = GetComponent<ImageEnumManager>();
                    background.sprite = iem.GetBackground(currentFrame.settings.graphicOptions.background);
                    if (currentFrame.random)
                    {
                        dialogue.text = randomTextCache;
                    }
                    else
                    {
                        dialogue.text = currentFrame.dialogue;
                    }

                    portrait1.sprite = iem.GetSprite(currentFrame.settings.graphicOptions.portrait1);
                    portrait2.sprite = iem.GetSprite(currentFrame.settings.graphicOptions.portrait2);
                    portrait3.sprite = iem.GetSprite(currentFrame.settings.graphicOptions.portrait3);
                    name1.text = currentFrame.settings.nameOptions.p1Name;
                    name2.text = currentFrame.settings.nameOptions.p2Name;
                    name3.text = currentFrame.settings.nameOptions.p3Name;
                    nameBlock1.SetActive(currentFrame.settings.nameOptions.showNameBlock1);
                    nameBlock2.SetActive(currentFrame.settings.nameOptions.showNameBlock2);
                    nameBlock3.SetActive(currentFrame.settings.nameOptions.showNameBlock3);
                    portrait1.gameObject.SetActive(currentFrame.settings.nameOptions.showNameBlock1);
                    portrait2.gameObject.SetActive(currentFrame.settings.nameOptions.showNameBlock2);
                    portrait3.gameObject.SetActive(currentFrame.settings.nameOptions.showNameBlock3);
                    for (int i = 0; i < currentFrame.options.Length; i++)
                    {
                        if (i == 0)
                        {
                            option1.text = currentFrame.options[i];
                            if (currentFrame.settings.conditions.Length >= 1)
                            {
                                option1Block.SetActive(currentFrame.checkConditions(currentFrame.settings.conditions[0].conditions, inventory.items, choiceManager.choices, valueTracker));
                            }
                            else
                            {
                                option1Block.SetActive(true);
                            }

                        }
                        else if (i == 1)
                        {
                            option2.text = currentFrame.options[i];
                            if (currentFrame.settings.conditions.Length >= 2)
                            {
                                option2Block.SetActive(currentFrame.checkConditions(currentFrame.settings.conditions[1].conditions, inventory.items, choiceManager.choices, valueTracker));
                            }
                            else
                            {
                                option2Block.SetActive(true);
                            }

                        }
                        else if (i == 2)
                        {
                            option3.text = currentFrame.options[i];
                            if (currentFrame.settings.conditions.Length >= 3)
                            {
                                option3Block.SetActive(currentFrame.checkConditions(currentFrame.settings.conditions[2].conditions, inventory.items, choiceManager.choices, valueTracker));
                            }
                            else
                            {
                                option3Block.SetActive(true);
                            }

                        }

                    }
                    if (currentFrame.options.Length == 0)
                    {
                        option1Block.SetActive(false);
                        option2Block.SetActive(false);
                        option3Block.SetActive(false);
                    }
                    else if (currentFrame.options.Length == 1)
                    {
                        option2Block.SetActive(false);
                        option3Block.SetActive(false);
                    }
                    else if (currentFrame.options.Length == 2)
                    {
                        option3Block.SetActive(false);
                    }
                    eventImage.sprite = currentFrame.settings.graphicOptions.eventImage;
                    eventPanel.SetActive(currentFrame.isEvent);
                    conversationPanel.SetActive(!currentFrame.isEvent);
                    if (currentFrame.isEvent)
                    {
                        eventText.text = dialogue.text;
                        if (currentFrame.valueAction.valueActionType == valueActionTypes.set)
                        {
                            valueTracker.SetValue(currentFrame.valueAction.key, currentFrame.valueAction.param);
                        }
                        else if (currentFrame.valueAction.valueActionType == valueActionTypes.change)
                        {
                            valueTracker.ChangeValue(currentFrame.valueAction.key, currentFrame.valueAction.param);
                        }
                        if (currentFrame.choiceAction.choiceActionType == ChoiceActionTypes.set)
                        {
                            try
                            {
                                choiceManager.choices[currentFrame.choiceAction.choiceIndex] = currentFrame.choiceAction.setTo;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }

                        if (Input.GetMouseButtonDown(0))
                        {
                            currentFrame = currentFrame.GetNext(0);
                            if (currentFrame.random)
                            {
                                randomTextCache = currentFrame.randomPossibleOptions[UnityEngine.Random.Range(0, currentFrame.randomPossibleOptions.Length - 1)];
                            }
                            if (currentFrame.toOtherGraph)
                            {
                                dialogueGraph = currentFrame.graphTo;
                                Frame setCurrentFrameTo = null;
                                foreach (Frame f in currentFrame.graphTo.nodes)
                                {
                                    if (f.root)
                                    {
                                        setCurrentFrameTo = f;
                                        Debug.Log("foundroot");
                                        break;
                                    }
                                }
                                currentFrame = setCurrentFrameTo;
                            }
                            if (currentFrame.inventoryAction.actionType != ActionTypes.None)
                            {
                                if (currentFrame.inventoryAction.actionType == ActionTypes.Add)
                                {
                                    inventory.Add(currentFrame.inventoryAction.item);
                                }
                                else if (currentFrame.inventoryAction.actionType == ActionTypes.Remove)
                                {
                                    inventory.Remove(currentFrame.inventoryAction.item);
                                }
                            }
                            if (currentFrame.skip)
                            {
                                if (currentFrame.checkConditions(currentFrame.settings.ifSkipOneConditions.conditions, inventory.items, choiceManager.choices, valueTracker))
                                {
                                    DialogueOption(0);
                                }
                                else
                                {
                                    DialogueOption(1);
                                }
                            }
                            else
                            {

                                foregroundSource.clip = currentFrame.settings.audioOptions.effect;
                                foregroundSource.Play();
                                if (backgroundSource.clip != currentFrame.settings.audioOptions.backgroundMusic)
                                {
                                    backgroundSource.clip = currentFrame.settings.audioOptions.backgroundMusic;
                                    backgroundSource.Play();
                                }

                            }

                        }

                    }
                }

            }
        }
        public void DialogueOption(int option)
        {
            currentFrame = currentFrame.GetNext(option);
            if (currentFrame.random)
            {
                randomTextCache = currentFrame.randomPossibleOptions[UnityEngine.Random.Range(0, currentFrame.randomPossibleOptions.Length - 1)];
            }
            if (currentFrame.toOtherGraph)
            {
                dialogueGraph = currentFrame.graphTo;
                Frame setCurrentFrameTo = null;
                foreach (Frame f in currentFrame.graphTo.nodes)
                {
                    if (f.root)
                    {
                        setCurrentFrameTo = f;
                        Debug.Log("foundroot");
                        break;
                    }
                }
                currentFrame = setCurrentFrameTo;
            }
            if (currentFrame.valueAction.valueActionType == valueActionTypes.set)
            {
                valueTracker.SetValue(currentFrame.valueAction.key, currentFrame.valueAction.param);
            }
            else if (currentFrame.valueAction.valueActionType == valueActionTypes.change)
            {
                valueTracker.ChangeValue(currentFrame.valueAction.key, currentFrame.valueAction.param);
            }
            if (currentFrame.inventoryAction.actionType != ActionTypes.None)
            {
                if (currentFrame.inventoryAction.actionType == ActionTypes.Add)
                {
                    inventory.Add(currentFrame.inventoryAction.item);
                }
                else if (currentFrame.inventoryAction.actionType == ActionTypes.Remove)
                {
                    inventory.Remove(currentFrame.inventoryAction.item);
                }
            }
            if (currentFrame.choiceAction.choiceActionType == ChoiceActionTypes.set)
            {
                try
                {
                    choiceManager.choices[currentFrame.choiceAction.choiceIndex] = currentFrame.choiceAction.setTo;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            endingManager.checkEnding();
            if (currentFrame.skip)
            {
                if (currentFrame.checkConditions(currentFrame.settings.ifSkipOneConditions.conditions, inventory.items, choiceManager.choices, valueTracker))
                {
                    DialogueOption(0);
                }
                else
                {
                    DialogueOption(1);
                }
                Debug.Log("skipped frame");
            }
            else
            {
                foregroundSource.clip = currentFrame.settings.audioOptions.effect;
                foregroundSource.Play();
                if (backgroundSource.clip != currentFrame.settings.audioOptions.backgroundMusic)
                {
                    backgroundSource.clip = currentFrame.settings.audioOptions.backgroundMusic;
                    backgroundSource.Play();
                }
            }
        }
        IEnumerator loadAllFrames()
        {
            loadingScreen.SetActive(true);
            foreach (DialogueGraph graph in graphsToLoad)
            {
                foreach (Frame frame in graph.nodes)
                {
                    currentFrame = frame;
                }
            }
            loadingScreen.SetActive(false);
            loadedIntoRam = true;
            yield return new WaitForSeconds(0);
        }
        public void OptionOne()
        {
            DialogueOption(0);
        }
        public void OptionTwo()
        {
            DialogueOption(1);
        }
        public void OptionThree()
        {
            DialogueOption(2);
        }
    }

}
