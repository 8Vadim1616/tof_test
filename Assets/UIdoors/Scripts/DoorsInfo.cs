using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Door Position info", menuName = "Create Door Position info")]
public class DoorsInfo : ScriptableObject
{
    public List<RectTransform> DoorsPositions;
}