using System;
using Spine;
namespace SpineStateMachine
{
    public abstract class SpineFsmState
    {
        // readonly properties 
        // kept up-to-date by the state machine

        public SpineFsm fsm { get; private set; }
        public TrackEntry trackEntry { get; private set; }
        public string key { get; private set; }
        public bool active { get; private set; }
        public bool retained { get; private set; }

        // fsm retain and release methods

        public void Retain(SpineFsm fsm, string key)
        {
            if (retained) throw new StateAlreadyRetainedException(this);
            this.fsm = fsm ?? throw new ArgumentNullException(nameof(fsm));
            this.key = key;
            retained = true;
        }

        public void Release()
        {
            Exit();
            fsm = null;
            key = null;
            trackEntry = null;
            retained = false;
        }

        // public methods for conditional or global states

        public void Enter()
        {
            if (!retained) throw new StateNotRetainedException(this);
            if (active) return;
            active = true;
            fsm?.Log($"Enter State {GetType().Name} ({key})", SpineFsm.Logging.StatePlayback);
            OnEnter();
        }

        public void Update(float deltaTime)
        {
            if (!retained) throw new StateNotRetainedException(this);
            if (!active) return;
            OnUpdate(deltaTime);
        }

        public void Exit()
        {
            if (!retained) throw new StateNotRetainedException(this);
            if (!active) return;
            active = false;
            fsm?.Log($"Exit State {GetType().Name} ({key})", SpineFsm.Logging.StatePlayback);
            OnExit();
        }

        // public methods for clip-based states (passing in the TrackEntry to keep it up-to-date)

        public void Enter(TrackEntry te)
        {
            if (!retained) throw new StateNotRetainedException(this);
            trackEntry = te ?? throw new NullTrackEntryException(this);
            Enter();
        }

        public void Update(TrackEntry te, float deltaTime)
        {
            if (!retained) throw new StateNotRetainedException(this);
            trackEntry = te ?? throw new NullTrackEntryException(this);
            Update(deltaTime);
        }

        public void Exit(TrackEntry te)
        {
            if (!retained) throw new StateNotRetainedException(this);
            trackEntry = te ?? throw new NullTrackEntryException(this);
            Exit();
        }

        // virtual methods for state classes

        protected virtual void OnEnter() { }

        protected virtual void OnUpdate(float deltaTime) { }

        protected virtual void OnExit() { }

        // logging

        protected void Log(string message) => fsm?.Log($"{GetType().Name}: {message}", SpineFsm.Logging.States);
    }
}