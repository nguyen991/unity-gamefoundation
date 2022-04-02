using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UniRx.Model
{
    public class Repository
    {
        private static Repository repository = null;

        public static Repository Instance => repository ?? (repository = new Repository());

        private IDictionary<string, BaseModel> models;

        private Repository()
        {
            models = new Dictionary<string, BaseModel>();
        }

        public T Register<T>(T model, bool reload = false, bool cacheType = true) where T : BaseModel
        {
            var key = model.Name();
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

            return model;
        }

        public bool UnRegister<T>(T model) where T : BaseModel
        {
            models.Remove(model.Name());
            models.Remove($"Type {model.GetType()}");
            return true;
        }

        public T Get<T>(string name = "") where T : BaseModel
        {
            var key = string.IsNullOrEmpty(name) ? $"Type {typeof(T)}" : name;
            if (models.TryGetValue(key, out BaseModel model))
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

        public void Save(string name)
        {
            if (models.TryGetValue(name, out var model) && model.GetType().IsSerializable)
            {
                var json = JsonConvert.SerializeObject(model);
                PlayerPrefs.SetString(model.Name(), json);
            }
        }

        public void Load(string name)
        {
            var json = PlayerPrefs.GetString(name);
            if (!string.IsNullOrEmpty(json))
            {
                JsonConvert.PopulateObject(json, models[name]);
            }
        }
    }
}