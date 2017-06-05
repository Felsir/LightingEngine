using Microsoft.Xna.Framework;

namespace Monolights
{
    /// <summary>
    /// The light type. 
    /// A pointlight shines equally bright into all directions.
    /// A spotlight shines into a specified direction.
    /// </summary>
    public enum LightType
    {
        Point,
        Spot
    }

    public abstract class Light
    {
        protected Color _color;
        protected float _actualPower;


        /// <summary>
        /// The position of the light in the scene.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The Power is the Initial Power of the Light
        /// </summary>
        public float Power
        {
            get { return _actualPower; }
            set
            {
                _actualPower = value;
            }
        }

        /// <summary>
        /// The color of the light.
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                Color4 = _color.ToVector4();
            }
        }

        /// <summary>
        /// The color of the light as a Vector4 value.
        /// </summary>
        public Vector4 Color4 { get; private set; }


        /// <summary>
        /// The distance over which the light decays to zero. 
        /// </summary>
        public float LightDecay { get; set; }

        /// <summary>
        /// The type of light: point or spotlight
        /// </summary>
        public LightType LightType { get; private set; }

        /// <summary>
        /// Constructor; a light of type LightType.
        /// </summary>
        /// <param name="lightType"></param>
        protected Light(LightType lightType)
        {
            LightType = lightType;
        }

        /// <summary>
        /// Set the enabled value of the light.
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}