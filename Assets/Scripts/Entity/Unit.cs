public class Unit : Entity
{
    public override void Initialize(GameController gameController)
    {
        base.Initialize(gameController);
        _team = GameController.TEAMALLY;
        entityData.team = GameController.TEAMALLY;
        entityData.teamTarget = GameController.TEAMENEMY;
    }    
}
