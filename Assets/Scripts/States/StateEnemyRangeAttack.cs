using UnityEngine;

//this should be attack and move at the same time,
//and will walk to an objective
//if there is no enemy or the enemy is outside of its attack range
public class StateEnemyRangeAttack : StateAttack
{
    private float walkCounter = 0;
    private bool isWalking;

    protected override void OnAttackTrigger()
    {
        if (ent == null || ent.entityData._target == null)
            return;
        
        isTriggered = true;
        Reset();

        Projectile projectile = GameObject.Instantiate(ent.entityData._projectile);
        var start = ent.GetBone(HumanBodyBones.RightHand);
        var end = ent.entityData._target.transform;
        projectile.Initialize(start, end, ent.entityData._projectileSpeed);
        projectile.onHitTarget = () => { ent.entityData._target.Damage(ent.entityData._damage); };
    }

    protected override void UpdateState()
    {
        //TODO: Add check condition when arrived at destination

        base.UpdateState();
        if (ent.entityData._target.IsDead)
        {
            ent.entityData._target = null;
            if (ent is Enemy)
                ent.ChangeState(Entity.State.Move);
            else if (ent is Unit)
                ent.ChangeState(Entity.State.Idle);
        }
        
        UpdateWalkState();
        UpdateStateToMove();
    }

    private void UpdateStateToMove()
    {
        Entity enemy = ent.CheckIsEnemyOnTheSameGround(ent, Global.TEAMALLY);
        if (enemy != null && !enemy.IsDead)
        {
            ent.entityData._target = enemy;
            ent.ChangeState(Entity.State.EnemyAttack);
        }
    }

    protected override void TransitionOffAttack()
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
                ent.ChangeState(Entity.State.Move);
            else if (ent is Unit)
                ent.ChangeState(Entity.State.Idle);
        }
        else
        {
            attackCounter = ent.entityData._duration;
            walkCounter = 0.25f;
            isWalking = true;
            isTriggered = false;
            isAttacking = false;
            ent.PlayAnimation(AnimationController.AnimationType.Run);
            ent.StartAgent();
        }
    }

    private void UpdateWalkState()
    {
        if (!isWalking)
            return;

        walkCounter -= Time.deltaTime;
        if (walkCounter > 0)
            return;

        isWalking = false;
        Reset();
        ent.StopAgent();
        ent.PlayAnimation(AnimationController.AnimationType.Idle);
        attackCounter = ent.entityData._duration;
        walkCounter = 0.25f;
    }

    protected override void PlayAnimationAttack()
    {
        ent.PlayAnimation(AnimationController.AnimationType.RangeAttack);
    }

    protected override void Reset()
    {
        transitionOnCounter = ent.UnitSO._rduration;
        transitionOffCounter = Mathf.Min(ent.UnitSO._rClip.length - transitionOnCounter, 0.5f);
    }
}