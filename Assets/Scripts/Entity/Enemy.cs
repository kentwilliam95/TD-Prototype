public class Enemy : Entity
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize(GameController gameController)
    {
        base.Initialize(gameController);
        _team = GameController.TEAMENEMY;
        entityData.team = global::GameController.TEAMENEMY;
        entityData.teamTarget = global::GameController.TEAMALLY;
        gameObject.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();
    }
}
