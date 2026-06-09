namespace _Game.Scripts.Template.GlobalProviders.Input
{
    public interface IClickableAction
    {
        void OnClickDown();
        
        void OnClickHold();
        
        void OnClickUp();
    }
}