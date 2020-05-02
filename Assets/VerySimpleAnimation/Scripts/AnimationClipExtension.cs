using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VerySimpleAnimation
{
    public static class AnimationClipExtension
    {
        public const string ON_START_ANIMATION = "OnStartAnimation";
        public const string ON_END_ANIMATION = "OnEndAnimation";
        public const float ANIMATION_CROSSFADE = 0.1f;

        public static bool ExistsEvent(this AnimationClip clip, string funcName)
        {
            var events = clip.events;
            for (int i = 0; i < events.Length; ++i)
            {
                if (events[i].functionName.StartsWith(funcName))
                {
                    return true;
                }
            }
            return false;
        }
        public static AnimationEvent GetEvent(this AnimationClip clip, string funcName)
        {
            var events = clip.events;
            for (int i = 0; i < events.Length; ++i)
            {
                if (events[i].functionName.StartsWith(funcName))
                {
                    return events[i];
                }
            }
            return null;
        }
    }
}
