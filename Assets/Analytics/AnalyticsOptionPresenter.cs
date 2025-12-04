#if !UNITY_WEBGL
using System.Threading;
using Yarn.Unity;

public class AnalyticsOptionPresenter : OptionsPresenter
{
    public override YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        var task = base.RunOptionsAsync(dialogueOptions, cancellationToken);
        var awaiter = base.RunOptionsAsync(dialogueOptions, cancellationToken).GetAwaiter();
        awaiter.OnCompleted(Continuation);

        void Continuation()
        {
            Xasu.HighLevel.AlternativeTracker.Instance.Selected("AltOption_" + DialogueManager.Instance.dialogueRunner.Dialogue.CurrentNode,
                "DialogueOption_" + awaiter.GetResult().DialogueOptionID);
        }
        return task;
    }
}
#endif