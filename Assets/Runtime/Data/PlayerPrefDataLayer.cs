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
    public class PlayerPrefDataLayer : IDataLayer
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
                PlayerPrefs.SetString(name, Convert.ToBase64String(ms.ToArray()));
            }
            return true;
        }

        public bool Load(string name, object target)
        {
            name = GetFileName(name);
            var text = PlayerPrefs.GetString(name, "");
            if (!string.IsNullOrEmpty(text))
            {
                var bytes = Convert.FromBase64String(text);

                // deserialize from bytes
                using (var ms = new MemoryStream(bytes))
                {
                    using (var br = new BsonReader(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Populate(br, target);
                    }
                }
            }
            return true;
        }

        public bool Delete(string name)
        {
            name = GetFileName(name);
            if (PlayerPrefs.HasKey(name))
            {
                PlayerPrefs.DeleteKey(name);
                return true;
            }
            return false;
        }

        public bool Exists(string name)
        {
            name = GetFileName(name);
            return PlayerPrefs.HasKey(name);
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
