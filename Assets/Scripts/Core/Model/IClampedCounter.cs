using System;

namespace Game.Core
{
    /// <summary>
    /// A number with a floor, a ceiling and two events. Health, power, lives and the fruit
    /// counter are all this - which is exactly why it is written once.
    ///
    /// It is deliberately domain-free: it does not know what a heart is, and it has no
    /// UnityEngine reference. That is what lets one implementation serve four features and
    /// still be unit-testable without opening the editor.
    ///
    /// Features derive their OWN interface from this (IPowerModel, IPlayerHealthModel).
    /// They add no members - the point is the NAME: a container has to tell "the power" and
    /// "the lives" apart even though their shape is identical, and a controller that asks
    /// for IPowerModel can never be handed the health by mistake.
    /// </summary>
    public interface IClampedCounter
    {
        int Current { get; }

        int Max { get; }

        bool IsFull { get; }

        /// <summary>Raised on every change of Current.</summary>
        event Action Changed;

        /// <summary>Raised once, on the transition down to zero.</summary>
        event Action Empty;

        /// <summary>
        /// Returns how much was ACTUALLY added, which is not always what was asked: a
        /// carrot worth two, eaten at 14 of 15, adds one. A bool could not say that.
        /// </summary>
        int Add(int amount);

        /// <summary>Returns how much was actually taken.</summary>
        int Remove(int amount);

        /// <summary>Back to a known value - a new life, or a new level.</summary>
        void Reset(int amount);
    }
}
