using Spine.Unity;

namespace SpineStateMachine
{
    public class SpineFsm<T> : SpineFsm where T : class
    {
        protected readonly T data;

        public SpineFsm(T data, SkeletonAnimation animation, Logging logging = 0) : base(animation, logging)
        {
            this.data = data;
        }

        public void AddGlobalState(SpineFsmState<T> state)
        {
            state.SetData(data);
            base.AddGlobalState(state);
        }

        public void AddConditionalState(string condition, SpineFsmState<T> state)
        {
            state.SetData(data);
            base.AddConditionalState(condition, state);
        }

        public void AddState(string clipName, SpineFsmState<T> state )
        {
            state.SetData(data);
            base.AddState(clipName, state);
        }
    }
}