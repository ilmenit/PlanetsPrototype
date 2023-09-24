using System;
using System.Collections.Generic;

public class FSM<StateType, CommandType>
{
    private List<FSMState> FSMStates = new List<FSMState>();

    public FSMState CurrentState { get; private set; }

    public class FSMState
    {
        private FSM<StateType, CommandType> owner;
        private StateType state;
        private SortedDictionary<CommandType, StateType> transitions = new SortedDictionary<CommandType, StateType>();

        public delegate void StateChangeHandler();

        public StateChangeHandler onEntryHandler = null;
        public StateChangeHandler onExitHandler = null;

        public StateType State
        {
            get
            {
                return state;
            }

            private set
            {
                state = value;
            }
        }

        public SortedDictionary<CommandType, StateType> Transitions
        {
            get
            {
                return transitions;
            }

            private set
            {
                transitions = value;
            }
        }

        public FSMState(StateType state, FSM<StateType, CommandType> owner)
        {
            State = state;
            this.owner = owner;
        }

        public FSMState Permit(CommandType command, StateType state)
        {
            Transitions[command] = state;
            owner.Configure(state);
            return this;
        }

        public FSMState OnEntry(StateChangeHandler handler)
        {
            onEntryHandler = handler;
            return this;
        }

        public FSMState OnExit(StateChangeHandler handler)
        {
            onExitHandler = handler;
            return this;
        }

        // public FSMState OnEntryFrom(StateChangeHandler handler)
        // public FSMState OnExitTo(StateChangeHandler handler)
    }

    private class FSMTransition
    {
        public StateType CurrentState;
        public CommandType Command;
        // public StateChangeHandler onTransition;

        public FSMTransition(StateType currentState, CommandType command)
        {
            CurrentState = currentState;
            Command = command;
        }

        public override int GetHashCode()
        {
            return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
        }

        public bool Equals(FSMTransition other)
        {
            return other != null && EqualityComparer<StateType>.Default.Equals(CurrentState, other.CurrentState) && EqualityComparer<CommandType>.Default.Equals(Command, other.Command);
        }
    }

    public FSMState Configure(StateType state)
    {
        FSMState current = FSMStates.Find(x => EqualityComparer<StateType>.Default.Equals(state, x.State));
        if (current == null)
        {
            current = new FSMState(state, this);
            FSMStates.Add(current);
        }
        return current;
    }

    public FSM(StateType defaultState)
    {
        CurrentState = Configure(defaultState);
    }

    public StateType GetNextStateType(CommandType command)
    {
        StateType nextState;
        if (!CurrentState.Transitions.TryGetValue(command, out nextState))
            throw new Exception("FSM: Invalid transition from " + CurrentState.State + " through " + command);
        return nextState;
    }

    public FSMState GetNext(CommandType command)
    {
        StateType nextStateType = GetNextStateType(command);
        FSMState next = FSMStates.Find(x => EqualityComparer<StateType>.Default.Equals(nextStateType, x.State));
        return next;
    }

    public FSMState Fire(CommandType command)
    {
        FSMState nextState = GetNext(command);
        if (CurrentState.onExitHandler != null)
            CurrentState.onExitHandler();
        CurrentState = nextState;
        if (CurrentState.onEntryHandler != null)
            CurrentState.onEntryHandler();
        return CurrentState;
    }
}