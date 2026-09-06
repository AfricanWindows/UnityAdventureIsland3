using System;

namespace Game.Core
{
    /// <summary>
    /// The one implementation of a clamped counter, and the only place these three rules
    /// are written:
    ///   - never above Max
    ///   - never below zero
    ///   - reaching zero is announced exactly ONCE, not on every further call
    ///
    /// Before this class existed, PlayerHealthModel and PowerModel each carried their own
    /// copy of it, and the lives counter and the fruit counter from the final assignment
    /// were about to become copies three and four. The rule that "20 fruit = a life" must
    /// live in a model, but it does not need its own arithmetic.
    ///
    /// No UnityEngine reference on purpose: this is data and rules, not a GameObject.
    ///
    /// It is not sealed and not abstract. A feature subclasses it to get a name
    /// (PowerModel), and can override nothing at all - which is the point.
    /// </summary>
    public class ClampedCounterModel : IClampedCounter
    {
        private readonly int max;
        private int current;

        // Empty is an EDGE, not a state. Without this flag a bar sitting at zero would
        // fire the event on every drain tick, and the player would lose a life every three
        // seconds while the death animation was still playing.
        private bool emptyAnnounced;

        public ClampedCounterModel(int max, int start)
        {
            this.max = max < 1 ? 1 : max;
            current = Clamp(start);
            emptyAnnounced = current <= 0;
        }

        public int Current { get { return current; } }

        public int Max { get { return max; } }

        public bool IsFull { get { return current >= max; } }

        public event Action Changed;

        public event Action Empty;

        public int Add(int amount)
        {
            if (amount <= 0 || IsFull)
                return 0;

            int before = current;
            Set(current + amount);

            return current - before;
        }

        public int Remove(int amount)
        {
            if (amount <= 0 || current <= 0)
                return 0;

            int before = current;
            Set(current - amount);

            if (current <= 0 && !emptyAnnounced)
            {
                emptyAnnounced = true;

                if (Empty != null)
                    Empty();
            }

            return before - current;
        }

        public void Reset(int amount)
        {
            Set(amount);
            emptyAnnounced = current <= 0;
        }

        private void Set(int value)
        {
            int clamped = Clamp(value);
            if (clamped == current)
                return;

            current = clamped;

            if (Changed != null)
                Changed();
        }

        private int Clamp(int value)
        {
            if (value < 0)
                return 0;

            return value > max ? max : value;
        }
    }
}
