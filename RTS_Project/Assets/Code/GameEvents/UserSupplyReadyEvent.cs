using Code.CoreSystem;
using Code.Players;
using UnityEngine;

namespace Code.GameEvents
{
    public struct UserSupplyReadyEvent : IEvent
    {
        public UserSupplies UserSupplies {  get;}

        public UserSupplyReadyEvent(UserSupplies userSupplies)
        {
            UserSupplies = userSupplies;
        }
    }
}