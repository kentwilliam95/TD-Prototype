using Core;
using UnityEngine;

//this will make each entity will stay on the spot and attack it's target
//after the target is dead, it will continue to it's end location
public class StateAttack : IState<Entity>
{
    protected float attackCounter;
    protected float transitionOffCounter;
    protected float transitionOnCounter;
    protected Entity ent;
    protected bool isAttacking;
    protected bool isTriggered;

    public void OnStateEnter(Entity t)
    {
        if(t.GameController.GameState == GameController.State.End)
            return;
        
        ent = t;
        ent.StopAgent();
        ent.PlayAnimation(AnimationController.AnimationType.Idle);
        Reset();
    }

    public void OnStateUpdate(Entity t)
    {
        if (t.GameController.GameState == GameController.State.End)
            return;
        UpdateState();
    }

    protected virtual void UpdateState()
    {
        //TODO: Add check condition when arrived at destination then return
        if(ent.CheckNearObjective())
        {
            // ent.ChangeState(new StateDeadDamage());
            ent.ChangeState(Entity.State.DeadAndDamage);
            return;
        }
            
        TransitionOnAttack();
        TransitionOffAttack();
        TransitionToAttack();
    }

    protected virtual void OnAttackTrigger()
    {        
        ent.entityData._target.Damage(ent.entityData._damage);

        isTriggered = true;
        Reset();
    }

    protected virtual void TransitionOnAttack()
    {
        if (!isAttacking)
            return;

        if (isTriggered)
            return;

        transitionOnCounter -= Time.deltaTime;
        if (transitionOnCounter > 0)
            return;

        OnAttackTrigger();
    }

    protected virtual void TransitionOffAttack()
    {
        if (!isTriggered)
            return;

        transitionOffCounter -= Time.deltaTime;
        if (transitionOffCounter > 0)
            return;

        if (ent.entityData._target.IsDead)
        {
            ent.entityData._target = null;
            if (ent is Enemy)
                // ent.ChangeState(new StateMove());
                ent.ChangeState(Entity.State.Move);
            else if (ent is Unit)
                // ent.ChangeState(new StateIdle());
                ent.ChangeState(Entity.State.Idle);
        }
        else
        {
            attackCounter = ent.entityData._duration;
            isTriggered = false;
            isAttacking = false;
        }
    }

    protected virtual void TransitionToAttack()
    {
        if (isAttacking)
            return;

        attackCounter -= Time.deltaTime;
        if (attackCounter > 0)
            return;

        var enemy = ent.entityData._target;
        isAttacking = true;
        ent.StopAgent();
        ent.PlayAnimation(AnimationController.AnimationType.Attack);
    }

    protected virtual void Reset()
    {
        transitionOffCounter = 0.5f;
        transitionOnCounter = 0.5f;
    }

    public void OnStateExit(Entity t)
    {
    }
}