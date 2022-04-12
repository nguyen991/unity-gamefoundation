namespace UniRx.Model
{
    public abstract class SingletonModel<T> : UniRxModel where T : new()
    {
        public static T Instance { get; private set; } = new T();
    }
}