using UnityEngine;
using System.Collections.Generic;

namespace AtomSim.Data
{
    /// <summary>
    /// Contains complete data for all 118 elements of the periodic table.
    /// Use PeriodicTableData.GetAllElements() to retrieve element data.
    /// </summary>
    public static class PeriodicTableData
    {
        public struct ElementInfo
        {
            public int atomicNumber;
            public string symbol;
            public string elementName;
            public float atomicMass;
            public int group;
            public int period;
            public ElementCategory category;
            public Color color;

            public ElementInfo(int atomicNumber, string symbol, string elementName, float atomicMass,
                int group, int period, ElementCategory category, Color color)
            {
                this.atomicNumber = atomicNumber;
                this.symbol = symbol;
                this.elementName = elementName;
                this.atomicMass = atomicMass;
                this.group = group;
                this.period = period;
                this.category = category;
                this.color = color;
            }
        }

        // Category colors
        private static readonly Color AlkaliMetalColor = new Color(1f, 0.4f, 0.4f);           // Red
        private static readonly Color AlkalineEarthColor = new Color(1f, 0.6f, 0.3f);         // Orange
        private static readonly Color TransitionMetalColor = new Color(1f, 0.8f, 0.4f);       // Yellow
        private static readonly Color PostTransitionColor = new Color(0.6f, 0.8f, 0.6f);      // Light green
        private static readonly Color MetalloidColor = new Color(0.4f, 0.8f, 0.8f);           // Teal
        private static readonly Color NonmetalColor = new Color(0.4f, 0.6f, 1f);              // Blue
        private static readonly Color HalogenColor = new Color(0.8f, 0.4f, 1f);               // Purple
        private static readonly Color NobleGasColor = new Color(0.9f, 0.5f, 0.9f);            // Pink
        private static readonly Color LanthanideColor = new Color(0.6f, 0.9f, 0.6f);          // Green
        private static readonly Color ActinideColor = new Color(0.9f, 0.6f, 0.7f);            // Rose
        private static readonly Color UnknownColor = new Color(0.7f, 0.7f, 0.7f);             // Gray

        private static List<ElementInfo> allElements;

        public static List<ElementInfo> GetAllElements()
        {
            if (allElements == null)
            {
                InitializeElements();
            }
            return allElements;
        }

        private static void InitializeElements()
        {
            allElements = new List<ElementInfo>
            {
                // Period 1
                new ElementInfo(1, "H", "Hydrogen", 1.008f, 1, 1, ElementCategory.Nonmetal, NonmetalColor),
                new ElementInfo(2, "He", "Helium", 4.003f, 18, 1, ElementCategory.NobleGas, NobleGasColor),

                // Period 2
                new ElementInfo(3, "Li", "Lithium", 6.941f, 1, 2, ElementCategory.AlkaliMetal, AlkaliMetalColor),
                new ElementInfo(4, "Be", "Beryllium", 9.012f, 2, 2, ElementCategory.AlkalineEarthMetal, AlkalineEarthColor),
                new ElementInfo(5, "B", "Boron", 10.81f, 13, 2, ElementCategory.Metalloid, MetalloidColor),
                new ElementInfo(6, "C", "Carbon", 12.01f, 14, 2, ElementCategory.Nonmetal, NonmetalColor),
                new ElementInfo(7, "N", "Nitrogen", 14.01f, 15, 2, ElementCategory.Nonmetal, NonmetalColor),
                new ElementInfo(8, "O", "Oxygen", 16.00f, 16, 2, ElementCategory.Nonmetal, NonmetalColor),
                new ElementInfo(9, "F", "Fluorine", 19.00f, 17, 2, ElementCategory.Halogen, HalogenColor),
                new ElementInfo(10, "Ne", "Neon", 20.18f, 18, 2, ElementCategory.NobleGas, NobleGasColor),

                // Period 3
                new ElementInfo(11, "Na", "Sodium", 22.99f, 1, 3, ElementCategory.AlkaliMetal, AlkaliMetalColor),
                new ElementInfo(12, "Mg", "Magnesium", 24.31f, 2, 3, ElementCategory.AlkalineEarthMetal, AlkalineEarthColor),
                new ElementInfo(13, "Al", "Aluminum", 26.98f, 13, 3, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(14, "Si", "Silicon", 28.09f, 14, 3, ElementCategory.Metalloid, MetalloidColor),
                new ElementInfo(15, "P", "Phosphorus", 30.97f, 15, 3, ElementCategory.Nonmetal, NonmetalColor),
                new ElementInfo(16, "S", "Sulfur", 32.07f, 16, 3, ElementCategory.Nonmetal, NonmetalColor),
                new ElementInfo(17, "Cl", "Chlorine", 35.45f, 17, 3, ElementCategory.Halogen, HalogenColor),
                new ElementInfo(18, "Ar", "Argon", 39.95f, 18, 3, ElementCategory.NobleGas, NobleGasColor),

                // Period 4
                new ElementInfo(19, "K", "Potassium", 39.10f, 1, 4, ElementCategory.AlkaliMetal, AlkaliMetalColor),
                new ElementInfo(20, "Ca", "Calcium", 40.08f, 2, 4, ElementCategory.AlkalineEarthMetal, AlkalineEarthColor),
                new ElementInfo(21, "Sc", "Scandium", 44.96f, 3, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(22, "Ti", "Titanium", 47.87f, 4, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(23, "V", "Vanadium", 50.94f, 5, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(24, "Cr", "Chromium", 52.00f, 6, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(25, "Mn", "Manganese", 54.94f, 7, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(26, "Fe", "Iron", 55.85f, 8, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(27, "Co", "Cobalt", 58.93f, 9, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(28, "Ni", "Nickel", 58.69f, 10, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(29, "Cu", "Copper", 63.55f, 11, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(30, "Zn", "Zinc", 65.38f, 12, 4, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(31, "Ga", "Gallium", 69.72f, 13, 4, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(32, "Ge", "Germanium", 72.63f, 14, 4, ElementCategory.Metalloid, MetalloidColor),
                new ElementInfo(33, "As", "Arsenic", 74.92f, 15, 4, ElementCategory.Metalloid, MetalloidColor),
                new ElementInfo(34, "Se", "Selenium", 78.97f, 16, 4, ElementCategory.Nonmetal, NonmetalColor),
                new ElementInfo(35, "Br", "Bromine", 79.90f, 17, 4, ElementCategory.Halogen, HalogenColor),
                new ElementInfo(36, "Kr", "Krypton", 83.80f, 18, 4, ElementCategory.NobleGas, NobleGasColor),

                // Period 5
                new ElementInfo(37, "Rb", "Rubidium", 85.47f, 1, 5, ElementCategory.AlkaliMetal, AlkaliMetalColor),
                new ElementInfo(38, "Sr", "Strontium", 87.62f, 2, 5, ElementCategory.AlkalineEarthMetal, AlkalineEarthColor),
                new ElementInfo(39, "Y", "Yttrium", 88.91f, 3, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(40, "Zr", "Zirconium", 91.22f, 4, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(41, "Nb", "Niobium", 92.91f, 5, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(42, "Mo", "Molybdenum", 95.95f, 6, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(43, "Tc", "Technetium", 98.00f, 7, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(44, "Ru", "Ruthenium", 101.1f, 8, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(45, "Rh", "Rhodium", 102.9f, 9, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(46, "Pd", "Palladium", 106.4f, 10, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(47, "Ag", "Silver", 107.9f, 11, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(48, "Cd", "Cadmium", 112.4f, 12, 5, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(49, "In", "Indium", 114.8f, 13, 5, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(50, "Sn", "Tin", 118.7f, 14, 5, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(51, "Sb", "Antimony", 121.8f, 15, 5, ElementCategory.Metalloid, MetalloidColor),
                new ElementInfo(52, "Te", "Tellurium", 127.6f, 16, 5, ElementCategory.Metalloid, MetalloidColor),
                new ElementInfo(53, "I", "Iodine", 126.9f, 17, 5, ElementCategory.Halogen, HalogenColor),
                new ElementInfo(54, "Xe", "Xenon", 131.3f, 18, 5, ElementCategory.NobleGas, NobleGasColor),

                // Period 6
                new ElementInfo(55, "Cs", "Cesium", 132.9f, 1, 6, ElementCategory.AlkaliMetal, AlkaliMetalColor),
                new ElementInfo(56, "Ba", "Barium", 137.3f, 2, 6, ElementCategory.AlkalineEarthMetal, AlkalineEarthColor),
                // Lanthanides (57-71) - placed in group 3 for standard position
                new ElementInfo(57, "La", "Lanthanum", 138.9f, 3, 6, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(58, "Ce", "Cerium", 140.1f, 4, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(59, "Pr", "Praseodymium", 140.9f, 5, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(60, "Nd", "Neodymium", 144.2f, 6, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(61, "Pm", "Promethium", 145.0f, 7, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(62, "Sm", "Samarium", 150.4f, 8, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(63, "Eu", "Europium", 152.0f, 9, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(64, "Gd", "Gadolinium", 157.3f, 10, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(65, "Tb", "Terbium", 158.9f, 11, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(66, "Dy", "Dysprosium", 162.5f, 12, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(67, "Ho", "Holmium", 164.9f, 13, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(68, "Er", "Erbium", 167.3f, 14, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(69, "Tm", "Thulium", 168.9f, 15, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(70, "Yb", "Ytterbium", 173.0f, 16, 9, ElementCategory.Lanthanide, LanthanideColor),
                new ElementInfo(71, "Lu", "Lutetium", 175.0f, 17, 9, ElementCategory.Lanthanide, LanthanideColor),
                // Continue Period 6
                new ElementInfo(72, "Hf", "Hafnium", 178.5f, 4, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(73, "Ta", "Tantalum", 180.9f, 5, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(74, "W", "Tungsten", 183.8f, 6, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(75, "Re", "Rhenium", 186.2f, 7, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(76, "Os", "Osmium", 190.2f, 8, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(77, "Ir", "Iridium", 192.2f, 9, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(78, "Pt", "Platinum", 195.1f, 10, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(79, "Au", "Gold", 197.0f, 11, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(80, "Hg", "Mercury", 200.6f, 12, 6, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(81, "Tl", "Thallium", 204.4f, 13, 6, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(82, "Pb", "Lead", 207.2f, 14, 6, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(83, "Bi", "Bismuth", 209.0f, 15, 6, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(84, "Po", "Polonium", 209.0f, 16, 6, ElementCategory.Metalloid, MetalloidColor),
                new ElementInfo(85, "At", "Astatine", 210.0f, 17, 6, ElementCategory.Halogen, HalogenColor),
                new ElementInfo(86, "Rn", "Radon", 222.0f, 18, 6, ElementCategory.NobleGas, NobleGasColor),

                // Period 7
                new ElementInfo(87, "Fr", "Francium", 223.0f, 1, 7, ElementCategory.AlkaliMetal, AlkaliMetalColor),
                new ElementInfo(88, "Ra", "Radium", 226.0f, 2, 7, ElementCategory.AlkalineEarthMetal, AlkalineEarthColor),
                // Actinides (89-103) - placed in group 3 for standard position
                new ElementInfo(89, "Ac", "Actinium", 227.0f, 3, 7, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(90, "Th", "Thorium", 232.0f, 4, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(91, "Pa", "Protactinium", 231.0f, 5, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(92, "U", "Uranium", 238.0f, 6, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(93, "Np", "Neptunium", 237.0f, 7, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(94, "Pu", "Plutonium", 244.0f, 8, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(95, "Am", "Americium", 243.0f, 9, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(96, "Cm", "Curium", 247.0f, 10, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(97, "Bk", "Berkelium", 247.0f, 11, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(98, "Cf", "Californium", 251.0f, 12, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(99, "Es", "Einsteinium", 252.0f, 13, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(100, "Fm", "Fermium", 257.0f, 14, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(101, "Md", "Mendelevium", 258.0f, 15, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(102, "No", "Nobelium", 259.0f, 16, 10, ElementCategory.Actinide, ActinideColor),
                new ElementInfo(103, "Lr", "Lawrencium", 262.0f, 17, 10, ElementCategory.Actinide, ActinideColor),
                // Continue Period 7
                new ElementInfo(104, "Rf", "Rutherfordium", 267.0f, 4, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(105, "Db", "Dubnium", 268.0f, 5, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(106, "Sg", "Seaborgium", 269.0f, 6, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(107, "Bh", "Bohrium", 270.0f, 7, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(108, "Hs", "Hassium", 277.0f, 8, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(109, "Mt", "Meitnerium", 278.0f, 9, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(110, "Ds", "Darmstadtium", 281.0f, 10, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(111, "Rg", "Roentgenium", 282.0f, 11, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(112, "Cn", "Copernicium", 285.0f, 12, 7, ElementCategory.TransitionMetal, TransitionMetalColor),
                new ElementInfo(113, "Nh", "Nihonium", 286.0f, 13, 7, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(114, "Fl", "Flerovium", 289.0f, 14, 7, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(115, "Mc", "Moscovium", 290.0f, 15, 7, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(116, "Lv", "Livermorium", 293.0f, 16, 7, ElementCategory.PostTransitionMetal, PostTransitionColor),
                new ElementInfo(117, "Ts", "Tennessine", 294.0f, 17, 7, ElementCategory.Halogen, HalogenColor),
                new ElementInfo(118, "Og", "Oganesson", 294.0f, 18, 7, ElementCategory.NobleGas, NobleGasColor),
            };
        }

        public static ElementInfo? GetElementByAtomicNumber(int atomicNumber)
        {
            var elements = GetAllElements();
            foreach (var element in elements)
            {
                if (element.atomicNumber == atomicNumber)
                    return element;
            }
            return null;
        }

        public static ElementInfo? GetElementBySymbol(string symbol)
        {
            var elements = GetAllElements();
            foreach (var element in elements)
            {
                if (element.symbol.Equals(symbol, System.StringComparison.OrdinalIgnoreCase))
                    return element;
            }
            return null;
        }

        /// <summary>
        /// Creates a runtime AtomData ScriptableObject from ElementInfo.
        /// </summary>
        public static AtomData CreateAtomData(ElementInfo info)
        {
            var atomData = ScriptableObject.CreateInstance<AtomData>();
            atomData.atomicNumber = info.atomicNumber;
            atomData.symbol = info.symbol;
            atomData.elementName = info.elementName;
            atomData.atomicMass = info.atomicMass;
            atomData.group = info.group;
            atomData.period = info.period;
            atomData.category = info.category;
            atomData.elementColor = info.color;
            atomData.description = $"{info.elementName} ({info.symbol}) - Atomic Number: {info.atomicNumber}";
            return atomData;
        }
    }
}
