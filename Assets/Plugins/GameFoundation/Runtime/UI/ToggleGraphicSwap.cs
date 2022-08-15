namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Toggle))]
    public class ToggleGraphicSwap : MonoBehaviour
    {
        private Toggle _toggle;

        private Toggle toggle => _toggle ?? (_toggle = GetComponent<Toggle>());

        private void Awake()
        {
            toggle.onValueChanged.AddListener(OnTargetToggleValueChanged);
        }

        private void OnEnable()
        {
            toggle.targetGraphic.enabled = !toggle.isOn;
        }

        public void OnTargetToggleValueChanged(bool on)
        {
            toggle.targetGraphic.enabled = !on;
        }
    }
}