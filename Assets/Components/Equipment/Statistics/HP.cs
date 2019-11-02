using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public class HP : Statistic
    {
        #region Fields
        [SerializeField] int max;
        [SerializeField] int current;
        #endregion
        #region Events
        public delegate void HPEvent(int previous, int current);
        public HPEvent OnChange;
        public HPEvent OnDamage;
        public HPEvent OnHeal;
        public HPEvent OnDie;
        #endregion

        public virtual bool Damage(int amount)
        {
            if (current > 0)
            {
                int previous = current;
                current -= amount;
                OnChange?.Invoke(previous, current);
                OnDamage?.Invoke(previous, current);
                if (current <= 0)
                {
                    current = 0;
                    OnDie?.Invoke(previous, current);
                }
                return true;
            }
            return false;
        }
        public virtual bool Heal(int amount)
        {
            if (current < max)
            {
                int previous = current;
                current += amount;
                OnChange?.Invoke(previous, current);
                OnHeal?.Invoke(previous, current);
                if (current > max)
                {
                    current = max;
                }
                return true;
            }
            return false;
        }
    }

}