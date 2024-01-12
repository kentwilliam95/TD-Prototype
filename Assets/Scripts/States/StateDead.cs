using Core;

public class StateDead:IState<Entity>
{
    public void OnStateEnter(Entity t)
    {
        //Play dead animation here
        //remove from gamecontroller entity        
    }

    public void OnStateUpdate(Entity t)
    {
        
    }

    public void OnStateExit(Entity t)
    {
        
    }
}