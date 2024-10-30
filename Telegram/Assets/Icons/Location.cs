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
//           7.1.0-build.5+g109463c06a
//       
//       Command:
//           LottieGen -Language CSharp -MinimumUapVersion 8 -Public -WinUIVersion 2.7 -InputFile u_location.json
//       
//       Input file:
//           u_location.json (3095 bytes created 19:23+01:00 Mar 17 2022)
//       
//       LottieGen source:
//           http://aka.ms/Lottie
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// ___________________________________________________________
// |       Object stats       | UAP v11 count | UAP v7 count |
// |__________________________|_______________|______________|
// | All CompositionObjects   |            24 |           14 |
// |--------------------------+---------------+--------------|
// | Expression animators     |             1 |            0 |
// | KeyFrame animators       |             1 |            0 |
// | Reference parameters     |             1 |            0 |
// | Expression operations    |             0 |            0 |
// |--------------------------+---------------+--------------|
// | Animated brushes         |             - |            - |
// | Animated gradient stops  |             - |            - |
// | ExpressionAnimations     |             1 |            - |
// | PathKeyFrameAnimations   |             1 |            - |
// |--------------------------+---------------+--------------|
// | ContainerVisuals         |             1 |            1 |
// | ShapeVisuals             |             1 |            1 |
// |--------------------------+---------------+--------------|
// | ContainerShapes          |             - |            - |
// | CompositionSpriteShapes  |             1 |            1 |
// |--------------------------+---------------+--------------|
// | Brushes                  |             1 |            1 |
// | Gradient stops           |             2 |            2 |
// | CompositionVisualSurface |             - |            - |
// -----------------------------------------------------------
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.Composition;

namespace Telegram.Assets.Icons
{
    // Name:        u_location
    // Frame rate:  60 fps
    // Frame count: 30
    // Duration:    500.0 mS
    sealed class Location
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

            if (Location_AnimatedVisual_UAPv11.IsRuntimeCompatible())
            {
                var res =
                    new Location_AnimatedVisual_UAPv11(
                        compositor
                        );
                res.CreateAnimations();
                return res;
            }

            if (Location_AnimatedVisual_UAPv7.IsRuntimeCompatible())
            {
                var res =
                    new Location_AnimatedVisual_UAPv7(
                        compositor
                        );
                res.CreateAnimations();
                return res;
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

        sealed class Location_AnimatedVisual_UAPv11 : Microsoft.UI.Xaml.Controls.IAnimatedVisual
        {
            const long c_durationTicks = 5000000;
            readonly Compositor _c;
            readonly ExpressionAnimation _reusableExpressionAnimation;
            CompositionPath _path;
            CompositionPathGeometry _pathGeometry;
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

            PathKeyFrameAnimation CreatePathKeyFrameAnimation(float initialProgress, CompositionPath initialValue, CompositionEasingFunction initialEasingFunction)
            {
                var result = _c.CreatePathKeyFrameAnimation();
                result.Duration = TimeSpan.FromTicks(c_durationTicks);
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

            CanvasGeometry Geometry_0()
            {
                var result = CanvasGeometry.CreateGroup(
                    null,
                    new CanvasGeometry[] { Geometry_1(), Geometry_2() },
                    CanvasFilledRegionDetermination.Winding);
                return result;
            }

            CanvasGeometry Geometry_1()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(-2.33299994F, -1.14300001F));
                    builder.AddCubicBezier(new Vector2(-2.33299994F, -2.40499997F), new Vector2(-1.28900003F, -3.4289999F), new Vector2(0F, -3.4289999F));
                    builder.AddCubicBezier(new Vector2(1.28900003F, -3.4289999F), new Vector2(2.33299994F, -2.40499997F), new Vector2(2.33299994F, -1.14300001F));
                    builder.AddCubicBezier(new Vector2(2.33299994F, 0.119000003F), new Vector2(1.28900003F, 1.14300001F), new Vector2(0F, 1.14300001F));
                    builder.AddCubicBezier(new Vector2(-1.28900003F, 1.14300001F), new Vector2(-2.33299994F, 0.119000003F), new Vector2(-2.33299994F, -1.14300001F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            CanvasGeometry Geometry_2()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(7F, -1.14300001F));
                    builder.AddCubicBezier(new Vector2(7F, -4.92999983F), new Vector2(3.86599994F, -8F), new Vector2(0F, -8F));
                    builder.AddCubicBezier(new Vector2(-3.86599994F, -8F), new Vector2(-7F, -4.92999983F), new Vector2(-7F, -1.14300001F));
                    builder.AddCubicBezier(new Vector2(-7F, 2.13400006F), new Vector2(-4.7420001F, 5.1500001F), new Vector2(-0.312999994F, 7.91099977F));
                    builder.AddCubicBezier(new Vector2(-0.122000001F, 8.02999973F), new Vector2(0.122000001F, 8.02999973F), new Vector2(0.312999994F, 7.91099977F));
                    builder.AddCubicBezier(new Vector2(4.7420001F, 5.1500001F), new Vector2(7F, 2.13400006F), new Vector2(7F, -1.14300001F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - - - - Shape tree root for layer: Shape
            // - - -  Offset:<100, 100>
            // - - Path 2+Path 1.PathGeometry
            // - Path
            CanvasGeometry Geometry_3()
            {
                var result = CanvasGeometry.CreateGroup(
                    null,
                    new CanvasGeometry[] { Geometry_4(), Geometry_5() },
                    CanvasFilledRegionDetermination.Winding);
                return result;
            }

            // - - - - - Shape tree root for layer: Shape
            // - - - -  Offset:<100, 100>
            // - - - Path 2+Path 1.PathGeometry
            // - - Path
            CanvasGeometry Geometry_4()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(-2.33299994F, -3.14299989F));
                    builder.AddCubicBezier(new Vector2(-2.33299994F, -4.40500021F), new Vector2(-1.28900003F, -5.4289999F), new Vector2(0F, -5.4289999F));
                    builder.AddCubicBezier(new Vector2(1.28900003F, -5.4289999F), new Vector2(2.33299994F, -4.40500021F), new Vector2(2.33299994F, -3.14299989F));
                    builder.AddCubicBezier(new Vector2(2.33299994F, -1.88100004F), new Vector2(1.28900003F, -0.856999993F), new Vector2(0F, -0.856999993F));
                    builder.AddCubicBezier(new Vector2(-1.28900003F, -0.856999993F), new Vector2(-2.33299994F, -1.88100004F), new Vector2(-2.33299994F, -3.14299989F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - - - - - Shape tree root for layer: Shape
            // - - - -  Offset:<100, 100>
            // - - - Path 2+Path 1.PathGeometry
            // - - Path
            CanvasGeometry Geometry_5()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(7F, -3.14299989F));
                    builder.AddCubicBezier(new Vector2(7F, -6.92999983F), new Vector2(3.86599994F, -9.33300018F), new Vector2(0F, -9.33300018F));
                    builder.AddCubicBezier(new Vector2(-3.86599994F, -9.33300018F), new Vector2(-7F, -6.92999983F), new Vector2(-7F, -3.14299989F));
                    builder.AddCubicBezier(new Vector2(-7F, 0.134000003F), new Vector2(-4.7420001F, 2.48300004F), new Vector2(-0.312999994F, 5.24399996F));
                    builder.AddCubicBezier(new Vector2(-0.122000001F, 5.36299992F), new Vector2(0.122000001F, 5.36299992F), new Vector2(0.312999994F, 5.24399996F));
                    builder.AddCubicBezier(new Vector2(4.7420001F, 2.48300004F), new Vector2(7F, 0.134000003F), new Vector2(7F, -3.14299989F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - - - - Shape tree root for layer: Shape
            // - - -  Offset:<100, 100>
            // - - Path 2+Path 1.PathGeometry
            // - Path
            CanvasGeometry Geometry_6()
            {
                var result = CanvasGeometry.CreateGroup(
                    null,
                    new CanvasGeometry[] { Geometry_7(), Geometry_8() },
                    CanvasFilledRegionDetermination.Winding);
                return result;
            }

            // - - - - - Shape tree root for layer: Shape
            // - - - -  Offset:<100, 100>
            // - - - Path 2+Path 1.PathGeometry
            // - - Path
            CanvasGeometry Geometry_7()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(-2.33299994F, -0.143000007F));
                    builder.AddCubicBezier(new Vector2(-2.33299994F, -1.40499997F), new Vector2(-1.28900003F, -2.4289999F), new Vector2(0F, -2.4289999F));
                    builder.AddCubicBezier(new Vector2(1.28900003F, -2.4289999F), new Vector2(2.33299994F, -1.40499997F), new Vector2(2.33299994F, -0.143000007F));
                    builder.AddCubicBezier(new Vector2(2.33299994F, 1.11899996F), new Vector2(1.28900003F, 2.14299989F), new Vector2(0F, 2.14299989F));
                    builder.AddCubicBezier(new Vector2(-1.28900003F, 2.14299989F), new Vector2(-2.33299994F, 1.11899996F), new Vector2(-2.33299994F, -0.143000007F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - - - - - Shape tree root for layer: Shape
            // - - - -  Offset:<100, 100>
            // - - - Path 2+Path 1.PathGeometry
            // - - Path
            CanvasGeometry Geometry_8()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(7F, -0.143000007F));
                    builder.AddCubicBezier(new Vector2(7F, -3.93000007F), new Vector2(3.86599994F, -7F), new Vector2(0F, -7F));
                    builder.AddCubicBezier(new Vector2(-3.86599994F, -7F), new Vector2(-7F, -3.93000007F), new Vector2(-7F, -0.143000007F));
                    builder.AddCubicBezier(new Vector2(-7F, 3.13400006F), new Vector2(-4.7420001F, 5.81599998F), new Vector2(-0.312999994F, 8.57699966F));
                    builder.AddCubicBezier(new Vector2(-0.122000001F, 8.6960001F), new Vector2(0.122000001F, 8.6960001F), new Vector2(0.312999994F, 8.57699966F));
                    builder.AddCubicBezier(new Vector2(4.7420001F, 5.81599998F), new Vector2(7F, 3.13400006F), new Vector2(7F, -0.143000007F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - - Shape tree root for layer: Shape
            // -  Offset:<100, 100>
            // Stop 0
            CompositionColorGradientStop GradientStop_0_AlmostMediumTurquoise_FF64C8D4()
            {
                return _c.CreateColorGradientStop(0F, Color.FromArgb(0xFF, 0x64, 0xC8, 0xD4));
            }

            // - - Shape tree root for layer: Shape
            // -  Offset:<100, 100>
            // Stop 1
            CompositionColorGradientStop GradientStop_1_AlmostSteelBlue_FF3370B8()
            {
                return _c.CreateColorGradientStop(1F, Color.FromArgb(0xFF, 0x33, 0x70, 0xB8));
            }

            // - Shape tree root for layer: Shape
            // Offset:<100, 100>
            CompositionLinearGradientBrush LinearGradientBrush()
            {
                var result = _c.CreateLinearGradientBrush();
                var colorStops = result.ColorStops;
                colorStops.Add(GradientStop_0_AlmostMediumTurquoise_FF64C8D4());
                colorStops.Add(GradientStop_1_AlmostSteelBlue_FF3370B8());
                result.MappingMode = CompositionMappingMode.Absolute;
                result.StartPoint = new Vector2(-4.375F, -6.5F);
                result.EndPoint = new Vector2(4.8119998F, 6F);
                return result;
            }

            CompositionPath Path()
            {
                if (_path != null) { return _path; }
                var result = _path = new CompositionPath(Geometry_0());
                return result;
            }

            // - Shape tree root for layer: Shape
            // Offset:<100, 100>
            // Path 2+Path 1.PathGeometry
            CompositionPathGeometry PathGeometry()
            {
                if (_pathGeometry != null) { return _pathGeometry; }
                var result = _pathGeometry = _c.CreatePathGeometry();
                return result;
            }

            // Shape tree root for layer: Shape
            // Path 2+Path 1
            CompositionSpriteShape SpriteShape()
            {
                // Offset:<100, 100>, Scale:<10, 10>
                var geometry = PathGeometry();
                var result = CreateSpriteShape(geometry, new Matrix3x2(10F, 0F, 0F, 10F, 100F, 100F), LinearGradientBrush()); ;
                return result;
            }

            // The root of the composition.
            ContainerVisual Root()
            {
                if (_root != null) { return _root; }
                var result = _root = _c.CreateContainerVisual();
                var propertySet = result.Properties;
                propertySet.InsertScalar("Progress", 0F);
                // Shape tree root for layer: Shape
                result.Children.InsertAtTop(ShapeVisual_0());
                return result;
            }

            CubicBezierEasingFunction CubicBezierEasingFunction_0()
            {
                return (_cubicBezierEasingFunction_0 == null)
                    ? _cubicBezierEasingFunction_0 = _c.CreateCubicBezierEasingFunction(new Vector2(0.600000024F, 0F), new Vector2(0.400000006F, 1F))
                    : _cubicBezierEasingFunction_0;
            }

            // - - Shape tree root for layer: Shape
            // -  Offset:<100, 100>
            // Path 2+Path 1.PathGeometry
            // Path
            PathKeyFrameAnimation PathKeyFrameAnimation_0()
            {
                // Frame 0.
                var result = CreatePathKeyFrameAnimation(0F, Path(), HoldThenStepEasingFunction());
                // Frame 10.
                result.InsertKeyFrame(0.333333343F, new CompositionPath(Geometry_3()), CubicBezierEasingFunction_0());
                // Frame 20.
                result.InsertKeyFrame(0.666666687F, new CompositionPath(Geometry_6()), CubicBezierEasingFunction_0());
                // Frame 29.
                result.InsertKeyFrame(0.966666639F, Path(), CubicBezierEasingFunction_0());
                return result;
            }

            // Shape tree root for layer: Shape
            ShapeVisual ShapeVisual_0()
            {
                var result = _c.CreateShapeVisual();
                result.Size = new Vector2(200F, 200F);
                // Offset:<100, 100>
                result.Shapes.Add(SpriteShape());
                return result;
            }

            // - - - Shape tree root for layer: Shape
            // - -  Offset:<100, 100>
            // - Path 2+Path 1.PathGeometry
            // Path
            StepEasingFunction HoldThenStepEasingFunction()
            {
                var result = _c.CreateStepEasingFunction();
                result.IsFinalStepSingleFrame = true;
                return result;
            }

            internal Location_AnimatedVisual_UAPv11(
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

            public void CreateAnimations()
            {
                _pathGeometry.StartAnimation("Path", PathKeyFrameAnimation_0());
                var controller = _pathGeometry.TryGetAnimationController("Path");
                controller.Pause();
                BindProperty(controller, "Progress", "_.Progress", "_", _root);
            }

            public void DestroyAnimations()
            {
                _pathGeometry.StopAnimation("Path");
            }

            internal static bool IsRuntimeCompatible()
            {
                return Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 11);
            }
        }

        sealed class Location_AnimatedVisual_UAPv7 : Microsoft.UI.Xaml.Controls.IAnimatedVisual
        {
            const long c_durationTicks = 5000000;
            readonly Compositor _c;
            readonly ExpressionAnimation _reusableExpressionAnimation;
            ContainerVisual _root;

            CompositionSpriteShape CreateSpriteShape(CompositionGeometry geometry, Matrix3x2 transformMatrix, CompositionBrush fillBrush)
            {
                var result = _c.CreateSpriteShape(geometry);
                result.TransformMatrix = transformMatrix;
                result.FillBrush = fillBrush;
                return result;
            }

            // - - - Shape tree root for layer: Shape
            // - -  Offset:<100, 100>
            // - Path 2+Path 1.PathGeometry
            CanvasGeometry Geometry_0()
            {
                var result = CanvasGeometry.CreateGroup(
                    null,
                    new CanvasGeometry[] { Geometry_1(), Geometry_2() },
                    CanvasFilledRegionDetermination.Winding);
                return result;
            }

            // - - - - Shape tree root for layer: Shape
            // - - -  Offset:<100, 100>
            // - - Path 2+Path 1.PathGeometry
            CanvasGeometry Geometry_1()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(-2.33299994F, -1.14300001F));
                    builder.AddCubicBezier(new Vector2(-2.33299994F, -2.40499997F), new Vector2(-1.28900003F, -3.4289999F), new Vector2(0F, -3.4289999F));
                    builder.AddCubicBezier(new Vector2(1.28900003F, -3.4289999F), new Vector2(2.33299994F, -2.40499997F), new Vector2(2.33299994F, -1.14300001F));
                    builder.AddCubicBezier(new Vector2(2.33299994F, 0.119000003F), new Vector2(1.28900003F, 1.14300001F), new Vector2(0F, 1.14300001F));
                    builder.AddCubicBezier(new Vector2(-1.28900003F, 1.14300001F), new Vector2(-2.33299994F, 0.119000003F), new Vector2(-2.33299994F, -1.14300001F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - - - - Shape tree root for layer: Shape
            // - - -  Offset:<100, 100>
            // - - Path 2+Path 1.PathGeometry
            CanvasGeometry Geometry_2()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(7F, -1.14300001F));
                    builder.AddCubicBezier(new Vector2(7F, -4.92999983F), new Vector2(3.86599994F, -8F), new Vector2(0F, -8F));
                    builder.AddCubicBezier(new Vector2(-3.86599994F, -8F), new Vector2(-7F, -4.92999983F), new Vector2(-7F, -1.14300001F));
                    builder.AddCubicBezier(new Vector2(-7F, 2.13400006F), new Vector2(-4.7420001F, 5.1500001F), new Vector2(-0.312999994F, 7.91099977F));
                    builder.AddCubicBezier(new Vector2(-0.122000001F, 8.02999973F), new Vector2(0.122000001F, 8.02999973F), new Vector2(0.312999994F, 7.91099977F));
                    builder.AddCubicBezier(new Vector2(4.7420001F, 5.1500001F), new Vector2(7F, 2.13400006F), new Vector2(7F, -1.14300001F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - - Shape tree root for layer: Shape
            // -  Offset:<100, 100>
            // Stop 0
            CompositionColorGradientStop GradientStop_0_AlmostMediumTurquoise_FF64C8D4()
            {
                return _c.CreateColorGradientStop(0F, Color.FromArgb(0xFF, 0x64, 0xC8, 0xD4));
            }

            // - - Shape tree root for layer: Shape
            // -  Offset:<100, 100>
            // Stop 1
            CompositionColorGradientStop GradientStop_1_AlmostSteelBlue_FF3370B8()
            {
                return _c.CreateColorGradientStop(1F, Color.FromArgb(0xFF, 0x33, 0x70, 0xB8));
            }

            // - Shape tree root for layer: Shape
            // Offset:<100, 100>
            CompositionLinearGradientBrush LinearGradientBrush()
            {
                var result = _c.CreateLinearGradientBrush();
                var colorStops = result.ColorStops;
                colorStops.Add(GradientStop_0_AlmostMediumTurquoise_FF64C8D4());
                colorStops.Add(GradientStop_1_AlmostSteelBlue_FF3370B8());
                result.MappingMode = CompositionMappingMode.Absolute;
                result.StartPoint = new Vector2(-4.375F, -6.5F);
                result.EndPoint = new Vector2(4.8119998F, 6F);
                return result;
            }

            // - Shape tree root for layer: Shape
            // Offset:<100, 100>
            // Path 2+Path 1.PathGeometry
            CompositionPathGeometry PathGeometry()
            {
                return _c.CreatePathGeometry(new CompositionPath(Geometry_0()));
            }

            // Shape tree root for layer: Shape
            // Path 2+Path 1
            CompositionSpriteShape SpriteShape()
            {
                // Offset:<100, 100>, Scale:<10, 10>
                var geometry = PathGeometry();
                var result = CreateSpriteShape(geometry, new Matrix3x2(10F, 0F, 0F, 10F, 100F, 100F), LinearGradientBrush()); ;
                return result;
            }

            // The root of the composition.
            ContainerVisual Root()
            {
                if (_root != null) { return _root; }
                var result = _root = _c.CreateContainerVisual();
                var propertySet = result.Properties;
                propertySet.InsertScalar("Progress", 0F);
                // Shape tree root for layer: Shape
                result.Children.InsertAtTop(ShapeVisual_0());
                return result;
            }

            // Shape tree root for layer: Shape
            ShapeVisual ShapeVisual_0()
            {
                var result = _c.CreateShapeVisual();
                result.Size = new Vector2(200F, 200F);
                // Offset:<100, 100>
                result.Shapes.Add(SpriteShape());
                return result;
            }

            internal Location_AnimatedVisual_UAPv7(
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

            public void CreateAnimations()
            {
            }

            public void DestroyAnimations()
            {
            }

            internal static bool IsRuntimeCompatible()
            {
                return Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);
            }
        }
    }
}
