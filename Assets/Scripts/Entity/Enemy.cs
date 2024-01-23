public class Enemy : Entity
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize(GameController gameController)
    {
        base.Initialize(gameController);
        _team = Global.TEAMENEMY;
        entityData.team = Global.TEAMENEMY;
        entityData.teamTarget = Global.TEAMALLY;
        gameObject.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();
    }
}
