#pragma warning disable CS0618

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using UnityEngine;

namespace GameFoundation.Data
{
    public class PersistenceDataLayer : IDataLayer
    {
        public bool Save(string name, object target)
        {
            name = GetFileName(name);
            using (var ms = new MemoryStream())
            {
                using (BsonWriter bw = new BsonWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(bw, target);
                }

                // save to file
                var path = Path.Combine(Application.persistentDataPath, name);
                File.WriteAllText(path, Convert.ToBase64String(ms.ToArray()));
                Debug.Log($"Save {name} to {path}");
            }
            return true;
        }

        public bool Load(string name, object target)
        {
            name = GetFileName(name);
            var path = Path.Combine(Application.persistentDataPath, name);
            Debug.Log($"Load {name} from {path}");

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
                    serializer.Populate(br, target);
                }
            }
            return true;
        }

        public bool Delete(string name)
        {
            name = GetFileName(name);
            var path = Path.Combine(Application.persistentDataPath, name);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        public bool Exists(string name)
        {
            name = GetFileName(name);
            var path = Path.Combine(Application.persistentDataPath, name);
            return File.Exists(path);
        }

        protected string GetFileName(string name)
        {
            name.Trim();
            if (!name.Contains(".dat"))
            {
                name = name + ".dat";
            }
            return name;
        }
    }
}
