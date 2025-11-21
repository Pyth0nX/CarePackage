using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

namespace CarePackage.Main.Sound
{
    [CreateAssetMenu(fileName = "SO_AudioMixer", menuName = "CarePackage/Audio/AudioMixerSetup")]
    public class SO_AudioMixer : ScriptableObject
    {
        [System.Serializable]
        public struct GroupEntry
        {
            public EAudioCategory category;
            public AudioMixerGroup mixerGroup;
        }

        [SerializeField] private List<GroupEntry> groups = new();

        private Dictionary<EAudioCategory, AudioMixerGroup> _map;
        
        public void Initialize()
        {
            _map = new Dictionary<EAudioCategory, AudioMixerGroup>();
            foreach (var entry in groups)
            {
                if (!_map.ContainsKey(entry.category))
                    _map[entry.category] = entry.mixerGroup;//_map.Add(entry.category, entry.mixerGroup);
            }
        }

        public AudioMixerGroup GetGroup(EAudioCategory category)
        {
            if (_map == null) Initialize();
            return _map.TryGetValue(category, out var group) ? group : null;
        }
    }
}