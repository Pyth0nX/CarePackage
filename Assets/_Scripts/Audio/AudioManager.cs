using UnityEngine;
using UnityEngine.Audio;

namespace CarePackage.Main.Sound
{
    public enum EAudioPlayType { OneShot, Looping }
    public enum EAudioCategory { Default, Ambience, Music, UI, Master }
    public enum EAudioPlayCondition { Never, PlayOnAwake, PlayOnEvent, PlayOnTriggered, PlayOnCollided, PlayOnInteracted, FollowUp }
    public enum EAudioPlayLocation { Global, Local }
    
    public enum EAudioEnums { AudioGroup, EAudioCategory, EAudioPlayCondition, EAudioPlayLocation }
    
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
        
        public void SetVolume(EAudioCategory exposedParam, float value)
        {
            var exposedVolumeParam = GetVolumeParamByGroup(exposedParam);
            mixer.SetFloat(exposedVolumeParam, Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
            PlayerPrefs.SetFloat(exposedVolumeParam, value);
        }
        
        public void PreviewVolume(EAudioCategory category, float value) 
        {
            var exposedParam = GetVolumeParamByGroup(category);
            mixer.SetFloat(exposedParam, Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        }
        
        public void SaveVolume(EAudioCategory category, float value) 
        {
            PreviewVolume(category, value);
            PlayerPrefs.SetFloat(GetVolumeParamByGroup(category), value);
        }
        
        private void LoadVolumes()
        {
            foreach (var volume in AudioVolumes)
            {
                var stored = PlayerPrefs.GetFloat(volume.Value, 1f);
                PreviewVolume(volume.Key, stored);
            }
        }
        
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
}