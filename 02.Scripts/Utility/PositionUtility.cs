using UnityEngine;

public class PositionUtility
{
    /// <summary>
    /// 화면 중심을 UI 캔버스 좌표로 변환
    /// </summary>
    /// <param name="rect">UI의 RectTransform</param>
    /// <param name="pos"></param>
    /// <param name="cam"></param>
    /// <returns></returns>
    public static Vector2 ScreenCenterToUI(RectTransform rect, Camera cam)
    {
        // 화면 중심 좌표 계산
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        return ScreenToUILocalPos(rect,screenCenter,cam);
    }

    /// <summary>
    /// 스크린 좌표를 UI 좌표로 변환
    /// </summary>
    /// <param name="rect">UI의 RectTransform</param>
    /// <param name="pos"></param>
    /// <param name="cam"></param>
    /// <returns></returns>
    public static Vector2 ScreenToUILocalPos(RectTransform rect, Vector3 pos, Camera cam)
    {
        // Screen 좌표를 UI 캔버스 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect.parent as RectTransform, // 부모 RectTransform 기준으로 변환
            pos,                          
            cam,                            
            out Vector2 localPoint);               
        return localPoint;
    }
}
