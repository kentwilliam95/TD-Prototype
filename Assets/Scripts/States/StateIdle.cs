using Core;
using Unity.VisualScripting;
using UnityEngine;

public class StateIdle : IState<Entity>
{
    public void OnStateEnter(Entity t)
    {
        t.PlayAnimation(AnimationController.AnimationType.Idle);
    }

    public void OnStateUpdate(Entity t)
    {
        UpdateStateToRangeAttack(t);
        UpdateStateToMeleeAttack(t);
    }

    private void UpdateStateToRangeAttack(Entity t)
    {
        var enemy = t.CheckEnemyInRange(t, t.entityData.teamTarget);
        if(!enemy)
            return;
        t.entityData._target = enemy;
        t.ChangeState(Entity.State.AllyRangeAttack);
    }

    private void UpdateStateToMeleeAttack(Entity t)
    {
        Entity enemy = t.CheckIsEnemyOnTheSameGround(t, t.entityData.teamTarget);
        if (!enemy)
            return;
        
        t.entityData._target = enemy;
        t.ChangeState(Entity.State.AllyAttack);
    }

    public void OnStateExit(Entity t)
    {
    }
}
