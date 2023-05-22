using System.Collections;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace GameFoundation.Tutorial
{
    [RequireComponent(typeof(Canvas))]
    public class TutorialManager : MonoBehaviour
    {
        public List<TutorialData> tutorials;

        [Header("Default Tutorial Renderer")]
        public GameObject hand;
        public string handClip;
        public Button touch;
        public GameObject mask;
        public GameObject messageBox;
        public string messageBoxClip;
        public bool isPersistent = true;

        public Canvas Canvas { get; private set; }
        public TutorialModel Model { get; private set; }

        private TutorialData currentTutorial;

        private async UniTaskVoid Start()
        {
            // update canvas
            Canvas = GetComponent<Canvas>();
            Canvas.enabled = false;

            // wait for completed initialize
            await UniTask.WaitUntil(() => GameFoundationInitializer.Instance.Initialized);

            // make a persistent game object
            if (isPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }

            // register model
            Model = TutorialModel.Instance;
            Model.Register(true);

            // subscribe events
            Model.active.Where(id => !Model.IsCompleted(id)).Subscribe(Active);
            Model.nextStep.Subscribe(_ => NextStep()).AddTo(this);
            Model.showMessage.Subscribe(p => ShowMessageBox(p.text, p.clip, p.anchorPosition)).AddTo(this);
        }

        private void Active(string id)
        {
            currentTutorial = tutorials.FirstOrDefault(t => t.id == id);
            if (currentTutorial)
            {
                Canvas.enabled = true;
                currentTutorial.Active(this);
            }
        }

        private void NextStep()
        {
            currentTutorial?.NextStep();
        }

        public void OnCompleted(TutorialData tut)
        {
            Canvas.enabled = false;
            Model.completed.Add(tut.id);
            Model.active.Value = "";
            Model.onCompleted.Execute(tut.id);
            currentTutorial = null;
        }

        public void ShowMessageBox(string text, string clip, Vector2 anchorPosition)
        {
            messageBox.GetComponent<RectTransform>().anchoredPosition = anchorPosition;
            messageBox.SetActive(true);
            messageBox.GetComponentInChildren<TextMeshProUGUI>().text = text;

            // play animation
            var anim = messageBox.GetComponent<Animator>();
            var animClip = string.IsNullOrEmpty(clip) ? messageBoxClip : clip;
            if (anim != null)
            {
                anim.Play(animClip);
            }
        }
    }
}