using UnityEngine.Audio;
using UnityEngine;

namespace CarePackage.Main.Sound
{
    public enum EAudioPlayType { OneShot, Looping }
    public enum EAudioCategory { Default, Ambience, Music, UI, Master }
    public enum EAudioPlayCondition { Never, PlayOnAwake, PlayOnEvent, PlayOnTriggered, PlayOnCollided, PlayOnInteracted, FollowUp }
    public enum EAudioPlayLocation { Global, Local }
    
    [System.Serializable]
    public class AudioEntry
    {
        public EAudioCategory category;
        public AudioClip clip;
        public EAudioPlayType playType;
        public int priority;
        public EAudioPlayCondition playCondition;
        public EAudioPlayLocation playLocation;
        public float delaySeconds;
        public string eventName;
    }
    
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private SO_AudioMixer mapping;
        [SerializeField] private AudioMixer mixer;
        
        public AudioMixerGroup GetGroup(EAudioCategory category) => mapping.GetGroup(category);
        public string GetVolumeParamByGroup(EAudioCategory group) => AudioVolumes[group];

        private static bool _initialized;
        
        private const string MasterKey = "MasterVolume";
        private const string DefaultKey = "DefaultVolume";
        private const string AmbienceKey = "AmbienceVolume";
        private const string MusicKey = "MusicVolume";
        private const string UiKey = "UIVolume";
        
        private static readonly System.Collections.Generic.Dictionary<EAudioCategory, string> AudioVolumes = new()
        {
            { EAudioCategory.Master, MasterKey },
            { EAudioCategory.Default, DefaultKey },
            { EAudioCategory.Ambience, AmbienceKey },
            { EAudioCategory.Music, MusicKey },
            { EAudioCategory.UI, UiKey },
        };
        
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            
            mapping.Initialize();
            LoadVolumes();
        }
        
        public void PreviewVolume(EAudioCategory category, float value) 
        {
            var exposedParam = GetVolumeParamByGroup(category);
            mixer.SetFloat(exposedParam, AudioMath.DecimalToDecibel(value));
        }
        
        public void SaveVolume(EAudioCategory category)
        {
            if (mixer.GetFloat(GetVolumeParamByGroup(category), out var value))
            {
                var valueInDecimal = AudioMath.DecibelToDecimal(value);
                PlayerPrefs.SetFloat(GetVolumeParamByGroup(category), valueInDecimal);
                PreviewVolume(category, valueInDecimal);
            }
        }

        public void SaveVolumes()
        {
            ForEachVolume((key, value) => SaveVolume(key));
        }
        
        public void LoadVolumes()
        {
            ForEachVolume((category, key) => 
            { 
                var stored = PlayerPrefs.GetFloat(key, 1f);
                PreviewVolume(category, stored);
            });
        }
        
        private void ForEachVolume(System.Action<EAudioCategory, string> action)
        {
            foreach (var kvp in AudioVolumes)
            {
                action(kvp.Key, kvp.Value);
            }
        }
        
        public float GetVolumeByAudioGroup(EAudioCategory category) => mixer.GetFloat(GetVolumeParamByGroup(category), out var value) ? AudioMath.DecibelToDecimal(value) : 1f;
        
        public static AudioSource PlayOneShotAtLocation(AudioClip clip, EAudioCategory category, Vector3 position, float volume = 1f)
        {
            if (clip == null || Instance == null) return null;
            var group = Instance.GetGroup(category);

            var go = new GameObject("OneShot_" + clip.name);
            go.transform.position = position;
            var src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = group;
            src.spatialBlend = 1f;
            src.dopplerLevel = 0f;
            src.PlayOneShot(clip, volume);
            Destroy(go, clip.length);
            return src;
        }

        public static AudioSource PlayLoop(AudioClip clip, EAudioCategory category, Transform parent = null, float volume = 1f, bool spatial = true)
        {
            if (clip == null || Instance == null) return null;
            var group = Instance.GetGroup(category);

            var go = new GameObject("Loop_" + clip.name);
            if (parent != null) go.transform.SetParent(parent);
            else go.transform.SetParent(Instance.transform);
            go.transform.localPosition = Vector3.zero;
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.loop = true;
            src.volume = volume;
            src.dopplerLevel = 0f;
            src.outputAudioMixerGroup = group;
            src.spatialBlend = spatial ? 1f : 0f;
            src.Play();
            return src;
        }
    }
    
    public static class AudioMath
    {
        public static float DecimalToDecibel(float inDecimal)
        {
            return Mathf.Log10(Mathf.Clamp(inDecimal, 0.0001f, 1f)) * 20f;
        }
        
        public static float DecibelToDecimal(float inDB)
        {
            return Mathf.Pow(10f, inDB / 20f);
        }
    }
}