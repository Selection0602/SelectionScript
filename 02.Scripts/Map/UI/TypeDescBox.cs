using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TypeDescBox : MonoBehaviour
{
    [SerializeField] private Image _nodeImage;
    [SerializeField] private TextMeshProUGUI _nodeName;
    
    public void SetData(NodeTypeData data)
    {
        _nodeImage.sprite = data.NodeIcon;
        _nodeImage.color = data.NodeColor;
        _nodeName.text = data.TypeName;
    }
}
