using UnityEngine;

namespace Blip
{
    /**
     * A BlipControl is named variable that can be tied to one or more aspects of a BlipEvent such
     * as a modifier (volume, pitch) or action. At runtime, a BlipControl can be modified which
     * then modifies the associated events either locally or globally.
     */ 
    [CreateAssetMenu(fileName = "BlipControl", menuName = "BlipKit/Control", order = 0)]
    public class BlipControl : ScriptableObject
    {
        public enum ControlType
        {
            Integer,
            Float
        }

        public string Name = "Control";
        public ControlType Type = ControlType.Integer;

        // Only one of these are ever visible the inspector, based on the control type.
        public int DefaultValueInt;
        public float DefaultValueFloat;

        // These are the runtime values. At start they are initialized to their default values.
        private int ValueInt;
        private float ValueFloat;

        BlipControl()
        {
            ValueInt = DefaultValueInt;
            ValueFloat = DefaultValueFloat;
        }

        public int GetValueInt()
        {
            if (Type == ControlType.Float)
            {
                return (int)ValueFloat;
            }

            return ValueInt;
        }

        public float GetValueFloat()
        {
            if (Type == ControlType.Integer)
            {
                return (float)ValueInt;
            }

            return ValueFloat;
        }
    }
}