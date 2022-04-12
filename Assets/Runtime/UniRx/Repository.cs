using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;
using System;

namespace UniRx.Model
{
    public class Repository
    {
        private static Repository repository = null;

        public static Repository Instance => repository ?? (repository = new Repository());

        private IDictionary<string, UniRxModel> models;

        private Repository()
        {
            models = new Dictionary<string, UniRxModel>();
        }

        public T Register<T>(T model, bool reload = false, bool cacheType = true) where T : UniRxModel
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

            return model;
        }

        public bool UnRegister<T>(T model) where T : UniRxModel
        {
            models.Remove(model.Name);
            models.Remove($"Type {model.GetType()}");
            return true;
        }

        public T Get<T>(string name = "") where T : UniRxModel
        {
            var key = string.IsNullOrEmpty(name) ? $"Type {typeof(T)}" : name;
            if (models.TryGetValue(key, out UniRxModel model))
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

        public bool Save<T>(T model) where T : UniRxModel
        {
            if (!model.GetType().IsSerializable)
            {
                return false;
            }

            using (var ms = new MemoryStream())
            {
                using (BsonWriter bw = new BsonWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(bw, model);
                }

                // save to file
                var path = Path.Combine(Application.persistentDataPath, model.FileName);
                File.WriteAllText(path, Convert.ToBase64String(ms.ToArray()));
                Debug.Log($"Save {model.Name} to {path}");
            }
            return true;
        }

        public bool Load(string name)
        {
            if (models.TryGetValue(name, out var model))
            {
                return Load(model);
            }
            return false;
        }

        public bool Load<T>(T model) where T : UniRxModel
        {
            var path = Path.Combine(Application.persistentDataPath, model.FileName);
            Debug.Log($"Load {model.Name} from {path}");

            // check file exists
            if (!File.Exists(path))
            {
                return false;
            }

            // read file
            var bytes = Convert.FromBase64String(File.ReadAllText(path));

            // deserialize from bytes
            using (var ms = new MemoryStream(bytes))
            {
                using (var br = new BsonReader(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Populate(br, model);
                }
            }
            return true;
        }

        public void DeleteSaveFile(string name)
        {
            var path = Path.Combine(Application.persistentDataPath, name);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public bool IsHaveSaveFile(string name)
        {
            var path = Path.Combine(Application.persistentDataPath, name);
            return File.Exists(path);
        }
    }
}