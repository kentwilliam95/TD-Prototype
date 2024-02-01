namespace Core
{
    public interface IState<T>
    {
        void OnStateEnter(T t);
        void OnStateUpdate(T t);
        void OnStateExit(T t);
    }
}