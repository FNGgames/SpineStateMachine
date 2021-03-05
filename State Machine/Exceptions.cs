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
    
    public class NullTrackEntryException : Exception
    {
        public NullTrackEntryException() : base($"Null track entry in state") { }
    }

    public class InvalidStateOperationException : Exception
    {
        public InvalidStateOperationException(string methodName, bool active) 
            : base($"Calling {methodName}() on an {(active ? "active" : "inactive")} state is not allowed") { }
    }
    
    public class StateNotRetainedException : Exception
    {
        public StateNotRetainedException() : base("State was not retained by a state machine") { }
    }
}
