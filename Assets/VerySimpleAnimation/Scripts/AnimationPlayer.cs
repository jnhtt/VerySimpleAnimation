using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace VerySimpleAnimation
{
    public class AnimationPlayer : MonoBehaviour
    {
        private const int MIX_NUM = 2;
        protected Animator animator;
        protected float weight;
        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;
        private AnimationClipPlayable currentPlayable;
        private AnimationClipPlayable previousPlayable;
        private AnimationClip currentAnimationClip;
        private Coroutine mixerCoroutine;
        private Action onStartEvent;
        private Action onEndEvent;
        public Animator Animator { get { return animator; } }
        public AnimationClip CurrentAnimationClip { get { return currentAnimationClip; } }
        public string CurrentAnimatinoClipName { get { return currentAnimationClip == null ? "" : currentAnimationClip.name; } }

        public virtual void Init(Animator animator)
        {
            this.animator = animator;
            InitPlayable();
        }
        public void Stop()
        {
            if (graph.IsValid())
            {
                graph.Stop();
            }
        }
        public void RegisterStartCallback(Action action)
        {
            onStartEvent = action;
        }
        private void OnStartAnimation()
        {
            if (onStartEvent != null)
            {
                onStartEvent();
            }
        }
        public void RegisterEndCallback(Action action)
        {
            onEndEvent = action;
        }
        private void OnEndAnimation()
        {
            if (onEndEvent != null)
            {
                onEndEvent();
            }
        }
        public bool IsPlaying()
        {
            if (!mixer.IsValid() || !currentPlayable.IsValid())
            {
                return false;
            }
            var clip = currentPlayable.GetAnimationClip();
            if (!clip.isLooping)
            {
                return clip.length >= currentPlayable.GetTime();
            }
            var state = currentPlayable.GetPlayState();
            return state == PlayState.Playing;
        }
        public void PlayDelay(AnimationClip nextAnimationClip, float fadeTime, float delayTime, float speed)
        {
            if (delayTime <= 0f)
            {
                Play(nextAnimationClip, fadeTime, speed);
            }
            else
            {
                StartCoroutine(DelayCoroutine(nextAnimationClip, fadeTime, delayTime, speed));
            }
        }
        private IEnumerator DelayCoroutine(AnimationClip nextAnimationClip, float fadeTime, float delayTime, float speed)
        {
            yield return new WaitForSeconds(delayTime);
            Play(nextAnimationClip, fadeTime, speed);
        }
        public void Play(AnimationClip nextAnimationClip, float fadeTime, float speed)
        {
            if (nextAnimationClip == null)
            {
                return;
            }
            if (currentAnimationClip == null)
            {
                graph.Disconnect(mixer, 1);
                currentPlayable = AnimationClipPlayable.Create(graph, nextAnimationClip);
                currentPlayable.SetSpeed(speed);
                mixer.ConnectInput(0, currentPlayable, 0);
                mixer.SetInputWeight(0, 1f);
                currentAnimationClip = nextAnimationClip;
            }
            else if (!currentAnimationClip.name.StartsWith(nextAnimationClip.name))
            {
                graph.Disconnect(mixer, 0);
                graph.Disconnect(mixer, 1);
                if (previousPlayable.IsValid())
                {
                    previousPlayable.Destroy();
                }
                currentAnimationClip = nextAnimationClip;
                previousPlayable = currentPlayable;
                currentPlayable = AnimationClipPlayable.Create(graph, nextAnimationClip);
                currentPlayable.SetSpeed(speed);
                mixer.ConnectInput(0, currentPlayable, 0);
                mixer.ConnectInput(1, previousPlayable, 0);
                if (fadeTime <= 0f)
                {
                    mixer.SetInputWeight(0, 1f);
                    mixer.SetInputWeight(1, 0f);
                    return;
                }
                if (mixerCoroutine != null)
                {
                    StopCoroutine(mixerCoroutine);
                    mixerCoroutine = null;
                }
                mixerCoroutine = StartCoroutine(PlayCoroutine(fadeTime));
            }
        }
        private IEnumerator PlayCoroutine(float fadeTime)
        {
            mixer.SetInputWeight(0, 0f);
            mixer.SetInputWeight(1, 1f);
            float interval = fadeTime;
            float rate = 0f;
            while (fadeTime > 0f)
            {
                yield return null;
                fadeTime -= Time.deltaTime;
                rate = Mathf.Clamp01((interval - fadeTime) / interval);
                mixer.SetInputWeight(0, rate);
                mixer.SetInputWeight(1, 1f - rate);
            }
            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);
        }
        protected void InitPlayable()
        {
            graph = PlayableGraph.Create();
            mixer = AnimationMixerPlayable.Create(graph, 2, true);
            mixer.ConnectInput(0, currentPlayable, 0);
            mixer.SetInputWeight(0, 1);
            var output = AnimationPlayableOutput.Create(graph, name + " output", animator);
            output.SetSourcePlayable(mixer, 0);
            graph.Play();
        }
        protected virtual void OnDestroy()
        {
            Cleanup();
        }
        protected void Cleanup()
        {
            if (graph.IsValid())
            {
                graph.Stop();
            }
            if (mixerCoroutine != null)
            {
                StopCoroutine(mixerCoroutine);
                mixerCoroutine = null;
            }
            if (currentPlayable.IsValid())
            {
                currentPlayable.Destroy();
            }
            if (previousPlayable.IsValid())
            {
                previousPlayable.Destroy();
            }
            if (mixer.IsValid())
            {
                mixer.Destroy();
            }
            if (graph.IsValid())
            {
                graph.Destroy();
            }
        }
    }
}