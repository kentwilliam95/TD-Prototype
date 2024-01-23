public class Unit : Entity
{
    public override void Initialize(GameController gameController)
    {
        base.Initialize(gameController);
        _team = Global.TEAMALLY;
        entityData.team = Global.TEAMALLY;
        entityData.teamTarget = Global.TEAMENEMY;
    }    
}
