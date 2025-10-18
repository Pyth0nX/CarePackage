using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
namespace OpenDialogue
{
    public class ImageEnumManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        [SerializedDictionary]
        public SerializedDictionary<characterImages, Sprite> images = new SerializedDictionary<characterImages, Sprite>();
        [SerializedDictionary]
        public SerializedDictionary<backgrounds, Sprite> backgroundImages = new SerializedDictionary<backgrounds, Sprite>();
        public Sprite questionMark;
        public Sprite GetSprite(characterImages characterImage)
        {
            Sprite toReturn;
            if (images.TryGetValue(characterImage, out toReturn))
            {
                return toReturn;
            }
            else
            {
                return questionMark;
            }
        }
        public Sprite GetBackground(backgrounds backgroundImage)
        {
            Sprite toReturn;
            if(backgroundImages.TryGetValue(backgroundImage, out toReturn))
            {
                return toReturn;
            }
            else
            {
                return questionMark;
            }
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
    [System.Serializable]
    public class ImagePair
    {
        public characterImages name;
        public Sprite image;
    }
}
