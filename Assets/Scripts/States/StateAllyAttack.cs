using UnityEngine;

namespace States
{
    public class StateAllyRangeAttack : StateAttack
    {
        protected override void UpdateStateProgress()
        {
            var enemy = ent.CheckIsEnemyOnTheSameGround();
            if (enemy)
            {
                ent.ChangeState(Entity.State.AllyAttack);
                return;
            }

            base.UpdateStateProgress();
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