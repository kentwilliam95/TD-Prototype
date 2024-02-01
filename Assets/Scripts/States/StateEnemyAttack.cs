using UnityEngine;

namespace States
{
    public class StateEnemyAttack : StateAttack
    {
        public override void OnStateEnter(Entity t)
        {
            base.OnStateEnter(t);
            t.WeaponHandler.Equip(WeaponHandler.WeaponType.Melee);
        }

        protected override void UpdateStateProgress()
        {
            //TODO: Add check condition when arrived at destination then return
            if (ent.CheckNearObjective())
            {
                // ent.ChangeState(new StateDeadDamage());
                ent.ChangeState(Entity.State.DeadAndDamage);
                return;
            }
            
            base.UpdateStateProgress();
        }

        protected override void ChangeTargetAfterAttack()
        {
            ent.ChangeState(Entity.State.Move);
        }

        protected override void PlayAnimationAttack()
        {
            ent.PlayAnimation(AnimationController.AnimationType.MeleeAttack);
        }

        public override void OnStateExit(Entity t)
        {
            t.WeaponHandler.UnEquip(WeaponHandler.WeaponType.Melee);
        }
    }
}