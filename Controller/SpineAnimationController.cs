using Spine.Unity;
using UnityEngine;

namespace SpineStateMachine.Unity
{
    [RequireComponent(typeof(SkeletonAnimation))]
    public abstract class SpineAnimationController : MonoBehaviour
    {
        public SpineFsm.Logging loggingFlags = SpineFsm.Logging.None;
        public bool updateSkeleton;

        protected SkeletonAnimation anim { get; private set; }
        protected SpineFsm fsm { get; private set; }
        public bool initialized { get; private set; }

        protected void Awake()
        {
            anim = GetComponent<SkeletonAnimation>();
            fsm = new SpineFsm(anim, loggingFlags);
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