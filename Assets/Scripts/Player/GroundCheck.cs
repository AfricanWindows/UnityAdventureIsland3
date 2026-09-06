using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tells whether we are standing on ANY solid object - floor tile, spikes, crate, enemy -
/// by reading the physics contact normals instead of expecting a special component
/// (like SC_Floor) on the other side.
///
/// It also reports whether that ground can carry us (a moving platform), because it is
/// the only class that already knows what is under our feet. Answering that question
/// twice, in two different ways, is how the two answers start to disagree.
/// </summary>
public class GroundCheck : MonoBehaviour, IGroundCheck, IPlatformProvider
{
    [Tooltip("How flat a surface must be to count as ground. 1 = perfectly flat, 0 = vertical wall.")]
    [SerializeField] private float minGroundNormal = 0.5f;

    private readonly List<Collider2D> groundContacts = new List<Collider2D>();

    // Looked up once per contact, so no GetComponent runs during FixedUpdate.
    private readonly List<IRideablePlatform> contactPlatforms = new List<IRideablePlatform>();

    public bool IsGrounded
    {
        get
        {
            DropDeadContacts();
            return groundContacts.Count > 0;
        }
    }

    /// <summary>The moving floor we are standing on, or null for normal ground.</summary>
    public IRideablePlatform CurrentPlatform
    {
        get
        {
            DropDeadContacts();

            for (int i = 0; i < contactPlatforms.Count; i++)
            {
                if (contactPlatforms[i] != null)
                    return contactPlatforms[i];
            }

            return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        UpdateContact(col);
    }

    /// <summary>
    /// Also refreshed on stay, so sliding off an edge is noticed.
    /// A sleeping Rigidbody2D stops sending Stay - that is fine, the contact
    /// stored on Enter simply remains until Exit.
    /// </summary>
    private void OnCollisionStay2D(Collision2D col)
    {
        UpdateContact(col);
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        RemoveContact(col.collider);
    }

    private void OnDisable()
    {
        groundContacts.Clear();
        contactPlatforms.Clear();
    }

    private void UpdateContact(Collision2D col)
    {
        if (IsStandingOn(col))
            AddContact(col.collider);
        else
            RemoveContact(col.collider);
    }

    private void AddContact(Collider2D collider)
    {
        if (groundContacts.Contains(collider))
            return;

        groundContacts.Add(collider);
        contactPlatforms.Add(collider.GetComponent<IRideablePlatform>());
    }

    private void RemoveContact(Collider2D collider)
    {
        int index = groundContacts.IndexOf(collider);
        if (index < 0)
            return;

        groundContacts.RemoveAt(index);
        contactPlatforms.RemoveAt(index);
    }

    /// <summary>Drops contacts whose object was destroyed or disabled meanwhile.</summary>
    private void DropDeadContacts()
    {
        for (int i = groundContacts.Count - 1; i >= 0; i--)
        {
            if (groundContacts[i] == null || !groundContacts[i].gameObject.activeInHierarchy)
            {
                groundContacts.RemoveAt(i);
                contactPlatforms.RemoveAt(i);
            }
        }
    }

    private bool IsStandingOn(Collision2D col)
    {
        for (int i = 0; i < col.contactCount; i++)
        {
            // The normal points from the other object towards us:
            // pointing up means that object is under our feet.
            if (col.GetContact(i).normal.y >= minGroundNormal)
                return true;
        }

        return false;
    }
}
