using UnityEngine;
using System;
using UnityEngine.Events;

namespace mods.Internal.MediaServiceAPI
{
[Serializable]
public class ColorChangedEvent : UnityEvent<Color>
{

}
}