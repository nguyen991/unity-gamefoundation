using GameFoundation.State;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace GameFoundation.Tutorial
{
    [System.Serializable]
    public class TutorialModel : SingletonModel<TutorialModel>
    {
        public HashSet<string> completed = new HashSet<string>();

        [JsonIgnore] public ReactiveProperty<string> active = new ReactiveProperty<string>();

        public ReactiveCommand<string> eventName = new ReactiveCommand<string>();
        public ReactiveCommand<string> endEventName = new ReactiveCommand<string>();
        public ReactiveCommand nextStep = new ReactiveCommand();
        public ReactiveCommand<(string text, string clip, Vector2 anchorPosition)> showMessage = new ReactiveCommand<(string text, string clip, Vector2 anchorPosition)>();
        public ReactiveCommand<string> onCompleted = new ReactiveCommand<string>();

        public bool IsCompleted(string id) => completed.Contains(id);
        public bool IsActive(string id) => !string.IsNullOrEmpty(active.Value) && active.Value == id;
        public bool IsActive() => !string.IsNullOrEmpty(active.Value);
    }
}