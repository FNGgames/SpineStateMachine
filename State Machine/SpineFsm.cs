using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

namespace SpineStateMachine
{
    public class SpineFsm
    {
        // MEMBERS

        #region MEMBERS

        protected readonly SkeletonAnimation animation;
        protected readonly List<SpineFsmState> globalStates;
        protected readonly Dictionary<string, List<SpineFsmState>> conditionalStates;
        protected readonly Dictionary<string, List<SpineFsmState>> states;
        protected readonly Dictionary<string, float> alphas;
        protected readonly Dictionary<string, float> timeScales;
        protected readonly Dictionary<string, List<Action>> events;
        protected readonly Properties properties;
        protected readonly HashSet<string> validClips;

        public SkeletonAnimation Animation => animation;
        public string Name => animation.gameObject.name;

        #endregion

        // CONSTRUCTOR

        #region CONSTRUCTOR

        public SpineFsm(SkeletonAnimation animation, Logging logging = 0)
        {
            if (animation == null) throw new ArgumentNullException(nameof(animation));
            
            this.animation = animation;
            this.logging = logging;

            globalStates = new List<SpineFsmState>();
            conditionalStates = new Dictionary<string, List<SpineFsmState>>();
            states = new Dictionary<string, List<SpineFsmState>>();
            alphas = new Dictionary<string, float>();
            timeScales = new Dictionary<string, float>();
            events = new Dictionary<string, List<Action>>();
            properties = new Properties();
            validClips = new HashSet<string>();
            
            GetValidClips();

            animation.AnimationState.Start += OnTrackStart;
            animation.AnimationState.Interrupt += OnTrackInterrupt;
            animation.AnimationState.End += OnTrackEnd;
            animation.AnimationState.Event += OnEvent;
        }

        #endregion

        // STATE CONTROL

        #region STATE_CONTROL
        
        // Global States

        public virtual void AddGlobalState(SpineFsmState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            globalStates.Add(state);
            state.Retain(this, "global");
            state.Enter();
            Log($"Global State Added ({state.GetType().Name})", Logging.StateSetup);
        }

        public void RemoveGlobalState(SpineFsmState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!globalStates.Remove(state)) throw new StateNotFoundException(this, state);
            state.Release();
            Log($"Global State Removed ({state.GetType().Name})", Logging.StateSetup);
        }
        
        // Conditional States

        public virtual void AddConditionalState(string condition, SpineFsmState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrEmpty(condition)) throw new StringIsNullOrEmptyException($"{nameof(condition)}");
            if (conditionalStates.ContainsKey(condition)) conditionalStates[condition].Add(state);
            else conditionalStates[condition] = new List<SpineFsmState> {state};
            state.Retain(this, condition);
            if (properties.GetBool(condition)) state.Enter();
            Log($"Conditional State Added ({state.GetType().Name})", Logging.StateSetup);
        }

        public void RemoveConditionalState(SpineFsmState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrEmpty(state.key)) throw new StringIsNullOrEmptyException($"Invalid condition in state {state.GetType().Name}");
            if (!conditionalStates.ContainsKey(state.key) || !conditionalStates[state.key].Remove(state)) throw new StateNotFoundException(this, state);
            state.Release();
            Log($"Conditional State Removed ({state.GetType().Name})", Logging.StateSetup);
        }
        
        // Clip States

        public virtual void AddState(string clipName, SpineFsmState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrEmpty(clipName)) throw new StringIsNullOrEmptyException($"{nameof(clipName)}");
            if (states.ContainsKey(clipName)) states[clipName].Add(state);
            else states[clipName] = new List<SpineFsmState> {state};
            state.Retain(this, clipName);
            if (IsClipPlaying(clipName, out var trackEntry)) state.Enter(trackEntry);
            Log($"State Added For Clip {clipName.ToUpper()} ({state.GetType().Name})", Logging.StateSetup);
        }

        public void RemoveState(SpineFsmState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrEmpty(state.key)) throw new StringIsNullOrEmptyException($"Invalid clip name in state {state.GetType().Name}");
            if (!states.ContainsKey(state.key) || !states[state.key].Remove(state)) throw new StateNotFoundException(this, state);
            if (states[state.key].Count == 0) states.Remove(state.key);
            state.Release();
            Log($"State Removed For Clip {state.key.ToUpper()} ({state.GetType().Name})", Logging.StateSetup);
        }

        #endregion

        // ANIMATION CONTROL

        #region ANIMATION_CONTROL

        public TrackEntry SetAnimation(int track, string clipName, bool loop, float mixDuration = -1, float timeScale = -1, float alpha = -1)
        {
            if (string.IsNullOrEmpty(clipName)) throw new StringIsNullOrEmptyException($"{nameof(clipName)}");
            if (!IsValidClip(clipName)) throw new InvalidClipException(clipName);

            if (track < 0) track = 0;

            var trackEntry = animation.AnimationState.SetAnimation(track, clipName, loop);
            if (mixDuration >= 0) trackEntry.MixDuration = mixDuration;

            if (timeScale >= 0) timeScales[clipName] = timeScale;
            if (timeScales.ContainsKey(clipName)) trackEntry.TimeScale = timeScales[clipName];

            if (alpha >= 0) alphas[clipName] = alpha;
            if (alphas.ContainsKey(clipName)) trackEntry.Alpha = alphas[clipName];

            Log($"Set Animation (Track: {track}, Clip: {clipName}, Looping: {loop})", Logging.AnimationSetup);

            return trackEntry;
        }

        public TrackEntry SetAnimationIfDifferent(int track, string clipName, bool loop, float mixDuration = -1, float timeScale = -1, float alpha = -1)
        {
            if (string.IsNullOrEmpty(clipName)) throw new StringIsNullOrEmptyException($"{nameof(clipName)}");
            var current = GetCurrent(track);
            if (current == null || current.Animation.Name != clipName)
                return SetAnimation(track, clipName, loop, mixDuration, timeScale, alpha);
            return null;
        }

        public TrackEntry QueueAnimation(int track, string clipName, bool loop, float delay = 0, float mixDuration = -1, float timeScale = -1, float alpha = -1)
        {
            if (string.IsNullOrEmpty(clipName)) throw new StringIsNullOrEmptyException($"{nameof(clipName)}");
            if (!IsValidClip(clipName)) throw new InvalidClipException(clipName);
            
            if (track < 0) track = 0;
            if (delay < 0) delay = 0;

            var trackEntry = animation.AnimationState.AddAnimation(track, clipName, loop, delay);
            if (mixDuration >= 0) trackEntry.MixDuration = mixDuration;

            if (timeScale >= 0) timeScales[clipName] = timeScale;
            if (timeScales.ContainsKey(clipName)) trackEntry.TimeScale = timeScales[clipName];

            if (alpha >= 0) alphas[clipName] = alpha;
            if (alphas.ContainsKey(clipName)) trackEntry.Alpha = alphas[clipName];

            Log($"Queue Animation (Track: {track}, Clip: {clipName}, Looping: {loop})", Logging.AnimationSetup);

            return trackEntry;
        }

        public TrackEntry SetEmptyAnimation(int track, float mixDuration = 0.25f)
        {
            Log($"Set Empty Animation (Track: {track})", Logging.AnimationSetup);
            return animation.AnimationState.SetEmptyAnimation(track, mixDuration);
        }

        public TrackEntry QueueEmptyAnimation(int track, float mixDuration, float delay = 0)
        {
            Log($"Queue Empty Animation (Track: {track})", Logging.AnimationSetup);
            return animation.AnimationState.AddEmptyAnimation(track, mixDuration, delay);
        }
        
        // settings for individual clips

        public void SetAlpha(string clipName, float alpha)
        {
            if (!IsValidClip(clipName)) throw new InvalidClipException(clipName);
            Log($"Set Alpha (Clip: {clipName}, Alpha: {alpha})", Logging.AnimationSetup);
            alphas[clipName] = alpha;
        }

        public void UnsetAlpha(string clipName)
        {
            if (!IsValidClip(clipName)) throw new InvalidClipException(clipName);
            Log($"Unset Alpha (Clip: {clipName})", Logging.AnimationSetup);
            alphas.Remove(clipName);
        }

        public void SetTimeScale(string clipName, float timeScale)
        {
            if (!IsValidClip(clipName)) throw new InvalidClipException(clipName);
            Log($"Set Timescale (Clip: {clipName}, Timescale: {timeScale})", Logging.AnimationSetup);
            timeScales[clipName] = timeScale;
        }

        public void UnsetTimeScale(string clipName)
        {
            if (!IsValidClip(clipName)) throw new InvalidClipException(clipName);
            Log($"Unset Timescale (Clip: {clipName})", Logging.AnimationSetup);
            timeScales.Remove(clipName);
        }
        
        #endregion

        // PROPERTIES

        #region PROPERTIES

        public void SetCondition(string condition, bool active)
        {
            Log($"Set Condition (Key: {condition}, State: {active})", Logging.Properties);
            
            if (active)
            {
                if (properties.GetBool(condition)) return;
                properties.SetBool(condition, true);
                if (!conditionalStates.ContainsKey(condition)) return;
                foreach (var state in conditionalStates[condition]) state.Enter();
            }
            else
            {
                if (!properties.GetBool(condition)) return;
                properties.SetBool(condition, false);
                if (!conditionalStates.ContainsKey(condition)) return;
                foreach (var state in conditionalStates[condition]) state.Exit();
            }
        }

        public void SwapCondition(string previous, string next)
        {
            Log($"Swap Condition (Previous: {previous}, Next: {next})", Logging.Properties);
            
            if (properties.GetBool(previous))
                if (conditionalStates.ContainsKey(previous))
                    foreach (var state in conditionalStates[previous])
                        state.Exit();

            properties.SetBool(previous, false);

            if (!properties.GetBool(next))
                if (conditionalStates.ContainsKey(next))
                    foreach (var state in conditionalStates[next])
                        state.Enter();

            properties.SetBool(next, true);
        }

        public bool GetCondition(string condition) => properties.GetBool(condition);

        public void SetFloat(string name, float value)
        {
            Log($"Set Float Property (Key: {name}, Value: {value})", Logging.Properties);
            properties.SetFloat(name, value);
        }

        public float GetFloat(string name) => properties.GetFloat(name);
        
        public bool IsFloatDefined(string name) => properties.ContainsFloat(name);

        public void SetInt(string name, int value)
        {
            Log($"Set Int Property (Key: {name}, Value: {value})", Logging.Properties);
            properties.SetInt(name, value);
        }

        public int GetInt(string name) => properties.GetInt(name);
        
        public bool IsIntDefined(string name) => properties.ContainsInt(name);

        public void SetString(string name, string value)
        {
            Log($"Set String Property (Key: {name}, Value: {value})", Logging.Properties);
            properties.SetString(name, value);
        }

        public string GetString(string name) => properties.GetString(name);
        
        public bool IsStringDefined(string name) => properties.ContainsString(name);

        #endregion

        // PLAYBACK

        #region PLAYBACK

        protected virtual void OnTrackStart(TrackEntry trackEntry)
        {
            Log($"Track Start (Clip: {trackEntry.Animation.Name}, Track: {trackEntry.TrackIndex})", Logging.AnimationPlayback);
            
            var activeStates = GetStatesForClip(trackEntry.Animation.Name);
            if (activeStates == null) return;
            
            foreach (var state in activeStates) state.Enter(trackEntry);
        }

        protected virtual void OnTrackInterrupt(TrackEntry trackEntry)
        {
            Log($"Track Interrupt (Clip: {trackEntry.Animation.Name}, Track: {trackEntry.TrackIndex})", Logging.AnimationPlayback);
            var activeStates = GetStatesForClip(trackEntry.Animation.Name);
            if (activeStates == null) return;
            foreach (var state in activeStates) state.Exit(trackEntry);
        }

        protected virtual void OnTrackEnd(TrackEntry trackEntry)
        {
            Log($"Track End (Clip: {trackEntry.Animation.Name}, Track: {trackEntry.TrackIndex})", Logging.AnimationPlayback);
            var activeStates = GetStatesForClip(trackEntry.Animation.Name);
            if (activeStates == null) return;
            foreach (var state in activeStates) state.Exit(trackEntry);
        }
        
        // TODO: Handle tracks that end without being interrupted

        public virtual void Update(float deltaTime)
        {
            foreach (var globalState in globalStates) globalState.Update(deltaTime);

            foreach (var kvp in conditionalStates)
            {
                if (!properties.GetBool(kvp.Key)) continue;
                foreach (var state in kvp.Value) state.Update(deltaTime);
            }

            foreach (var trackEntry in animation.AnimationState.Tracks)
            {
                if (trackEntry == null) continue;
                
                var activeStates = GetStatesForClip(trackEntry.Animation.Name);
                if (activeStates == null) continue;

                foreach (var state in activeStates) state.Update(trackEntry, deltaTime);
            }
        }

        #endregion

        // EVENTS

        #region EVENTS

        public virtual void SubscribeToEvent(string eventName, Action action)
        {
            Log($"Event Subscribed (Event: {eventName})", Logging.EventSetup);
            if (events.ContainsKey(eventName)) events[eventName].Add(action);
            else events[eventName] = new List<Action> {action};
        }

        public virtual void UnsubscribeFromEvent(string eventName, Action action)
        {
            Log($"Event Unsubscribed (Event: {eventName})", Logging.EventSetup);
            if (!events.ContainsKey(eventName)) return;
            events[eventName].Remove(action);
            if (events[eventName].Count == 0) events.Remove(eventName);
        }

        protected virtual void OnEvent(TrackEntry trackentry, Event e)
        {
            var evtName = e.Data.Name;
            Log($"Event Fired (Event: {evtName})", Logging.EventPlayback);
            if (!events.ContainsKey(evtName)) return;
            foreach (var action in events[evtName]) action();
        }

        #endregion

        // CLEANUP

        #region CLEANUP

        public void ClearGlobalStates()
        {
            Log("Cleared Global States", Logging.StateSetup);
            globalStates.Clear();
        }

        public void ClearConditionalStates()
        {
            Log("Cleared Conditional States", Logging.StateSetup);
            conditionalStates.Clear();
        }

        public void ClearStates()
        {
            Log("Cleared Clip States", Logging.StateSetup);
            states.Clear();
        }

        public void ClearAllStates()
        {
            ClearGlobalStates();
            ClearConditionalStates();
            ClearStates();
            Log("Cleared All States", Logging.StateSetup);
        }

        public void ClearConditions()
        {
            Log("Cleared Conditional States", Logging.Properties);
            properties.ClearBools();
        }

        public void ClearProperties()
        {
            Log("Cleared All Properties", Logging.Properties);
            properties.ClearInts();
            properties.ClearFloats();
            properties.ClearStrings();
        }

        public void ClearAlphas()
        {
            Log("Cleared Alphas", Logging.AnimationSetup);
            alphas.Clear();
        }

        public void ClearTimeScales()
        {
            Log("Cleared Timescales", Logging.AnimationSetup);
            timeScales.Clear();
        }

        public void ClearAll()
        {
            Log("Cleared All Data (Full Reset)", Logging.Properties);

            globalStates.Clear();
            conditionalStates.Clear();
            properties.Clear();
            states.Clear();
            alphas.Clear();
            timeScales.Clear();

            animation.AnimationState.Start -= OnTrackStart;
            animation.AnimationState.Interrupt -= OnTrackInterrupt;
            animation.AnimationState.Event -= OnEvent;
        }

        #endregion

        // HELPERS
        
        #region HELPERS

        public TrackEntry GetCurrent(int track) => animation.AnimationState.GetCurrent(track);

        public bool IsTrackEmpty(int track) => GetCurrent(track) == null;

        public bool IsPlayingClip(int track, string clip) => GetCurrent(track)?.Animation.Name == clip;

        public bool IsValidClip(string clip) => validClips.Contains(clip);

        public bool IsClipPlaying(string clip) => IsClipPlaying(clip, out var _);
        
        public bool IsClipPlaying(string clip, out TrackEntry track)
        {
            foreach (var trackEntry in Animation.AnimationState.Tracks)
            {
                if (trackEntry.Animation.Name != clip) continue;
                track = trackEntry;
                return true;
            }

            track = null;
            return false;
        }

        private void GetValidClips()
        {
            validClips.Clear();
            foreach (var clip in Animation.Skeleton.Data.Animations.ToArray()) validClips.Add(clip.Name);
        }

        protected List<SpineFsmState> GetStatesForClip(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) throw new StringIsNullOrEmptyException(nameof(clipName));
            return states.ContainsKey(clipName) ? states[clipName] : null;
        }

        protected List<SpineFsmState> GetStatesForCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition)) throw new StringIsNullOrEmptyException(nameof(condition));
            return conditionalStates.ContainsKey(condition) ? conditionalStates[condition] : null;
        }

        #endregion

        // LOGGING

        #region LOGGING

        private Logging logging;

        public void SetLogging(Logging logging)
        {
            this.logging = logging;
            Log($"Set Logging: ({logging})", Logging.Properties);
        }

        public virtual void Log(string message, Logging category)
        {
            if (logging.HasFlag(category)) Debug.Log($"{Name} - {message}");
        }

        [Flags]
        public enum Logging
        {
            None = 0,
            States = 1 << 0,
            StateSetup = 1 << 1,
            AnimationSetup = 1 << 2,
            Properties = 1 << 3,
            EventSetup = 1 << 4,
            EventPlayback = 1 << 5,
            StatePlayback = 1 << 6,
            AnimationPlayback = 1 << 7,
            External = 1 << 8,
            All = -1
        }

        #endregion
    }
}