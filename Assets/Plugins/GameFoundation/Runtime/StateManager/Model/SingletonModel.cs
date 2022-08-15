namespace GameFoundation.State
{
    public abstract class SingletonModel<T> : Model where T : new()
    {
        public static T Instance { get; private set; } = new T();
    }
}