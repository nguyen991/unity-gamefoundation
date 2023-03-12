using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using GameFoundation.Utilities;
using UnityEngine.XR;

namespace GameFoundation.Tutorial
{
    [System.Flags]
    public enum FocusMode
    {
        None = 0,
        Hand = 1 << 1,
        Top = 1 << 2,
        Mask = 1 << 3,
        Touch = 1 << 4,
    }

    [System.Serializable]
    public class TutorialStep
    {
        public float delay = 0;
        public string eventName;
        public string focus;
        public FocusMode focusMode;
        public string handClip = null;
        public List<string> actives = new List<string>();
        public List<string> deactives = new List<string>();

        [Header("Message Box")]
        [TextArea]
        public string message = "";
        public Vector2 messageBoxPosition = Vector2.zero;
        public bool alignWithFocus = true;
        public string messageBoxClip = null;
    }

    [CreateAssetMenu(fileName = "Tutorial", menuName = "Game Foundation/Tutorial/Create Tutorial")]
    public class TutorialData : ScriptableObject
    {
        public string id;
        public List<TutorialStep> steps = new List<TutorialStep>();

        private TutorialManager tutorial;
        private GameObject interactive;
        private Transform interactiveParent;
        private Vector3 interactiveOriginPosition;

        private int currentStep = 0;

        public void Active(TutorialManager tutorial)
        {
            this.tutorial = tutorial;
            interactive = null;
            currentStep = -1;
            NextStep();
        }

        public void NextStep()
        {
            currentStep++;

            if (currentStep > 0 && !string.IsNullOrEmpty(steps[currentStep - 1].eventName))
            {
                tutorial.Model.endEventName.Execute(steps[currentStep - 1].eventName);
            }
            if (currentStep >= steps.Count)
            {
                OnCompleted();
            }
            else
            {
                NextStep(currentStep).Forget();
            }
        }

        private async UniTaskVoid NextStep(int step)
        {
            var tut = steps[step];
            var container = tutorial.transform;
            var hand = tutorial.hand;
            var handClip = string.IsNullOrEmpty(tut.handClip) ? tutorial.handClip : tut.handClip;
            var messageClip = tut.messageBoxClip != null ? tut.messageBoxClip : tutorial.messageBoxClip;
            var touch = tutorial.touch;

            DeactiveStep(step - 1);

            // delay
            if (tut.delay > 0.001f)
                await UniTask.Delay(System.TimeSpan.FromSeconds(tut.delay), ignoreTimeScale: true);

            // active game objects
            if (steps[step].actives.Count() > 0)
            {
                steps[step].actives.Select(path => GameObject.Find(path)).Where(obj => obj != null).ToList().ForEach(obj => obj.SetActive(true));
                await UniTask.NextFrame();
            }

            // find interactive object
            if (!string.IsNullOrEmpty(tut.focus))
            {
                interactive = GameObject.Find(tut.focus);
                if (!interactive)
                {
                    Debug.LogError("Could not find focus object: " + tut.focus);
                    return;
                }
            }

            // deactivate game objects
            steps[step].deactives
                .Select(path => GameObject.Find(path))
                .Where(obj => obj != null)
                .ToList()
                .ForEach(obj => obj.SetActive(false));

            // focus position
            var focusPosition = interactive ? tutorial.Canvas.WorldToCanvasPosition(interactive.transform.position) : Vector2.zero;            

            // check mode had Top
            if (tut.focusMode.HasFlag(FocusMode.Top))
            {
                interactiveParent = interactive.transform.parent;
                interactiveOriginPosition = interactive.transform.localPosition;
                interactive.transform.SetParent(container, false);
                interactive.transform.SetAsFirstSibling();
                await UniTask.NextFrame();
            }

            // check mode has Hand
            if (tut.focusMode.HasFlag(FocusMode.Hand))
            {
                // focus hand
                hand.SetActive(true);
                hand.GetComponent<RectTransform>().anchoredPosition = focusPosition;
                Debug.Log(focusPosition);

                // play animation
                var anim = hand.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.enabled = true;
                    anim.Play(handClip);                    
                }
            }
            else
            {
                hand.SetActive(false);
            }

            // update touch zone
            touch.interactable = tut.focusMode.HasFlag(FocusMode.Touch) || tut.focusMode.HasFlag(FocusMode.Hand);
            if (touch.interactable)
            {
                touch.GetComponent<RectTransform>().anchoredPosition = tut.focusMode.HasFlag(FocusMode.Touch) ? Vector2.zero : focusPosition;
                var touchSize = new Vector2(0, 0);
                if (tut.focusMode.HasFlag(FocusMode.Touch) || interactive == null)
                {
                    touchSize = tutorial.Canvas.GetComponent<RectTransform>().sizeDelta;
                }
                else if (interactive != null && interactive.GetComponent<RectTransform>() != null)
                {
                    touchSize = interactive.GetComponent<RectTransform>().sizeDelta;
                }
                touch.GetComponent<RectTransform>().sizeDelta = touchSize;
            }

            // check mode has Mask
            if (tut.focusMode.HasFlag(FocusMode.Mask))
            {
                tutorial.mask.SetActive(true);
                tutorial.mask.GetComponent<RectTransform>().anchoredPosition = focusPosition;
            }
            else
            {
                tutorial.mask.SetActive(false);
            }

            // show message box
            if (string.IsNullOrEmpty(tut.message))
            {
                tutorial.messageBox.SetActive(false);
            }
            else
            {
                var boxPosition = tut.alignWithFocus ?
                    new Vector2(tut.messageBoxPosition.x, focusPosition.y + tut.messageBoxPosition.y) :
                    tut.messageBoxPosition;
                tutorial.ShowMessageBox(tut.message, messageClip, boxPosition);
            }

            // trigger event name
            if (!string.IsNullOrEmpty(tut.eventName))
            {
                tutorial.Model.eventName.Execute(tut.eventName);
            }

            if (touch.interactable)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f), ignoreTimeScale: true);
                touch.onClick.AddListener(OnTouch);
            }
        }

        public void DeactiveStep(int step)
        {
            if (step > 0)
            {
                // actives game objects
                steps[step].deactives
                    .Select(path => GameObject.Find(path))
                    .Where(obj => obj != null)
                    .ToList()
                    .ForEach(obj => obj.SetActive(true));

                // reset interactive object parent
                if (interactive && steps[step].focusMode.HasFlag(FocusMode.Top))
                {
                    interactive.transform.SetParent(interactiveParent, false);
                    interactive.transform.localPosition = interactiveOriginPosition;
                }
            }

            tutorial.mask.SetActive(false);
            tutorial.hand.GetComponent<Animator>().enabled = false;
            tutorial.hand.SetActive(false);
            tutorial.touch.onClick.RemoveAllListeners();
        }

        private void OnTouch()
        {
            // active button action
            if (!steps[currentStep].focusMode.HasFlag(FocusMode.Touch) && interactive != null)
            {
                var button = interactive.GetComponentInChildren<Button>();
                var toggle = interactive.GetComponentInChildren<Toggle>();
                if (button != null)
                    button.onClick.Invoke();
                else if (toggle != null)
                    toggle.isOn = true;
            }

            // next step
            NextStep();
        }

        private void OnCompleted()
        {
            DeactiveStep(currentStep - 1);
            tutorial.touch.onClick.RemoveAllListeners();
            tutorial.OnCompleted(this);
            tutorial.hand.SetActive(false);
        }
    }
}