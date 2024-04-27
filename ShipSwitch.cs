using System.Reflection;
using Debug = UnityEngine.Debug;

namespace ShipyardReBuild;

public class ShipSwitch : MonoBehaviour, Hoverable, Interactable
{
    static string modName = "ERRROR";
    const string KEY_lightOn = "lightOn";
    public string m_name = "";

    public GameObject m_enabledObject;

    public EffectList m_activateEffect = new EffectList();

    public EffectList m_deactivateEffect = new EffectList();

    public ZNetView m_nview;

    public Ship m_ship;

    private void Awake()
    {
        if (modName == "ERRROR") modName = Assembly.GetExecutingAssembly().GetName().Name;

        if (!m_nview) m_nview = GetComponent<ZNetView>();
        if (!m_nview.IsValid() || m_nview.GetZDO() == null) return;

        m_nview.Register<long>("ToggleShipLight", RPC_ToggleShipLight);
        // m_enabledObject?.SetActive(false); //if it should be invisible at start
    }

    private void Update() => UpdateVisual();

    private void UpdateVisual()
    {
        var active = IsEnabled();
        if (m_enabledObject) m_enabledObject.SetActive(active);
        else Debug.LogWarning($"[{modName}] No object to Toggle");
    }

    public string GetHoverText() =>
        Localization.instance.Localize(
            $"{m_name}\n[<color=yellow><b>$KEY_Use</b></color>] $bs_{(IsEnabled() ? "disable" : "enable")}");

    public string GetHoverName() => m_name;

    private void RPC_ToggleShipLight(long uid, long _) => ToggleShipLight();

    private void ToggleShipLight() => SetEnabled(!m_nview.GetZDO().GetBool(KEY_lightOn));

    public bool IsEnabled() => m_nview.GetZDO().GetBool(KEY_lightOn);

    private void SetEnabled(bool enabled)
    {
        m_nview.GetZDO().Set(KEY_lightOn, enabled);
        if (enabled) m_activateEffect.Create(transform.position, transform.rotation);
        else m_deactivateEffect.Create(transform.position, transform.rotation);
    }

    public bool Interact(Humanoid character, bool repeat, bool alt)
    {
        if (repeat || alt) return false;
        //	if (!PrivateArea.CheckAccess(transform.position)) return true;
        m_nview.InvokeRPC("ToggleShipLight");
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
}