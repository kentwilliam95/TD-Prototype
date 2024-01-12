namespace Core
{
    public class StateMachine<T>
    {
        private T owner;
        private IState<T> state;
        public IState<T> CurrentState => state;

        public StateMachine(T owner)
        {
            this.owner = owner;
        }

        public void ChangeState(IState<T> nextState)
        {
            if (state != null)
                state.OnStateExit(owner);

            state = nextState;
            state.OnStateEnter(owner);
        }

        public void OnUpdate()
        {
            if (state != null)
                state.OnStateUpdate(owner);
        }
    }
}