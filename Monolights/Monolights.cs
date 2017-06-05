using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monolights
{
    /// <summary>
    /// 2D Deferred Lights for Monogame. 
    /// Version 1.0, 2017-06-05 Felsir (http://www.felsirworld.net/)
    /// 
    /// Implementation of a deferred lighting engine in Monogame.
    /// Creates a light effect as seen in the game 'Full Bore' or effects as created by 'Sprite Lamp'.
    /// 
    /// 1. Create the class and add some lights.
    /// 2. Render the game to this class' colormap.
    /// 3. Render the same frame with normalmap data to this class' normalmap. 
    /// 4. Call the Draw function in the class.
    /// 5. Enjoy the results.
    /// 
    /// </summary>
    public class Monolights
    {
        private GraphicsDevice _graphicsDevice;
        private RenderTarget2D _normalMapRenderTarget;
        private RenderTarget2D _colorMapRenderTarget;
        private RenderTarget2D _lightMapRenderTarget;

        private VertexBuffer _screenQuad;
        private List<Light> _lights = new List<Light>();

        private Effect _lightEffect;
        private Effect _deferredLightEffect;

        private EffectTechnique _lightEffectTechniquePointLight;
        private EffectTechnique _lightEffectTechniqueSpotLight;
        private EffectParameter _lightEffectParameterStrength;
        private EffectParameter _lightEffectParameterPosition;
        private EffectParameter _lightEffectParameterLightColor;
        private EffectParameter _lightEffectParameterLightDecay;
        private EffectParameter _lightEffectParameterScreenWidth;
        private EffectParameter _lightEffectParameterScreenHeight;
        private EffectParameter _lightEffectParameterNormapMap;
        private EffectParameter _lightEffectParameterInvertY;
        private EffectParameter _lightEffectParameterAmbientColor;
        private EffectParameter _lightEffectParameterColormap;
        private EffectParameter _lightEffectParameterSpecularStrength;

        private EffectParameter _lightEffectParameterConeAngle;
        private EffectParameter _lightEffectParameterConeDecay;
        private EffectParameter _lightEffectParameterConeDirection;

        private EffectTechnique _deferredLightEffectTechnique;
        private EffectParameter _deferredLightEffectParamAmbient;
        private EffectParameter _deferredLightEffectParamLightAmbient;
        private EffectParameter _deferredLightEffectParamAmbientColor;
        private EffectParameter _deferredLightEffectParamColorMap;
        private EffectParameter _deferredLightEffectParamShadowMap;
        private EffectParameter _deferredLightEffectParamNormalMap;

        /// <summary>
        /// The color of the ambient light that affects the scene.
        /// </summary>
        public Color AmbientLight = new Color(1f, 1f, 1f, 1f);

        /// <summary>
        /// How much the AmbientLight affects the scene.
        /// </summary>
        public float AmbientLightPower = 0.1f;

        /// <summary>
        /// Set the specular strength of the rendered scene.
        /// </summary>
        public float SpecularStrength = 1.5f;

        /// <summary>
        /// Most normalmaps are generated with the Y axis inverted.
        /// </summary>
        public bool InvertYNormal = true;

        /// <summary>
        /// 2D Deferred lights. 
        /// </summary>
        /// <param name="graphicsdevice">Graphics device to create rendertarget and vertexbuffer</param>
        /// <param name="lighteffect">The lighteffect - used to draw lights on the lightmap</param>
        /// <param name="deferredlighteffect">The deferredlighteffect - used to combine the diffuse, normal and lightmap.</param>
        public Monolights(GraphicsDevice graphicsdevice, Effect lighteffect, Effect deferredlighteffect)
        {
            _graphicsDevice = graphicsdevice;
            _lightEffect = lighteffect;
            _deferredLightEffect = deferredlighteffect;

            VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
            Vertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
            Vertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
            Vertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            Vertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));
            _screenQuad = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColorTexture), Vertices.Length, BufferUsage.None);
            _screenQuad.SetData(Vertices);

            PresentationParameters pp = _graphicsDevice.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;
            SurfaceFormat format = pp.BackBufferFormat;

            _colorMapRenderTarget = new RenderTarget2D(_graphicsDevice, width, height);
            _normalMapRenderTarget = new RenderTarget2D(_graphicsDevice, width, height);
            _lightMapRenderTarget = new RenderTarget2D(_graphicsDevice, width, height, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            // Point light technique
            _lightEffectTechniquePointLight = _lightEffect.Techniques["DeferredPointLight"];

            // Spot light technique
            _lightEffectTechniqueSpotLight = _lightEffect.Techniques["DeferredSpotLight"];

            // Shared light properties
            _lightEffectParameterLightColor = _lightEffect.Parameters["lightColor"];
            _lightEffectParameterLightDecay = _lightEffect.Parameters["lightDecay"];
            _lightEffectParameterPosition = _lightEffect.Parameters["lightPosition"];
            _lightEffectParameterScreenHeight = _lightEffect.Parameters["screenHeight"];
            _lightEffectParameterScreenWidth = _lightEffect.Parameters["screenWidth"];
            _lightEffectParameterStrength = _lightEffect.Parameters["lightStrength"];
            _lightEffectParameterSpecularStrength = _lightEffect.Parameters["specularStrength"]; 
            _lightEffectParameterInvertY = _lightEffect.Parameters["invertY"];
            _lightEffectParameterAmbientColor = _lightEffect.Parameters["ambientColor"];
            _lightEffectParameterColormap = _lightEffect.Parameters["ColorMap"];
            _lightEffectParameterNormapMap = _lightEffect.Parameters["NormalMap"];

            // Spot light parameters
            _lightEffectParameterConeDirection = _lightEffect.Parameters["coneDirection"];
            _lightEffectParameterConeAngle = _lightEffect.Parameters["coneAngle"];
            _lightEffectParameterConeDecay = _lightEffect.Parameters["coneDecay"];

            // Deferred effect parameters
            _deferredLightEffectTechnique = _deferredLightEffect.Techniques["DeferredLightEffect"];
            _deferredLightEffectParamAmbient = _deferredLightEffect.Parameters["ambient"];
            _deferredLightEffectParamLightAmbient = _deferredLightEffect.Parameters["lightAmbient"];
            _deferredLightEffectParamAmbientColor = _deferredLightEffect.Parameters["ambientColor"];
            _deferredLightEffectParamColorMap = _deferredLightEffect.Parameters["ColorMap"];
            _deferredLightEffectParamShadowMap = _deferredLightEffect.Parameters["ShadingMap"];
            _deferredLightEffectParamNormalMap = _deferredLightEffect.Parameters["NormalMap"];

        }

        /// <summary>
        /// The Normalmap, that the main game can use to draw the normal data to.
        /// </summary>
        public RenderTarget2D Normalmap
        {
            get { return _normalMapRenderTarget; }
        }

        /// <summary>
        /// The Colormap (or diffusemap), that the main game can use to draw the game visuals to.
        /// </summary>
        public RenderTarget2D Colormap
        {
            get { return _colorMapRenderTarget; }
        }

        /// <summary>
        /// Add a light to the scene.
        /// </summary>
        /// <param name="light">The light to be added.</param>
        public void AddLight(Light light)
        {
            _lights.Add(light);
        }

        /// <summary>
        /// Remove a specific light.
        /// </summary>
        /// <param name="light">Light to remove.</param>
        public void RemoveLight(Light light)
        {
            if (_lights.Contains(light))
                _lights.Remove(light);
        }

        /// <summary>
        /// Clear all lights from the scene.
        /// </summary>
        public void ClearLights()
        {
            _lights.Clear();
        }

        public Light SceneLight(int index)
        {
            if (index >= 0 && index < _lights.Count)
            {
                return _lights[index];
            }
            else
            {
                throw new System.Exception("Lightindex out of bounds.");
            }
        }

        /// <summary>
        /// Return the number of lights in the scene.
        /// </summary>
        public int LightsCount
        {
            get { return _lights.Count; }
        }

        /// <summary>
        /// Draw the lighted scene to the rendertarget. It is assumed the diffuse and normal maps are generated.
        /// </summary>
        /// <param name="rendertarget">The rendertarget to render the result (null is allowed).</param>
        /// <param name="spritebatch">A spritebatch to use in the render process.</param>
        /// <param name="viewport">Optional a viewport, which is used for culling and shifting the lights into view. If not supplied, no culling happens.</param>
        public void Draw(RenderTarget2D rendertarget, SpriteBatch spritebatch, Rectangle? viewport=null)
        {
            //Draw the lights based on the normalmap to the lightmap rendertarget.
            GenerateLightMap(viewport);

            _graphicsDevice.SetRenderTarget(rendertarget);
            _graphicsDevice.Clear(Color.Black);

            // Draw the combined Maps onto the rendertarget.
            DrawCombinedMaps(spritebatch);
        }

        /// <summary>
        /// Generates the light map from the ColorMap and NormalMap textures combined with all the active lights.
        /// </summary>
        /// <param name="viewport">Optional a viewport, which is used for culling and shifting the lights into view.</param>
        /// <returns></returns>
        private void GenerateLightMap(Rectangle? viewport)
        {
            _graphicsDevice.SetRenderTarget(_lightMapRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);
            _graphicsDevice.SetVertexBuffer(_screenQuad);

            _lightEffectParameterScreenWidth.SetValue((float)_graphicsDevice.Viewport.Width);
            _lightEffectParameterScreenHeight.SetValue((float)_graphicsDevice.Viewport.Height);
            _lightEffectParameterAmbientColor.SetValue(AmbientLight.ToVector4());
            _lightEffectParameterColormap.SetValue(_colorMapRenderTarget);
            _lightEffectParameterNormapMap.SetValue(_normalMapRenderTarget);
            _lightEffectParameterInvertY.SetValue(InvertYNormal);

            Vector3 offset = Vector3.Zero; //offset if the viewport has moved.
            if (viewport.HasValue)
            {
                //Set the offset.
                offset.X = -viewport.Value.X;
                offset.Y = -viewport.Value.Y;
            }


            //Loop through all the lights
            foreach (var light in _lights)
            {
                //If a light is not enabled- do not render and continue to the next light.
                if (!light.IsEnabled)
                    continue;

                //do culling if the viewport was set.
                if (viewport.HasValue)
                {
                    //Culling is now based on a simple boundingbox overlap.

                    if(light.LightType==LightType.Point && !new Rectangle((int)(light.Position.X-light.LightDecay), (int)(light.Position.Y - light.LightDecay),(int)(2*light.LightDecay),(int)(2*light.LightDecay)).Intersects(viewport.Value))
                    {
                        //the rectangles do not overlap. Do not draw the light.
                        continue;
                    }
                    if(light.LightType==LightType.Spot)
                    {
                        //the spotlight is a bit more complicated.
                        //for no do a boundingbox check; this should be improved to only hold the actual direction, and width of the beam of the spotlight
                        if(!new Rectangle((int)(light.Position.X - (2*light.LightDecay)), (int)(light.Position.Y - (2*light.LightDecay)), (int)(4 * light.LightDecay), (int)(4 * light.LightDecay)).Intersects(viewport.Value))
                        {
                            //the rectangles do not overlap. Do not draw the light.
                            continue;
                        }
                        
                    }

                }

                // Set the values for this lightsource.
                _lightEffectParameterStrength.SetValue(light.Power);
                _lightEffectParameterPosition.SetValue(light.Position + offset);
                _lightEffectParameterLightColor.SetValue(light.Color4);
                _lightEffectParameterLightDecay.SetValue(light.LightDecay); 
                _lightEffectParameterSpecularStrength.SetValue(SpecularStrength);

                if (light.LightType == LightType.Point)
                {
                    _lightEffect.CurrentTechnique = _lightEffectTechniquePointLight;
                }
                else
                {
                    _lightEffect.CurrentTechnique = _lightEffectTechniqueSpotLight;
                    _lightEffectParameterConeDecay.SetValue(((SpotLight)light).SpotBeamWidthExponent);
                    _lightEffectParameterConeDirection.SetValue(((SpotLight)light).Direction);
                }


                _lightEffect.CurrentTechnique.Passes[0].Apply();

                // Add Black background
                _graphicsDevice.BlendState = BlendBlack;

                // Draw the light:
                _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            // Complete the drawing
            _graphicsDevice.SetRenderTarget(null);

        }

        /// <summary>
        /// Draw the final result, based on the diffuse, normalmap and generated lightmap.
        /// </summary>
        /// <param name="spritebatch">Spritebatch to use in the render process.</param>
        private void DrawCombinedMaps(SpriteBatch spritebatch)
        {
            _deferredLightEffect.CurrentTechnique = _deferredLightEffectTechnique;
            _deferredLightEffectParamAmbient.SetValue(AmbientLightPower);
            _deferredLightEffectParamLightAmbient.SetValue(2f);
            _deferredLightEffectParamAmbientColor.SetValue(AmbientLight.ToVector4());

            _deferredLightEffectParamShadowMap.SetValue(_lightMapRenderTarget);
            _deferredLightEffectParamNormalMap.SetValue(_normalMapRenderTarget);
            _deferredLightEffect.CurrentTechnique.Passes[0].Apply();

            spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _deferredLightEffect);
            spritebatch.Draw(_colorMapRenderTarget, Vector2.Zero, Color.White);
            spritebatch.End();
        }

        /// <summary>
        /// Draws the internal render targets onto the bottom of the screen.
        /// </summary>
        /// <param name="spriteBatch">Spritebatch to use in the render process.</param>
        public void DrawDebugRenderTargets(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.Opaque);

            //calculate the dimensions of each map.
            Rectangle size = new Rectangle(0, 0, _colorMapRenderTarget.Width / 3, _colorMapRenderTarget.Height / 3);
            var position = new Vector2(0, _graphicsDevice.Viewport.Height - size.Height);
            spriteBatch.Draw(
                _colorMapRenderTarget,
                new Rectangle(
                    (int)position.X, (int)position.Y,
                    size.Width,
                    size.Height),
                Color.White);

            spriteBatch.Draw(
                _normalMapRenderTarget,
                new Rectangle(
                    (int)position.X + size.Width, (int)position.Y,
                    size.Width,
                    size.Height),
                Color.White);

            spriteBatch.Draw(
                _lightMapRenderTarget,
                new Rectangle(
                    (int)position.X + size.Width * 2, (int)position.Y,
                    size.Width,
                    size.Height),
                Color.White);

            spriteBatch.End();
        }

        /// <summary>
        /// Blendstate used to combine the lights.
        /// </summary>
        public static BlendState BlendBlack = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,

            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One
        };

    }
}
