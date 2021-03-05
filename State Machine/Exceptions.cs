using System;

namespace SpineStateMachine
{
    public class StringIsNullOrEmptyException : Exception
    {
        public StringIsNullOrEmptyException(string message) : base(message) { }
    }
    
    public class InvalidClipException : Exception
    {
        public InvalidClipException(string clip) : base($"{clip} is not a valid clip") { }
    }

    public class InvalidStateOperationException : Exception
    {
        public InvalidStateOperationException(string methodName, bool active) 
            : base($"Calling {methodName}() on an {(active ? "active" : "inactive")} state is not allowed") { }
    }

    public class StateAlreadyRetainedException : Exception
    {
        public StateAlreadyRetainedException(SpineFsmState state) 
            : base($"{state.GetType().Name} was already retained by {state.fsm.Name}.") { }
    }
    
    public class StateNotFoundException : Exception
    {
        public StateNotFoundException(SpineFsm fsm, SpineFsmState state) : 
            base($"{state.GetType().Name} was not found in state machine {fsm.GetType().Name}") { }
    }
    
    public class NullTrackEntryException : Exception
    {
        public NullTrackEntryException(SpineFsmState state) : base($"Null track entry in state {state.GetType().Name}") { }
    }
}
