namespace UniRx.Model
{
    public abstract class SingletonModel<T> : BaseModel where T : new()
    {
        public static T Instance { get; private set; } = new T();
    }
}