using System;
using System.Collections.Generic;
using UnityEngine;

namespace VerySimpleAnimation.Sample
{
    public class AnimationListPlayer : VerySimpleAnimation.AnimationPlayer
    {
        [Serializable]
        public class AnimationData
        {
            public string clipName;
            public AnimationClip clip;
            public Action onFinish;
            public float speed;
        }

        [SerializeField]
        private List<AnimationData> animationDataList = default;
        private Dictionary<string, AnimationData> animationDataDictionary;
        private AnimationData currentAnimationData;

        private Queue<AnimationData> animationDataQueue = new Queue<AnimationData>();

        public override void Init(Animator animator)
        {
            base.Init(animator);
            animationDataDictionary = new Dictionary<string, AnimationData>();
            foreach (var data in animationDataList)
            {
                if (!data.clip.ExistsEvent(AnimationClipExtension.ON_START_ANIMATION))
                {
                    var evt = new AnimationEvent();
                    evt.functionName = AnimationClipExtension.ON_START_ANIMATION;
                    evt.time = 0f;
                    data.clip.AddEvent(evt);
                }
                if (!data.clip.ExistsEvent(AnimationClipExtension.ON_END_ANIMATION))
                {
                    var evt = new AnimationEvent();
                    evt.functionName = AnimationClipExtension.ON_END_ANIMATION;
                    evt.time = data.clip.length - AnimationClipExtension.ANIMATION_CROSSFADE;
                    data.clip.AddEvent(evt);
                }
                animationDataDictionary.Add(data.clipName, data);
            }
            Play("Idle");
        }
        private void OnStartAnimation()
        {
        }
        private void OnEndAnimation()
        {
            if (currentAnimationData != null)
            {
                if (currentAnimationData.onFinish != null)
                {
                    currentAnimationData.onFinish();
                }
            }
            if (animationDataQueue.Count > 0)
            {
                var data = animationDataQueue.Dequeue();
                if (data.clipName == CurrentAnimatinoClipName)
                {
                    return;
                }
                Play(data);
            }
        }
        public void Play(string clipName, float speed = 1f, Action onFinish = null)
        {
            AnimationData data = null;
            if (animationDataDictionary.TryGetValue(clipName, out data))
            {
                data.onFinish = onFinish;
                data.speed = speed;
                Play(data);
            }
        }
        public void Play(int index, float speed = 1f)
        {
            if (animationDataList == null || animationDataList.Count <= 0)
            {
                return;
            }
            index = index % animationDataList.Count;
            var data = animationDataList[index];
            data.speed = speed;
            Play(data);
        }
        public void PlayDelay(string clipName, float delayTime, float speed = 1f)
        {
            AnimationData data = null;
            if (animationDataDictionary.TryGetValue(clipName, out data))
            {
                if (IsPlaying())
                {
                    data.speed = speed;
                    animationDataQueue.Enqueue(data);
                }
                else
                {
                    data.speed = speed;
                    Play(data);
                }
            }
        }
        public void PlayDelay(int index, float delayTime, float speed = 1f)
        {
            if (animationDataList == null || animationDataList.Count <= 0)
            {
                return;
            }
            index = index % animationDataList.Count;
            var data = animationDataList[index];
            data.speed = speed;
            PlayDelay(data.clipName, delayTime);
        }
        public void PlayQueue(string clipName, float speed = 1f)
        {
            AnimationData data = null;
            if (animationDataDictionary.TryGetValue(clipName, out data))
            {
                if (IsPlaying())
                {
                    data.speed = speed;
                    animationDataQueue.Enqueue(data);
                }
                else
                {
                    data.speed = speed;
                    Play(data);
                }
            }
        }
        public void PlayQueue(int index, float speed = 1f)
        {
            if (animationDataList == null || animationDataList.Count <= 0)
            {
                return;
            }
            index = index % animationDataList.Count;
            var data = animationDataList[index];
            data.speed = speed;
            PlayQueue(data.clipName);
        }
        protected void Play(AnimationData data)
        {
            RegisterStartCallback(OnStartAnimation);
            RegisterEndCallback(OnEndAnimation);
            currentAnimationData = data;
            Play(data.clip, AnimationClipExtension.ANIMATION_CROSSFADE, data.speed);
        }
    }
}