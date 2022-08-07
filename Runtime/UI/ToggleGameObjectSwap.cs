namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Toggle))]
    public class ToggleGameObjectSwap : MonoBehaviour
    {
        public GameObject active;
        public GameObject deactive;

        private Toggle _toggle;

        private Toggle toggle => _toggle ?? (_toggle = GetComponent<Toggle>());

        private void Awake()
        {
            toggle.onValueChanged.AddListener(OnTargetToggleValueChanged);
        }

        private void OnEnable()
        {
            active?.SetActive(toggle.isOn);
            deactive?.SetActive(!toggle.isOn);
        }

        private void OnTargetToggleValueChanged(bool on)
        {
            active?.SetActive(on);
            deactive?.SetActive(!on);
        }
    }
}