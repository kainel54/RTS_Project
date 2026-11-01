using Code.Environments;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

namespace Code.Units.BT.Events
{

#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/Supply event channel")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "Supply event channel", message: "[Unit] gather [Amount] [Supplies]", category: "Events", id: "b0943b809ce9226b9a2aeff7c2a21e73")]
    public sealed partial class SupplyEventChannel : EventChannel<AbstractUnit, int, SupplySO> { }
}