using Spine.Unity;
using UnityEngine;

namespace SpineStateMachine.Unity
{
    [RequireComponent(typeof(SkeletonAnimation))]
    public abstract class SpineAnimationController<T> : MonoBehaviour where T : class
    {
        public SpineFsm.Logging loggingFlags = SpineFsm.Logging.None;
        public bool updateSkeleton;

        protected SkeletonAnimation anim { get; private set; }
        protected SpineFsm<T> fsm { get; private set; }
        public bool initialized { get; private set; }
        
        protected abstract T data { get; }

        protected void Awake()
        {
            anim = GetComponent<SkeletonAnimation>();
            fsm = new SpineFsm<T>(data, anim, loggingFlags);
            initialized = true;
            Setup();
        }

        private void Update()
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