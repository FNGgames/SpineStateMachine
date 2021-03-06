using System;

namespace SpineStateMachine
{
    public class StringIsNullOrWhitespaceException : Exception
    {
        public StringIsNullOrWhitespaceException(string argumentName) : base(
            $"{argumentName} cannot be null, empty or whitespace") { }
    }

    public class InvalidClipException : Exception
    {
        public InvalidClipException(string clip) : base($"{clip} is not a valid clip") { }
    }

    public class StateAlreadyRetainedException : Exception
    {
        public StateAlreadyRetainedException(SpineFsmState state) : base(
            $"{state.GetType().Name} was already retained by {state.fsm.Name}.") { }
    }

    public class StateNotRetainedException : Exception
    {
        public StateNotRetainedException(SpineFsmState state) : base(
            $"{state.GetType().Name} was not yet retained by a state machine. You cannot call lifecycle methods on an orphan state") { }
    }

    public class StateNotFoundException : Exception
    {
        public StateNotFoundException(SpineFsm fsm, SpineFsmState state) : base(
            $"{state.GetType().Name} was not found in state machine {fsm.GetType().Name}") { }
    }

    public class NullTrackEntryException : Exception
    {
        public NullTrackEntryException(SpineFsmState state) :
            base($"Null TrackEntry in state {state.GetType().Name}") { }
    }
}