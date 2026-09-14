/// <summary>
/// Something that can be switched on and off by the game rather than by the player: an enemy
/// that only moves once he is worth watching, a trap that only arms when someone is near.
///
/// It exists so that whatever does the switching - a range trigger, a cutscene, a boss fight -
/// never has to know WHAT it is switching on. ActivateNearPlayer asks for this interface and
/// works with a hopping snake, a ghost and anything written later, with no edit to itself
/// (Dependency Inversion, Open/Closed).
///
/// Deliberately not "enabled = false" on the component: a sleeping enemy usually has to do
/// something as it falls asleep - stop dead, hide, drop out of a chase - and only the enemy
/// itself knows what that is.
/// </summary>
public interface IActivatable
{
    /// <summary>Wake up and start behaving.</summary>
    void Activate();

    /// <summary>Go back to sleep. Whatever "still" means for this object, do it now.</summary>
    void Deactivate();
}
