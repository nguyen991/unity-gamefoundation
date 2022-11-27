using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using GameFoundation.Utilities;

namespace GameFoundation.Tutorial
{
    [System.Flags]
    public enum FocusMode
    {
        None = 0,
        Hand = 1 << 1,
        Top = 1 << 2,
        Mask = 1 << 3,
    }

    [System.Serializable]
    public class TutorialStep
    {
        public string eventName;
        public string focus;
        public FocusMode focusMode;
        public AnimationClip handClip = null;
        public List<string> deactives = new List<string>();
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
        private List<GameObject> deactiveObjects;
        private int currentStep = 0;

        public void Active(TutorialManager tutorial)
        {
            this.tutorial = tutorial;
            interactive = null;
            currentStep = -1;
            tutorial.touch.onClick.AddListener(OnTouch);
            NextStep();
        }

        public void NextStep()
        {
            currentStep++;

            if (currentStep > 1 && !string.IsNullOrEmpty(steps[currentStep - 1].eventName))
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
            var handClip = tut.handClip != null ? tut.handClip : tutorial.handClip;
            var touch = tutorial.touch;

            if (step > 0)
            {
                DeactiveStep(step - 1);
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
            deactiveObjects = steps[step].deactives
                .Select(path => GameObject.Find(path))
                .Where(obj => obj != null)
                .ToList();
            deactiveObjects.ForEach(obj => obj.SetActive(false));

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
                if (tut.focusMode.HasFlag(FocusMode.Top))
                {
                    hand.transform.position = interactive.transform.position;
                }
                else
                {
                    hand.GetComponent<RectTransform>().anchoredPosition = tutorial.Canvas.WorldToCanvasPosition(interactive.transform.position);
                }

                // play animation
                var anim = hand.GetComponent<Animation>();
                if (anim == null)
                {
                    anim = hand.AddComponent<Animation>();
                }
                anim.clip = handClip;
                anim.AddClip(handClip, handClip.name);
                anim.Play(handClip.name);

                // update touch zone
                var rect = interactive.GetComponent<RectTransform>();
                if (rect)
                {
                    touch.GetComponent<RectTransform>().sizeDelta = rect.sizeDelta;
                    touch.GetComponent<RectTransform>().anchoredPosition = tutorial.Canvas.WorldToCanvasPosition(interactive.transform.position);
                }
                else
                {
                    touch.transform.position = interactive.transform.position;
                }
            }
            else
            {
                hand.GetComponent<Animation>()?.Stop();
                hand.SetActive(false);
            }

            // check mode has Mask
            if (tut.focusMode.HasFlag(FocusMode.Mask))
            {
                tutorial.mask.transform.position = hand.transform.position;
                tutorial.mask.SetActive(true);
            }
            else
            {
                tutorial.mask.SetActive(false);
            }

            // trigger event name
            if (!string.IsNullOrEmpty(tut.eventName))
            {
                tutorial.Model.eventName.Execute(tut.eventName);
            }
        }

        public void DeactiveStep(int step)
        {
            // actives game objects
            deactiveObjects.ForEach(obj => obj.SetActive(true));
            deactiveObjects.Clear();

            // reset interactive object parent
            if (interactive && steps[step].focusMode.HasFlag(FocusMode.Top))
            {
                interactive.transform.SetParent(interactiveParent, false);
                interactive.transform.localPosition = interactiveOriginPosition;
            }
        }

        private void OnTouch()
        {
            // active button action
            interactive?.GetComponentInChildren<Button>()?.onClick.Invoke();

            // next step
            NextStep();
        }

        private void OnCompleted()
        {
            DeactiveStep(currentStep - 1);
            tutorial.touch.onClick.RemoveListener(OnTouch);
            tutorial.OnCompleted(this);
        }
    }
}