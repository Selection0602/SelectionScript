using Unity.VisualScripting;

public class Manager : Singleton<Manager>
{
    public SoundManager SoundManager;
    public DataManager DataManager = new();
    public MapManager MapManager = new();
    public EffectManager EffectManager;
    public UIManager UIManager = new();
    public AddressableManager AddressableManager = new();
    public CursorManager CursorManager;
    public AnalyticsManager AnalyticsManager;
    public SaveManager SaveManager;
    public CreditManager CreditManager = new();

    protected override void Awake()
    {
        base.Awake();
        EffectManager = transform.AddComponent<EffectManager>();
        SoundManager = transform.AddComponent<SoundManager>();
        CursorManager = transform.AddComponent<CursorManager>();
        AnalyticsManager = transform.AddComponent<AnalyticsManager>();
        SaveManager = transform.AddComponent<SaveManager>();    }
}
