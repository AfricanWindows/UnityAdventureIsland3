using System;

namespace Game.Weapons
{
    /// <summary>
    /// Counts rounds, and does nothing else.
    ///
    /// It was carved out of AxeWeapon, which used to be three things at once: a weapon,
    /// an ammo store, and the ICounter the HUD reads. Three reasons to change in one class
    /// is precisely what Single Responsibility forbids - re-balancing the throw arc meant
    /// editing the same file the UI depends on.
    ///
    /// It is a plain C# class with no UnityEngine reference: a number with rules is not
    /// something that needs a GameObject. That also makes it directly unit-testable, and
    /// reusable by any future weapon that needs limited ammo, without inheriting anything.
    ///
    /// It implements the project's existing ICounter, so UI_CounterView displays it with
    /// no change at all (Open/Closed).
    /// </summary>
    public class AmmoMagazine : ICounter
    {
        private int _rounds;

        /// <summary>What the HUD shows.</summary>
        public int Value { get { return _rounds; } }

        /// <summary>Raised on every change, which is how the label stays in step.</summary>
        public event Action<int> OnValueChanged;

        public bool HasRounds { get { return _rounds > 0; } }

        public AmmoMagazine(int startRounds)
        {
            _rounds = startRounds > 0 ? startRounds : 0;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            _rounds += amount;
            Raise();
        }

        /// <summary>
        /// Takes rounds out, or refuses. Returning a bool - rather than letting the caller
        /// check and then subtract - keeps "can I?" and "do it" as one indivisible step,
        /// so the count can never go negative through a missed check.
        /// </summary>
        public bool TrySpend(int amount)
        {
            if (amount <= 0 || _rounds < amount)
                return false;

            _rounds -= amount;
            Raise();
            return true;
        }

        /// <summary>Fires the change event to whoever is listening right now.</summary>
        public void Raise()
        {
            if (OnValueChanged != null)
                OnValueChanged(_rounds);
        }
    }
}
