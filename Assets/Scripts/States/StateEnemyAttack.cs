using UnityEngine;

namespace States
{
    public class StateEnemyAttack : StateAttack
    {
        protected override void OnAttackTrigger()
        {
            base.OnAttackTrigger();
        }

        protected override void UpdateState()
        {
            //TODO: Add check condition when arrived at destination then return
            if (ent.CheckNearObjective())
            {
                // ent.ChangeState(new StateDeadDamage());
                ent.ChangeState(Entity.State.DeadAndDamage);
                return;
            }
            
            base.UpdateState();
        }

        protected override void ChangeTargetAfterAttack()
        {
            ent.ChangeState(Entity.State.Move);
        }

        protected override void PlayAnimationAttack()
        {
            ent.PlayAnimation(AnimationController.AnimationType.MeleeAttack);
        }

        protected override void Reset()
        {
            base.Reset();
        }
    }
}