/// <summary>
/// An enemy that can be put to sleep and woken up again - everything except the static one,
/// which has no behaviour to stop.
///
/// It exists because four enemies carried the same three members: a flag, Activate and
/// Deactivate. That is the sort of copy that quietly drifts apart - one of them forgets to
/// reset its timer on waking, and only that enemy misbehaves (Don't Repeat Yourself).
///
/// Activate and Deactivate are NOT virtual: whatever a subclass does, the flag must end up
/// right, so the flag is set here and the subclass fills in the hooks (Template Method). The
/// snake and the frog use OnActivated to start their pause over, the shooting snake to start
/// its countdown over; ActivateNearPlayer, which calls this, knows none of that - it only
/// sees IActivatable (Dependency Inversion).
/// </summary>
public abstract class ActivatableEnemy : BaseEnemy, IActivatable
{
    // Awake by default, so an enemy with no ActivateNearPlayer next to it simply behaves.
    private bool active = true;

    /// <summary>True while the enemy is allowed to act. Subclasses check it in FixedUpdate.</summary>
    protected bool IsActive { get { return active; } }

    /// <summary>The player came into range.</summary>
    public void Activate()
    {
        active = true;
        OnActivated();
    }

    /// <summary>The player left. What is already under way is allowed to finish.</summary>
    public void Deactivate()
    {
        active = false;
        OnDeactivated();
    }

    /// <summary>Waking up: a subclass may start its own clock over here.</summary>
    protected virtual void OnActivated() { }

    /// <summary>Falling asleep.</summary>
    protected virtual void OnDeactivated() { }
}
