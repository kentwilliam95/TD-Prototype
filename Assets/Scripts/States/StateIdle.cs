using Core;

public class StateIdle : IState<Entity>
{
    public void OnStateEnter(Entity t)
    {
        t.PlayAnimation(AnimationController.AnimationType.Idle);
    }

    public void OnStateUpdate(Entity t)
    {
        Entity enemy = t.CheckIsEnemyOnTheSameGround(t, t.entityData.teamTarget);
        if (!enemy)
            return;
        
        t.entityData._target = enemy;
        // t.ChangeState(new StateAttack());
        t.ChangeState(Entity.State.Attack);
    }

    public void OnStateExit(Entity t)
    {
    }
}
