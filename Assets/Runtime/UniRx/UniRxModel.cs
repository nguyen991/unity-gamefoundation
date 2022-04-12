using UnityEngine;

namespace UniRx.Model
{
    public abstract class UniRxModel
    {
        public string Name { get; set; } = "";

        public string FileName { get; set; } = "";

        public UniRxModel(string name = "")
        {
            Name = string.IsNullOrEmpty(name) ? GetType().ToString() : name;
            FileName = Name + ".dat";
            Debug.Log($"Create Model {Name}");
        }

        public virtual void Register(bool reload = false, bool cacheType = false)
        {
            Repository.Instance.Register(this, reload, cacheType);
        }

        public virtual void UnRegister()
        {
            Repository.Instance.UnRegister(this);
        }

        public virtual void Save()
        {
            Repository.Instance.Save(this);
        }

        public virtual void Load()
        {
            Repository.Instance.Load(this);
        }
    }
}