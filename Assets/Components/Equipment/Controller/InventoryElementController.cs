using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment {
    public interface IDroppable
    {
        bool Drop();
    }
    
    public interface IUsable
    {
        bool Use();
    }
    public abstract class InventoryElementController<T> : GameElement where T : InventoryData
    {
        public enum InventoryElementType
        {
            Weapon_primary, Weapon_secondary, Passive 
        }
        public InventoryElementType elementType;
        public T data;
    }
}