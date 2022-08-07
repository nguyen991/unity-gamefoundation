using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public class PlatformOnly : MonoBehaviour
    {
        [System.Flags]
        public enum Platform
        {
            None = 0,
            Editor = 1 << 1,
            iOS = 1 << 2,
            Android = 1 << 3,
            Web = 1 << 4,
            Other = 1 << 5,
        }

        public Platform platform = Platform.Editor;

        private void Awake()
        {
            var result = false;
            var values = Enum.GetValues(typeof(Platform)).Cast<Platform>().Where(p => platform.HasFlag(p));
            foreach (var p in values)
            {
                switch (p)
                {
                    case Platform.Editor:
                        result = Application.isEditor;
                        break;
                    case Platform.iOS:
                        result = Application.platform == RuntimePlatform.IPhonePlayer;
                        break;
                    case Platform.Android:
                        result = Application.platform == RuntimePlatform.Android;
                        break;
                    case Platform.Web:
                        result = Application.platform == RuntimePlatform.WebGLPlayer;
                        break;
                }
                if (result) break;
            }
            if (!result && platform != Platform.Other)
            {
                Destroy(gameObject);
            }
        }
    }
}
