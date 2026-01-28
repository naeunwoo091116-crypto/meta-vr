using UnityEngine;

namespace AtomSim.Data
{
    [CreateAssetMenu(fileName = "NewAtomData", menuName = "Atoms/Atom Data", order = 1)]
    public class AtomData : ScriptableObject
    {
        [Header("Basic Properties")]
        public int atomicNumber;
        public string symbol;
        public string elementName;
        public float atomicMass;

        [Header("Periodic Table Position")]
        [Tooltip("Group (Column) 1-18")]
        [Range(1, 18)]
        public int group;

        [Tooltip("Period (Row) 1-7")]
        [Range(1, 7)]
        public int period;

        [Header("Visuals")]
        public Color elementColor = Color.white;
        public ElementCategory category = ElementCategory.Unknown;

        [TextArea]
        public string description;
    }
}
