using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.State
{
    public class Repository
    {
        private static Repository repository = null;

        public static Repository Instance => repository ??= new Repository();

        public Data.IDataLayer DataLayer { get; set; }

        private readonly IDictionary<string, Model> models;

        private readonly Container di_container;

        private Repository()
        {
            models = new Dictionary<string, Model>();
            di_container = new Container();
        }

        public T Register<T>(T model, bool reload = false, bool cacheType = true) where T : Model
        {
            var key = model.Name;
            if (models.ContainsKey(key))
            {
                Debug.LogWarning($"Model {key} already registed");
            }
            else
            {
                models.Add(key, model);
                if (cacheType) models.Add($"Type {model.GetType()}", model);
            }

            // reload model
            if (reload)
            {
                Load(key);
            }

            // register dependency injection
            RegisterDI(model);

            return model;
        }

        public bool UnRegister<T>(T model) where T : Model
        {
            models.Remove(model.Name);
            models.Remove($"Type {model.GetType()}");
            di_container.RemoveInstance(model);
            return true;
        }

        public T Get<T>(string name = "") where T : Model
        {
            var key = string.IsNullOrEmpty(name) ? $"Type {typeof(T)}" : name;
            if (models.TryGetValue(key, out Model model))
            {
                return model as T;
            }
            return null;
        }

        public void SaveAll()
        {
            foreach (var key in models.Keys)
            {
                if (key.IndexOf("Type ") < 0) Save(key);
            }
        }

        public bool Save(string name)
        {
            if (models.TryGetValue(name, out var model))
            {
                return Save(model);
            }
            return false;
        }

        public bool Save<T>(T model) where T : Model
        {
            if (model.GetType().IsSerializable)
            {
                model.PrepareSerialize();
                return DataLayer.Save(model.FileName, model);
            }
            return false;
        }

        public bool Load(string name)
        {
            if (models.TryGetValue(name, out var model))
            {
                return Load(model);
            }
            return false;
        }

        public bool Load<T>(T model) where T : Model
        {
            if (model.GetType().IsSerializable)
            {
                model.PrepareDeSerialize();
                return DataLayer.Load(model.FileName, model);
            }
            return false;
        }

        public bool DeleteSaveFile(string name)
        {
            return DataLayer.Delete(name);
        }

        public bool IsHaveSaveFile(string name)
        {
            return DataLayer.Exists(name);
        }
        
        public void RegisterDI<T>(T obj)
        {
            di_container.RegisterInstance(obj).AsSelf();
        }

        public void ResolveDI<T>(T obj)
        {
            di_container.InjectAll(obj);
        }
    }
}
