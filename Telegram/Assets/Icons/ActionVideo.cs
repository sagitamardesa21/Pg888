//
// Copyright Fela Ameghino 2015-2024
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//       LottieGen version:
//           7.1.0+ge1fa92580f
//       
//       Command:
//           LottieGen -Language CSharp -Namespace Telegram.Assets.Icons -Public -WinUIVersion 2.7 -InputFile ActionVideo.json
//       
//       Input file:
//           ActionVideo.json (1380 bytes created 16:38+01:00 Dec 22 2021)
//       
//       LottieGen source:
//           http://aka.ms/Lottie
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// ____________________________________
// |       Object stats       | Count |
// |__________________________|_______|
// | All CompositionObjects   |    20 |
// |--------------------------+-------|
// | Expression animators     |     1 |
// | KeyFrame animators       |     1 |
// | Reference parameters     |     1 |
// | Expression operations    |     0 |
// |--------------------------+-------|
// | Animated brushes         |     1 |
// | Animated gradient stops  |     - |
// | ExpressionAnimations     |     1 |
// | PathKeyFrameAnimations   |     - |
// |--------------------------+-------|
// | ContainerVisuals         |     1 |
// | ShapeVisuals             |     1 |
// |--------------------------+-------|
// | ContainerShapes          |     - |
// | CompositionSpriteShapes  |     1 |
// |--------------------------+-------|
// | Brushes                  |     1 |
// | Gradient stops           |     - |
// | CompositionVisualSurface |     - |
// ------------------------------------
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.Composition;

namespace Telegram.Assets.Icons
{
    // Name:        u_recording_video
    // Frame rate:  60 fps
    // Frame count: 60
    // Duration:    1000.0 mS
    public sealed class ActionVideo
        : Microsoft.UI.Xaml.Controls.IAnimatedVisualSource
        , Microsoft.UI.Xaml.Controls.IAnimatedVisualSource2
    {
        // Animation duration: 1.000 seconds.
        internal const long c_durationTicks = 10000000;
        internal readonly Color m_foreground;

        public ActionVideo(Color foreground)
        {
            m_foreground = foreground;
        }

        public Microsoft.UI.Xaml.Controls.IAnimatedVisual TryCreateAnimatedVisual(Compositor compositor)
        {
            object ignored = null;
            return TryCreateAnimatedVisual(compositor, out ignored);
        }

        public Microsoft.UI.Xaml.Controls.IAnimatedVisual TryCreateAnimatedVisual(Compositor compositor, out object diagnostics)
        {
            diagnostics = null;

            if (ActionVideo_AnimatedVisual.IsRuntimeCompatible())
            {
                return
                    new ActionVideo_AnimatedVisual(
                        compositor,
                        m_foreground
                        );
            }

            return null;
        }

        /// <summary>
        /// Gets the number of frames in the animation.
        /// </summary>
        public double FrameCount => 60d;

        /// <summary>
        /// Gets the frame rate of the animation.
        /// </summary>
        public double Framerate => 60d;

        /// <summary>
        /// Gets the duration of the animation.
        /// </summary>
        public TimeSpan Duration => TimeSpan.FromTicks(c_durationTicks);

        /// <summary>
        /// Converts a zero-based frame number to the corresponding progress value denoting the
        /// start of the frame.
        /// </summary>
        public double FrameToProgress(double frameNumber)
        {
            return frameNumber / 60d;
        }

        /// <summary>
        /// Returns a map from marker names to corresponding progress values.
        /// </summary>
        public IReadOnlyDictionary<string, double> Markers =>
            new Dictionary<string, double>
            {
            };

        /// <summary>
        /// Sets the color property with the given name, or does nothing if no such property
        /// exists.
        /// </summary>
        public void SetColorProperty(string propertyName, Color value)
        {
        }

        /// <summary>
        /// Sets the scalar property with the given name, or does nothing if no such property
        /// exists.
        /// </summary>
        public void SetScalarProperty(string propertyName, double value)
        {
        }

        sealed class ActionVideo_AnimatedVisual : Microsoft.UI.Xaml.Controls.IAnimatedVisual
        {
            const long c_durationTicks = 10000000;
            readonly Compositor _c;
            readonly Color _f;
            readonly ExpressionAnimation _reusableExpressionAnimation;
            ContainerVisual _root;
            CubicBezierEasingFunction _cubicBezierEasingFunction_0;

            void BindProperty(
                CompositionObject target,
                string animatedPropertyName,
                string expression,
                string referenceParameterName,
                CompositionObject referencedObject)
            {
                _reusableExpressionAnimation.ClearAllParameters();
                _reusableExpressionAnimation.Expression = expression;
                _reusableExpressionAnimation.SetReferenceParameter(referenceParameterName, referencedObject);
                target.StartAnimation(animatedPropertyName, _reusableExpressionAnimation);
            }

            ColorKeyFrameAnimation CreateColorKeyFrameAnimation(float initialProgress, Color initialValue, CompositionEasingFunction initialEasingFunction)
            {
                var result = _c.CreateColorKeyFrameAnimation();
                result.Duration = TimeSpan.FromTicks(c_durationTicks);
                result.InterpolationColorSpace = CompositionColorSpace.Rgb;
                result.InsertKeyFrame(initialProgress, initialValue, initialEasingFunction);
                return result;
            }

            CompositionSpriteShape CreateSpriteShape(CompositionGeometry geometry, Matrix3x2 transformMatrix, CompositionBrush fillBrush)
            {
                var result = _c.CreateSpriteShape(geometry);
                result.TransformMatrix = transformMatrix;
                result.FillBrush = fillBrush;
                return result;
            }

            // - - - Shape tree root for layer: icon
            // - - ShapeGroup: Group 1 Offset:<100, 100>
            CanvasGeometry Geometry()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(0F, 50F));
                    builder.AddCubicBezier(new Vector2(22.0909996F, 50F), new Vector2(40F, 32.0909996F), new Vector2(40F, 10F));
                    builder.AddCubicBezier(new Vector2(40F, -12.0909996F), new Vector2(22.0909996F, -30F), new Vector2(0F, -30F));
                    builder.AddCubicBezier(new Vector2(-22.0909996F, -30F), new Vector2(-40F, -12.0909996F), new Vector2(-40F, 10F));
                    builder.AddCubicBezier(new Vector2(-40F, 32.0909996F), new Vector2(-22.0909996F, 50F), new Vector2(0F, 50F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - - Shape tree root for layer: icon
            // - ShapeGroup: Group 1 Offset:<100, 100>
            // Color
            ColorKeyFrameAnimation ColorAnimation_Black_to_Black()
            {
                // Frame 0.
                var result = CreateColorKeyFrameAnimation(0F, Color.FromArgb(0xFF, _f.R, _f.G, _f.B), HoldThenStepEasingFunction());
                // Frame 10.
                // Transparent
                result.InsertKeyFrame(0.166666672F, Color.FromArgb(0x00, _f.R, _f.G, _f.B), CubicBezierEasingFunction_0());
                // Frame 25.
                // Transparent
                result.InsertKeyFrame(0.416666657F, Color.FromArgb(0x00, _f.R, _f.G, _f.B), _cubicBezierEasingFunction_0);
                // Frame 35.
                // Black
                result.InsertKeyFrame(0.583333313F, Color.FromArgb(0xFF, _f.R, _f.G, _f.B), _cubicBezierEasingFunction_0);
                return result;
            }

            // - Shape tree root for layer: icon
            // ShapeGroup: Group 1 Offset:<100, 100>
            CompositionColorBrush AnimatedColorBrush_Black_to_Black()
            {
                var result = _c.CreateColorBrush();
                result.StartAnimation("Color", ColorAnimation_Black_to_Black());
                var controller = result.TryGetAnimationController("Color");
                controller.Pause();
                BindProperty(controller, "Progress", "_.Progress", "_", _root);
                return result;
            }

            // - Shape tree root for layer: icon
            // ShapeGroup: Group 1 Offset:<100, 100>
            CompositionPathGeometry PathGeometry()
            {
                return _c.CreatePathGeometry(new CompositionPath(Geometry()));
            }

            // Shape tree root for layer: icon
            // Path 1
            CompositionSpriteShape SpriteShape()
            {
                // Offset:<100, 100>
                var geometry = PathGeometry();
                var result = CreateSpriteShape(geometry, new Matrix3x2(1F, 0F, 0F, 1F, 100F, 100F), AnimatedColorBrush_Black_to_Black());
                return result;
            }

            // The root of the composition.
            ContainerVisual Root()
            {
                var result = _root = _c.CreateContainerVisual();
                var propertySet = result.Properties;
                propertySet.InsertScalar("Progress", 0F);
                // Shape tree root for layer: icon
                result.Children.InsertAtTop(ShapeVisual_0());
                return result;
            }

            CubicBezierEasingFunction CubicBezierEasingFunction_0()
            {
                return _cubicBezierEasingFunction_0 = _c.CreateCubicBezierEasingFunction(new Vector2(0.166999996F, 0.166999996F), new Vector2(0.833000004F, 0.833000004F));
            }

            // Shape tree root for layer: icon
            ShapeVisual ShapeVisual_0()
            {
                var result = _c.CreateShapeVisual();
                result.Size = new Vector2(200F, 200F);
                // ShapeGroup: Group 1 Offset:<100, 100>
                result.Shapes.Add(SpriteShape());
                return result;
            }

            // - - - Shape tree root for layer: icon
            // - - ShapeGroup: Group 1 Offset:<100, 100>
            // Color
            StepEasingFunction HoldThenStepEasingFunction()
            {
                var result = _c.CreateStepEasingFunction();
                result.IsFinalStepSingleFrame = true;
                return result;
            }

            internal ActionVideo_AnimatedVisual(
                Compositor compositor,
                Color foreground
                )
            {
                _c = compositor;
                _f = foreground;
                _reusableExpressionAnimation = compositor.CreateExpressionAnimation();
                Root();
            }

            public Visual RootVisual => _root;
            public TimeSpan Duration => TimeSpan.FromTicks(c_durationTicks);
            public Vector2 Size => new Vector2(200F, 200F);
            void IDisposable.Dispose() => _root?.Dispose();

            internal static bool IsRuntimeCompatible()
            {
                return Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);
            }
        }
    }
}
