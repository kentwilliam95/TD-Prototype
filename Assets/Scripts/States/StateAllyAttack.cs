using UnityEngine;

namespace States
{
    public class StateAllyRangeAttack : StateAttack
    {
        //Do range attack
        protected override void OnAttackTrigger()
        {
            base.OnAttackTrigger();
        }

        protected override void UpdateState()
        {
            var enemy = ent.CheckIsEnemyOnTheSameGround(ent, ent.entityData.teamTarget);
            if (enemy)
            {
                ent.ChangeState(Entity.State.AllyAttack);
                return;
            }

            base.UpdateState();
        }

        protected override void ChangeTargetAfterAttack()
        {
            ent.ChangeState(Entity.State.Idle);
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

    public class StateAllyAttack:StateAttack
    {
        protected override void ChangeTargetAfterAttack()
        {
            ent.ChangeState(Entity.State.Idle);
        }

        protected override void PlayAnimationAttack()
        {
            ent.PlayAnimation(AnimationController.AnimationType.MeleeAttack);
        }
    }
}