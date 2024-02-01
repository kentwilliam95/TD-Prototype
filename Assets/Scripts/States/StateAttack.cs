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

    public virtual void OnStateEnter(Entity t)
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
        UpdateStateProgress();
    }

    protected virtual void UpdateStateProgress()
    {
        TransitionOnAttack();
        TransitionOffAttack();
        TransitionToAttack();
    }

    private void OnAttackTrigger()
    {
        isTriggered = true;
        DamageOtherEntity();
        Reset();
    }

    protected virtual void DamageOtherEntity()
    {
        ent.entityData._target.Damage(ent.UnitSO._damage);
    }

    private void TransitionOnAttack()
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
        
        attackCounter = ent.entityData._duration;
        isTriggered = false;
        isAttacking = false;
        ChangeTargetAfterAttack();
    }

    protected virtual void ChangeTargetAfterAttack()
    {
        if (ent.entityData._target.IsDead)
        {
            ent.entityData._target = null;
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

    public virtual void OnStateExit(Entity t)
    {
        
    }
}