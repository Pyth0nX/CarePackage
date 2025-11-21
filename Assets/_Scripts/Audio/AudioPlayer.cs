using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System.Linq;

namespace CarePackage.Main.Sound
{
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private List<AudioEntry> entries = new();

        private List<AudioSource> activeSources = new();

        private void Awake()
        {
            foreach (var entry in entries.OrderByDescending(e => e.priority))
            {
                HandleEntry(entry);
            }
        }

        private void HandleEntry(AudioEntry entry)
        {
            switch (entry.playCondition)
            {
                case EAudioPlayCondition.PlayOnAwake:
                    if (entry.delaySeconds > 0) PrimeTween.Tween.Delay(entry.delaySeconds, () => Play(entry));
                    else Play(entry);
                    break;
                case EAudioPlayCondition.PlayOnEvent:
                    AudioEvents.Subscribe(entry.eventName, () => Play(entry));
                    break;
                case EAudioPlayCondition.PlayOnTriggered:/*
                    var trigger = gameObject.AddComponent<UnityEngine.AudioTrigger>();
                    trigger.Setup(entry, Play);*/
                    break;
                case EAudioPlayCondition.PlayOnCollided:/*
                    var collider = gameObject.AddComponent<AudioCollision>();
                    collider.Setup(entry, Play);*/
                    break;
                case EAudioPlayCondition.PlayOnInteracted:
                    var interactable = GetComponent<Interaction.Interactable>();
                    if (interactable != null)
                        interactable.OnInteracted += () => Play(entry);
                    break;
                case EAudioPlayCondition.FollowUp:
                    int index = entries.IndexOf(entry);
                    AudioEntry oneShotAbove = null;
                    for (int i = index - 1; i >= 0; i--) 
                    {
                        if (entries[i].playType == EAudioPlayType.OneShot) 
                        {
                            oneShotAbove = entries[i];
                            break;
                        }
                    }
                    if (oneShotAbove != null) 
                    {
                        Play(oneShotAbove, () => Play(entry));
                    }
                    break;
                case EAudioPlayCondition.Never:
                    break;
            }
        }

        public void Play(AudioEntry entry, System.Action onFinished = null)
        {
            AudioSource src = null;
            if (entry.playType == EAudioPlayType.OneShot) src = AudioManager.PlayOneShotAtLocation(entry.clip, entry.category, transform.position);
            else src = AudioManager.PlayLoop(entry.clip, entry.category, transform, 1f, false);
            
            if (onFinished != null && src != null && entry.playType == EAudioPlayType.OneShot) 
            {
                PrimeTween.Tween.Delay(entry.clip.length, onFinished);
            }
        }
    }
    
    public static class AudioEvents
    {
        private static Dictionary<string, System.Action> _events = new();

        public static void Subscribe(string eventName, System.Action callback)
        {
            if (!_events.ContainsKey(eventName))
                _events[eventName] = () => { };
            _events[eventName] += callback;
        }

        public static void Unsubscribe(string eventName, System.Action callback)
        {
            if (_events.ContainsKey(eventName))
                _events[eventName] -= callback;
        }

        public static void Trigger(string eventName)
        {
            if (_events.ContainsKey(eventName))
                _events[eventName]?.Invoke();
        }
    }
}