namespace GameFoundation.Model
{
    public abstract class SingletonModel<T> : GFModel where T : new()
    {
        public static T Instance { get; private set; } = new T();
    }
}