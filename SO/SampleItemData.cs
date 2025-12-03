using UnityEngine;

namespace SheetToSO
{
    [CreateAssetMenu(menuName = "SheetToSO/Sample Item")]
    public class SampleItemData : ScriptableObject
    {
        public string id;
        public Sprite image;
        public string displayName;
        [TextArea]
        public string description;
    }
}