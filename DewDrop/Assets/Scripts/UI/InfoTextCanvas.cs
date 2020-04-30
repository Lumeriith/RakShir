using Sirenix.OdinInspector;
using UnityEngine;

public class InfoTextCanvas : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Required References")]
    private InfoText _infoTextPrefab;

    private void Start()
    {
        GameManager.instance.OnActivatableInstantiate += (Activatable activatable) =>
        {
            if (activatable is IInfoTextable target)
            {
                InfoText text = Instantiate(_infoTextPrefab, transform).GetComponent<InfoText>();
                text.Setup(target);
            }
        };
    }
}
