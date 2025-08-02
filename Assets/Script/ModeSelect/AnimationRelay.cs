using UnityEngine;
using UnityEngine.Events;

public class AnimationRelay : MonoBehaviour
{
    public UnityEvent onFadeOutFinished;         // For Mode → Difficulty
    public UnityEvent onFadeOutBackFinished;     // For Difficulty → Mode

    // Called by FadeOut animation (Mode panel fade-out)
    public void FadeOutFinished()
    {
        onFadeOutFinished?.Invoke();
    }

    // Called by FadeOutBack animation (Difficulty panel back)
    public void FadeOutBackFinished()
    {
        onFadeOutBackFinished?.Invoke();
    }
}
