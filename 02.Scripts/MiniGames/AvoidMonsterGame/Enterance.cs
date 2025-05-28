using UnityEngine;

public class Enterance : MonoBehaviour
{
    public Direction Direction;             //입구 방향
    public GameObject Block;                //입구가 없을 때 막아주는 블럭
    public GameObject LockBlock;            //잠겨있을 때 막아주는 블럭

    public bool IsLocked = false;           //잠김 여부
    public int RequiredKeyItemId = -1;      //

    public bool IsExit = false;             //탈출구 여부
}
