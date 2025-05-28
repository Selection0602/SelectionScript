using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TalkList", menuName = "TalkList")]

public class TalkList : ScriptableObject
{
    [SerializeField]
    private List<string> talks;
    public List<string> Talks { get { return talks; } set { talks = value; } }
}

