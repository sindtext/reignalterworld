using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvent : MonoBehaviour
{
    public event Action<AnimationEvent> AtkEnd;

    void OnAnimationEnd(AnimationEvent animationEvent)
    {
        AtkEnd?.Invoke(animationEvent);
    }
}
