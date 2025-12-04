#if !UNITY_WEBGL
using System;
using System.Threading.Tasks;
using TinCan;
using UnityEngine;
using Xasu;
using Xasu.HighLevel;

public class GameLogger : MonoBehaviour
{
    private DateTime startTime;
    
    private async void Start()
    {
        await Task.Yield();
        XasuTracker.Instance.DefaultActor = new Agent
        {
            name = Guid.NewGuid().ToString(),
        };
        CompletableTracker.Instance.Initialized("CarePackage", CompletableTracker.CompletableType.Game);
        startTime = DateTime.Now;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (current, next) => AccessibleTracker.Instance.Accessed(next.name, AccessibleTracker.AccessibleType.Area);
    }

    private void OnDestroy()
    {
        CompletableTracker.Instance.Completed("CarePackage", CompletableTracker.CompletableType.Game).WithDuration(startTime, DateTime.Now);
    }
}
#endif