using System.IO;
using UnityEngine;
namespace OpenDialogue
{
    //All code is written by me. No third part code is included.
    public class ChoiceManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        public int[] choices;
        public int slot;
        public float timeSinceSave;
        public float saveInterval;
        public void LoadSaveIntoMemory(int s)
        {
            if (File.Exists(Application.dataPath + "/save.txt"))
            {
                string text = File.ReadAllText(Application.dataPath + "/save.txt");
                choices = JsonUtility.FromJson<saves>(text).saveSlots[s].choices;
            }
            else
            {
                File.Create(Application.dataPath + "/save.txt");
                string text = File.ReadAllText(Application.dataPath + "/save.txt");
                choices = JsonUtility.FromJson<saves>(text).saveSlots[s].choices;
            }

        }
        public void WriteSaveToFile(int s)
        {
            if (File.Exists(Application.dataPath + "/save.txt"))
            {
                try
                {
                    string text = File.ReadAllText(Application.dataPath + "/save.txt");
                    saves toWrite = JsonUtility.FromJson<saves>(text);
                    toWrite.saveSlots[s].choices = choices;
                    File.WriteAllText(Application.dataPath + "/save.txt", JsonUtility.ToJson(toWrite));
                }
                catch (System.Exception)
                {
                    Debug.Log("Error reading, overwrote save file");
                    save[] newSaves = { new save(choices) };
                    Debug.Log("Writing new file");
                    File.WriteAllText(Application.dataPath + "/save.txt", JsonUtility.ToJson(new saves(newSaves)));
                    Debug.Log("Done writing file");

                }

            }
            else
            {
                Debug.Log("create new save file");
                File.Create(Application.dataPath + "/save.txt");
                Debug.Log("Created file");
                save[] newSaves = { new save(choices) };
                Debug.Log("Writing new file");
                File.WriteAllText(Application.dataPath + "/save.txt", JsonUtility.ToJson(new saves(newSaves)));
                Debug.Log("Done writing file");
            }

        }
        // Update is called once per frame
        void Update()
        {
            timeSinceSave += Time.deltaTime;
            if (timeSinceSave > saveInterval)
            {
                timeSinceSave = 0;
                WriteSaveToFile(slot);
            }
        }
    }
    public class saves
    {
        public save[] saveSlots;
        public saves(save[] Slots)
        {
            this.saveSlots = Slots;
        }
    }
    public class save
    {
        public int[] choices;
        public save(int[] choicesToSet)
        {
            this.choices = choicesToSet;
        }
    }
}

