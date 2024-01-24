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
            ChangeTargetAfterAttack();
        }
        else
        {
            attackCounter = ent.entityData._duration;
            isTriggered = false;
            isAttacking = false;
        }
    }

    protected virtual void ChangeTargetAfterAttack()
    {
        
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
        PlayAnimationAttack();
    }

    protected virtual void PlayAnimationAttack()
    {
        
    }

    protected virtual void Reset()
    {
        transitionOffCounter = Mathf.Min(ent.UnitSO._mClip.length - ent.UnitSO._duration, 0.5f);
        transitionOnCounter = ent.UnitSO._duration;
    }

    public void OnStateExit(Entity t)
    {
    }
}