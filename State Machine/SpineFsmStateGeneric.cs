using System;

namespace SpineStateMachine
{
    public class SpineFsmState<T> : SpineFsmState where T : class
    {
        public T data { get; private set; }

        public void SetData(T data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}