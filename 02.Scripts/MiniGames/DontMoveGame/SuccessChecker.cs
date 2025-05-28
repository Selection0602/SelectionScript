using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuccessChecker : MonoBehaviour
{
    [Header("성공할 시에 채워질 스택")]
    [SerializeField] private List<Image> _successStackList = new List<Image> ();

    //성공시 색상 변경
    public void CheckSuccess(int count)
    {
        _successStackList[count - 1].color = Color.white;
    }
}
