using UnityEngine;
using UnityEngine.UIElements;

namespace Flui.Creator
{
    public class FluiCreatorCodeCreator : MonoBehaviour
    {
        [SerializeField] private bool _createCode;
        [SerializeField] private string _code;

        private void OnValidate()
        {
            if (_createCode)
            {
                _createCode = false;
                var document = GetComponent<UIDocument>();
                _code = FluiCreatorHelper.GenerateCreatorCode(document.rootVisualElement);
            }
        }
    }
}