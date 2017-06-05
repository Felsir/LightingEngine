namespace Monolights
{
    public class PointLight : Light
    {
        /// <summary>
        /// A pointlight that shines even in all directions.
        /// </summary>
        public PointLight()
            : base (LightType.Point)
        {
            
        }
    }
}
