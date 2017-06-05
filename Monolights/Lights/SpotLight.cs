using System;
using Microsoft.Xna.Framework;

namespace Monolights
{
    public class SpotLight : Light
    {
        private float _spotRotation;

        /// <summary>
        /// Gets or sets the spot beam exponent. 
        /// 0 means the spot is 180 degrees wide. The higher value means the beam gets more narrow, but still looks like a beam (not a 'lazorbeam').
        /// 6-9 gives nice results.
        /// </summary>
        /// <value>The spot beamwidth exponent.</value>
        public float SpotBeamWidthExponent { get; set; }

        /// <summary>
        /// Sets the angle of the beam.
        /// </summary>
        public float SpotRotation
        {
            get { return _spotRotation; }
            set
            {
                _spotRotation = value;
                Direction = new Vector3(
                    (float)Math.Cos(_spotRotation),
                    (float)Math.Sin(_spotRotation),
                    Direction.Z);
            }
        }

        /// <summary>
        /// The direction vector based on the angle of the spot.
        /// </summary>
        public Vector3 Direction { get; private set; }

        /// <summary>
        /// Sets the target 'depth' of the direction vector.
        /// Best results when value smaller or equal to zero.
        /// </summary>
        public float DirectionZ
        {
            get { return Direction.Z; }
            set
            {
                Direction = new Vector3(Direction.X, Direction.Y, value);
            }
        }

        /// <summary>
        /// A spotlight, lights in a specific direction
        /// </summary>
        public SpotLight()
            : base(LightType.Spot)
        {
        }

    }
}