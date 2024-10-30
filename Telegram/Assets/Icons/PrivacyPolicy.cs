//
// Copyright Fela Ameghino 2015-2023
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
//           LottieGen -Language CSharp -Namespace Telegram.Assets.Icons -Public -WinUIVersion 2.7 -InputFile PrivacyPolicy.json
//       
//       Input file:
//           PrivacyPolicy.json (4248 bytes created 17:41+01:00 Dec 21 2021)
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
// | All CompositionObjects   |    50 |
// |--------------------------+-------|
// | Expression animators     |     4 |
// | KeyFrame animators       |     4 |
// | Reference parameters     |     4 |
// | Expression operations    |     0 |
// |--------------------------+-------|
// | Animated brushes         |     - |
// | Animated gradient stops  |     - |
// | ExpressionAnimations     |     1 |
// | PathKeyFrameAnimations   |     - |
// |--------------------------+-------|
// | ContainerVisuals         |     1 |
// | ShapeVisuals             |     1 |
// |--------------------------+-------|
// | ContainerShapes          |     1 |
// | CompositionSpriteShapes  |     3 |
// |--------------------------+-------|
// | Brushes                  |     2 |
// | Gradient stops           |     2 |
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
    // Name:        u_privacy_policy
    // Frame rate:  60 fps
    // Frame count: 30
    // Duration:    500.0 mS
    sealed class PrivacyPolicy
        : Microsoft.UI.Xaml.Controls.IAnimatedVisualSource
        , Microsoft.UI.Xaml.Controls.IAnimatedVisualSource2
    {
        // Animation duration: 0.500 seconds.
        internal const long c_durationTicks = 5000000;

        public Microsoft.UI.Xaml.Controls.IAnimatedVisual TryCreateAnimatedVisual(Compositor compositor)
        {
            object ignored = null;
            return TryCreateAnimatedVisual(compositor, out ignored);
        }

        public Microsoft.UI.Xaml.Controls.IAnimatedVisual TryCreateAnimatedVisual(Compositor compositor, out object diagnostics)
        {
            diagnostics = null;

            if (PrivacyPolicy_AnimatedVisual.IsRuntimeCompatible())
            {
                return
                    new PrivacyPolicy_AnimatedVisual(
                        compositor
                        );
            }

            return null;
        }

        /// <summary>
        /// Gets the number of frames in the animation.
        /// </summary>
        public double FrameCount => 30d;

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
            return frameNumber / 30d;
        }

        /// <summary>
        /// Returns a map from marker names to corresponding progress values.
        /// </summary>
        public IReadOnlyDictionary<string, double> Markers =>
            new Dictionary<string, double>
            {
                { "NormalToPointerOver_Start", 0.0 },
                { "NormalToPointerOver_End", 1 },
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

        sealed class PrivacyPolicy_AnimatedVisual : Microsoft.UI.Xaml.Controls.IAnimatedVisual
        {
            const long c_durationTicks = 5000000;
            readonly Compositor _c;
            readonly ExpressionAnimation _reusableExpressionAnimation;
            CompositionColorBrush _colorBrush_White;
            CompositionPath _path;
            ContainerVisual _root;
            CubicBezierEasingFunction _cubicBezierEasingFunction_0;
            ExpressionAnimation _rootProgress;
            StepEasingFunction _holdThenStepEasingFunction;
            StepEasingFunction _stepThenHoldEasingFunction;

            static void StartProgressBoundAnimation(
                CompositionObject target,
                string animatedPropertyName,
                CompositionAnimation animation,
                ExpressionAnimation controllerProgressExpression)
            {
                target.StartAnimation(animatedPropertyName, animation);
                var controller = target.TryGetAnimationController(animatedPropertyName);
                controller.Pause();
                controller.StartAnimation("Progress", controllerProgressExpression);
            }

            ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(float initialProgress, float initialValue, CompositionEasingFunction initialEasingFunction)
            {
                var result = _c.CreateScalarKeyFrameAnimation();
                result.Duration = TimeSpan.FromTicks(c_durationTicks);
                result.InsertKeyFrame(initialProgress, initialValue, initialEasingFunction);
                return result;
            }

            Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(float initialProgress, Vector2 initialValue, CompositionEasingFunction initialEasingFunction)
            {
                var result = _c.CreateVector2KeyFrameAnimation();
                result.Duration = TimeSpan.FromTicks(c_durationTicks);
                result.InsertKeyFrame(initialProgress, initialValue, initialEasingFunction);
                return result;
            }

            // - - - Shape tree root for layer: icon
            // - - ShapeGroup: Group 2
            CanvasGeometry Geometry_0()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(2.77399993F, -79.1600037F));
                    builder.AddCubicBezier(new Vector2(1.09399998F, -80.2799988F), new Vector2(-1.09399998F, -80.2799988F), new Vector2(-2.773F, -79.1600037F));
                    builder.AddCubicBezier(new Vector2(-22.1550007F, -66.2389984F), new Vector2(-43.1170006F, -58.1769981F), new Vector2(-65.7070007F, -54.9500008F));
                    builder.AddCubicBezier(new Vector2(-68.1699982F, -54.5979996F), new Vector2(-70F, -52.487999F), new Vector2(-70F, -50F));
                    builder.AddLine(new Vector2(-70F, -5F));
                    builder.AddCubicBezier(new Vector2(-70F, 33.9129982F), new Vector2(-46.9300003F, 62.3069992F), new Vector2(-1.79499996F, 79.6669998F));
                    builder.AddCubicBezier(new Vector2(-0.639999986F, 80.1110001F), new Vector2(0.639999986F, 80.1110001F), new Vector2(1.79499996F, 79.6669998F));
                    builder.AddCubicBezier(new Vector2(46.9300003F, 62.3069992F), new Vector2(70F, 33.9129982F), new Vector2(70F, -5F));
                    builder.AddLine(new Vector2(70F, -50F));
                    builder.AddCubicBezier(new Vector2(70F, -52.487999F), new Vector2(68.1699982F, -54.5979996F), new Vector2(65.7070007F, -54.9500008F));
                    builder.AddCubicBezier(new Vector2(43.1170006F, -58.1769981F), new Vector2(22.1550007F, -66.2389984F), new Vector2(2.77399993F, -79.1600037F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            CanvasGeometry Geometry_1()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.BeginFigure(new Vector2(-30F, 0F));
                    builder.AddLine(new Vector2(-10F, 20F));
                    builder.AddLine(new Vector2(30F, -20F));
                    builder.EndFigure(CanvasFigureLoop.Open);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            CompositionColorBrush ColorBrush_White()
            {
                return _colorBrush_White = _c.CreateColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            }

            // - - Shape tree root for layer: icon
            // - ShapeGroup: Group 2
            // Stop 0
            CompositionColorGradientStop GradientStop_0_AlmostLightGray_FFC8CED4()
            {
                return _c.CreateColorGradientStop(0F, Color.FromArgb(0xFF, 0xC8, 0xCE, 0xD4));
            }

            // - - Shape tree root for layer: icon
            // - ShapeGroup: Group 2
            // Stop 1
            CompositionColorGradientStop GradientStop_1_AlmostLightSlateGray_FF899097()
            {
                return _c.CreateColorGradientStop(1F, Color.FromArgb(0xFF, 0x89, 0x90, 0x97));
            }

            // Shape tree root for layer: icon
            // ShapeGroup: Group 1
            CompositionContainerShape ContainerShape()
            {
                var result = _c.CreateContainerShape();
                result.Offset = new Vector2(105F, 100F);
                var shapes = result.Shapes;
                // Path 1
                shapes.Add(SpriteShape_1());
                // Path 1
                shapes.Add(SpriteShape_2());
                StartProgressBoundAnimation(result, "Scale", ScaleVector2Animation_1(), _rootProgress);
                return result;
            }

            // - Shape tree root for layer: icon
            // ShapeGroup: Group 2
            CompositionLinearGradientBrush LinearGradientBrush()
            {
                var result = _c.CreateLinearGradientBrush();
                var colorStops = result.ColorStops;
                colorStops.Add(GradientStop_0_AlmostLightGray_FFC8CED4());
                colorStops.Add(GradientStop_1_AlmostLightSlateGray_FF899097());
                result.MappingMode = CompositionMappingMode.Absolute;
                result.StartPoint = new Vector2(0.0329999998F, -78.6460037F);
                result.EndPoint = new Vector2(0.782999992F, 79.7890015F);
                return result;
            }

            CompositionPath Path()
            {
                var result = _path = new CompositionPath(Geometry_1());
                return result;
            }

            // - Shape tree root for layer: icon
            // ShapeGroup: Group 2
            CompositionPathGeometry PathGeometry_0()
            {
                return _c.CreatePathGeometry(new CompositionPath(Geometry_0()));
            }

            // - - Shape tree root for layer: icon
            // - ShapeGroup: Group 1
            // Path 1
            CompositionPathGeometry PathGeometry_1()
            {
                var result = _c.CreatePathGeometry(Path());
                StartProgressBoundAnimation(result, "TrimStart", TrimStartScalarAnimation_0_to_1(), _rootProgress);
                return result;
            }

            // - - Shape tree root for layer: icon
            // - ShapeGroup: Group 1
            // Path 1
            CompositionPathGeometry PathGeometry_2()
            {
                var result = _c.CreatePathGeometry(_path);
                StartProgressBoundAnimation(result, "TrimEnd", TrimEndScalarAnimation_0_to_1(), _rootProgress);
                return result;
            }

            // Shape tree root for layer: icon
            // Path 1
            CompositionSpriteShape SpriteShape_0()
            {
                var result = _c.CreateSpriteShape(PathGeometry_0());
                result.Offset = new Vector2(100F, 100F);
                result.FillBrush = LinearGradientBrush();
                StartProgressBoundAnimation(result, "Scale", ScaleVector2Animation_0(), RootProgress());
                return result;
            }

            // - Shape tree root for layer: icon
            // ShapeGroup: Group 1
            // Path 1
            CompositionSpriteShape SpriteShape_1()
            {
                var result = _c.CreateSpriteShape(PathGeometry_1());
                result.StrokeBrush = ColorBrush_White();
                result.StrokeDashCap = CompositionStrokeCap.Round;
                result.StrokeStartCap = CompositionStrokeCap.Round;
                result.StrokeEndCap = CompositionStrokeCap.Round;
                result.StrokeLineJoin = CompositionStrokeLineJoin.Round;
                result.StrokeMiterLimit = 2F;
                result.StrokeThickness = 10F;
                return result;
            }

            // - Shape tree root for layer: icon
            // ShapeGroup: Group 1
            // Path 1
            CompositionSpriteShape SpriteShape_2()
            {
                var result = _c.CreateSpriteShape(PathGeometry_2());
                result.StrokeBrush = _colorBrush_White;
                result.StrokeDashCap = CompositionStrokeCap.Round;
                result.StrokeStartCap = CompositionStrokeCap.Round;
                result.StrokeEndCap = CompositionStrokeCap.Round;
                result.StrokeLineJoin = CompositionStrokeLineJoin.Round;
                result.StrokeMiterLimit = 2F;
                result.StrokeThickness = 10F;
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
                return _cubicBezierEasingFunction_0 = _c.CreateCubicBezierEasingFunction(new Vector2(0.600000024F, 0F), new Vector2(0.400000006F, 1F));
            }

            ExpressionAnimation RootProgress()
            {
                var result = _rootProgress = _c.CreateExpressionAnimation("_.Progress");
                result.SetReferenceParameter("_", _root);
                return result;
            }

            // - - - Shape tree root for layer: icon
            // - - ShapeGroup: Group 1
            // - Path 1
            // TrimEnd
            ScalarKeyFrameAnimation TrimEndScalarAnimation_0_to_1()
            {
                // Frame 0.
                var result = CreateScalarKeyFrameAnimation(0F, 0F, _stepThenHoldEasingFunction);
                // Frame 16.
                result.InsertKeyFrame(0.533333361F, 0F, _holdThenStepEasingFunction);
                // Frame 26.
                result.InsertKeyFrame(0.866666675F, 1F, _cubicBezierEasingFunction_0);
                return result;
            }

            // - - - Shape tree root for layer: icon
            // - - ShapeGroup: Group 1
            // - Path 1
            // TrimStart
            ScalarKeyFrameAnimation TrimStartScalarAnimation_0_to_1()
            {
                // Frame 0.
                var result = CreateScalarKeyFrameAnimation(0F, 0F, StepThenHoldEasingFunction());
                // Frame 8.
                result.InsertKeyFrame(0.266666681F, 0F, _holdThenStepEasingFunction);
                // Frame 18.
                result.InsertKeyFrame(0.600000024F, 1F, _cubicBezierEasingFunction_0);
                return result;
            }

            // Shape tree root for layer: icon
            ShapeVisual ShapeVisual_0()
            {
                var result = _c.CreateShapeVisual();
                result.Size = new Vector2(200F, 200F);
                var shapes = result.Shapes;
                // ShapeGroup: Group 2
                shapes.Add(SpriteShape_0());
                // ShapeGroup: Group 1
                shapes.Add(ContainerShape());
                return result;
            }

            StepEasingFunction HoldThenStepEasingFunction()
            {
                var result = _holdThenStepEasingFunction = _c.CreateStepEasingFunction();
                result.IsFinalStepSingleFrame = true;
                return result;
            }

            StepEasingFunction StepThenHoldEasingFunction()
            {
                var result = _stepThenHoldEasingFunction = _c.CreateStepEasingFunction();
                result.IsInitialStepSingleFrame = true;
                return result;
            }

            // - Shape tree root for layer: icon
            // ShapeGroup: Group 2
            // Scale
            Vector2KeyFrameAnimation ScaleVector2Animation_0()
            {
                // Frame 0.
                var result = CreateVector2KeyFrameAnimation(0F, new Vector2(1F, 1F), HoldThenStepEasingFunction());
                // Frame 8.
                result.InsertKeyFrame(0.266666681F, new Vector2(1.12F, 1.12F), CubicBezierEasingFunction_0());
                // Frame 16.
                result.InsertKeyFrame(0.533333361F, new Vector2(0.949999988F, 0.949999988F), _cubicBezierEasingFunction_0);
                // Frame 24.
                result.InsertKeyFrame(0.800000012F, new Vector2(1F, 1F), _cubicBezierEasingFunction_0);
                return result;
            }

            // - Shape tree root for layer: icon
            // ShapeGroup: Group 1
            // Scale
            Vector2KeyFrameAnimation ScaleVector2Animation_1()
            {
                // Frame 0.
                var result = CreateVector2KeyFrameAnimation(0F, new Vector2(1F, 1F), _stepThenHoldEasingFunction);
                // Frame 3.
                result.InsertKeyFrame(0.100000001F, new Vector2(1F, 1F), _holdThenStepEasingFunction);
                // Frame 11.
                result.InsertKeyFrame(0.366666675F, new Vector2(1.12F, 1.12F), _cubicBezierEasingFunction_0);
                // Frame 19.
                result.InsertKeyFrame(0.633333325F, new Vector2(0.949999988F, 0.949999988F), _cubicBezierEasingFunction_0);
                // Frame 27.
                result.InsertKeyFrame(0.899999976F, new Vector2(1F, 1F), _cubicBezierEasingFunction_0);
                return result;
            }

            internal PrivacyPolicy_AnimatedVisual(
                Compositor compositor
                )
            {
                _c = compositor;
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
