using Core;
using UnityEngine;

public class StateMove : IState<Entity>
{
    public void OnStateEnter(Entity t)
    {
        if(t.GameController.GameState == GameController.State.End)
            return;
                
        t.StartAgent();
        t.PlayAnimation(AnimationController.AnimationType.Run);
    }

    public void OnStateUpdate(Entity t)
    {
        if(t.GameController.GameState == GameController.State.End)
            return;
        
        if (t.CheckNearObjective())
        {
            // t.ChangeState(new StateDeadDamage());
            t.ChangeState(Entity.State.DeadAndDamage);
            return;
        }

        //Prioritize Enemy on the same ground first
        //Melee attack
        Entity enemy = t.CheckIsEnemyOnTheSameGround(t, GameController.TEAMALLY);
        if (enemy != null && !enemy.IsDead)
        {            
            t.entityData._target = enemy;
            var dist = Vector3.Distance(t.entityData._target.transform.position, t.transform.position);
            if (dist < 1) //1 should be replaced with this unit attack range
            {
                // t.ChangeState(new StateAttack());
                t.ChangeState(Entity.State.Attack);
                return;
            }
        }

        // if there is no enemy on the same ground check for enemy on different attack range
        // Range attack
        enemy = t.CheckEnemyInRange(t, GameController.TEAMALLY);
        if (enemy != null && !enemy.IsDead)
        {            
            t.entityData._target = enemy;
            // t.ChangeState(new StateAttackandMove());
            t.ChangeState(Entity.State.AttackAndMove);
            return;
        }
    }

    public void OnStateExit(Entity t)
    {
    }
}