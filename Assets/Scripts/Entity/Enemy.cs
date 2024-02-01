public class Enemy : Entity
{
    public override void Initialize(GameController gameController)
    {
        base.Initialize(gameController);
        gameObject.SetActive(true);
    }
}
