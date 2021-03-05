﻿using Spine;
using Spine.Unity;
using UnityEngine;

namespace SpineStateMachine
{
    [RequireComponent(typeof(SkeletonAnimation))]
    public abstract class SpineAnimationController : MonoBehaviour
    {
        public SpineFsm.Logging loggingFlags = SpineFsm.Logging.None;
        public bool updateSkeleton;

        protected SkeletonAnimation anim;
        protected SpineFsm fsm;
        protected AnimationStateData data;

        public bool initialized { get; private set; }

        public void Awake()
        {
            anim = GetComponent<SkeletonAnimation>();
            fsm = new SpineFsm(anim, loggingFlags);
            data = anim.AnimationState.Data;
            initialized = true;
            Setup();
        }

        protected void Update()
        {
            if (!initialized) return;
            OnBeforeUpdate();
            fsm.Update(Time.deltaTime);
            OnAfterUpdate();
            if (updateSkeleton) fsm.Animation.Update(Time.deltaTime);
        }

        protected void Log(string message)
        {
            fsm?.Log(message, SpineFsm.Logging.External);
        }

        protected abstract void Setup();
        protected virtual void OnBeforeUpdate() { }
        protected virtual void OnAfterUpdate() { }
    }
}