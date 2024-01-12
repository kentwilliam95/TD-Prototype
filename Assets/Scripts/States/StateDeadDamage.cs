using Core;

public class StateDeadDamage : IState<Entity>
{
    public void OnStateEnter(Entity t)
    {
        if(t.GameController.GameState == GameController.State.End)
            return;
        
        t.OnArrivedAtDestination?.Invoke(1);
        GameController.Instance.UnRegisterEntity(t);
        //Play a dead animation 
    }

    public void OnStateUpdate(Entity t)
    {
    }

    public void OnStateExit(Entity t)
    {
    }
}
