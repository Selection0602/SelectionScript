public class MapManager
{
    private MapData _savedMapData;
    public MapData SavedMapData
    {
        get => _savedMapData;
        set
        {
            _savedMapData = value;
            if (value != null)
            {
                Manager.Instance.SaveManager.SaveMapData(); // 맵 데이터가 변경될 때마다 저장
            }
        }
    }

    public bool canSkip = false;

    public void ClearData()
    {
        SavedMapData = null;
        canSkip = false;
    }
}
