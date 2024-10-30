//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using System.Numerics;
using Unigram.Navigation;

namespace Unigram.Controls
{
    public class PlaybackNextButton : GlyphButton
    {
        private CompositionSpriteShape _line;
        private CompositionSpriteShape _triangle1;
        private CompositionSpriteShape _triangle2;

        public PlaybackNextButton()
        {
            DefaultStyleKey = typeof(PlaybackNextButton);
            Click += OnClick;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (_triangle1 == null || _triangle2 == null)
            {
                return;
            }

            var easing = BootStrapper.Current.Compositor.CreateLinearEasingFunction();

            var scale1 = BootStrapper.Current.Compositor.CreateVector2KeyFrameAnimation();
            scale1.InsertKeyFrame(1, Vector2.One, easing);
            scale1.InsertKeyFrame(0, Vector2.Zero, easing);
            //scale1.Duration = TimeSpan.FromMilliseconds(500);

            var scale2 = BootStrapper.Current.Compositor.CreateVector2KeyFrameAnimation();
            scale2.InsertKeyFrame(1, Vector2.Zero, easing);
            scale2.InsertKeyFrame(0, Vector2.One, easing);
            //scale2.Duration = TimeSpan.FromMilliseconds(500);

            _triangle1.StartAnimation("Scale", scale1);
            _triangle2.StartAnimation("Scale", scale2);
        }

        protected override void OnApplyTemplate()
        {
            var w = 16;
            var h = 16;

            var back = IsPrevious;
            var target = GetTemplateChild("Target") as UIElement;

            var compositor = BootStrapper.Current.Compositor;

            var line = compositor.CreateLineGeometry();
            line.Start = new Vector2(back ? 2.5f : w - 2.5f, 2);
            line.End = new Vector2(back ? 2.5f : w - 2.5f, 14);

            var triangle = compositor.CreatePathGeometry(GetPolygon(
                new Vector2(back ? 13.5f : w - 13.5f, 3),
                new Vector2(back ? 13.5f : w - 13.5f, 13),
                new Vector2(back ? 6.5f : w - 6.5f, 8)));

            var lineShape = compositor.CreateSpriteShape(line);
            lineShape.StrokeThickness = 1;
            lineShape.StrokeBrush = compositor.CreateColorBrush(Colors.Black);

            var triangleShape1 = compositor.CreateSpriteShape(triangle);
            triangleShape1.StrokeThickness = 1;
            triangleShape1.StrokeBrush = compositor.CreateColorBrush(Colors.Black);
            triangleShape1.CenterPoint = new Vector2(back ? 2.5f : w - 2.5f, 8);
            triangleShape1.IsStrokeNonScaling = true;

            var triangleShape2 = compositor.CreateSpriteShape(triangle);
            triangleShape2.StrokeThickness = 1;
            triangleShape2.StrokeBrush = compositor.CreateColorBrush(Colors.Black);
            triangleShape2.CenterPoint = new Vector2(back ? 16 : w - 16, 8);
            triangleShape2.Scale = Vector2.Zero;
            triangleShape2.IsStrokeNonScaling = true;

            var test = compositor.CreateShapeVisual();
            test.Size = new Vector2(w, h);
            test.Shapes.Add(lineShape);
            test.Shapes.Add(triangleShape1);
            test.Shapes.Add(triangleShape2);

            _line = lineShape;
            _triangle1 = triangleShape1;
            _triangle2 = triangleShape2;

            ApplyForeground();
            RegisterPropertyChangedCallback(ForegroundProperty, OnForegroundChanged);

            ElementCompositionPreview.SetElementChildVisual(target, test);
        }

        private void OnForegroundChanged(DependencyObject sender, DependencyProperty dp)
        {
            ApplyForeground();
        }

        private void ApplyForeground()
        {
            if (Foreground is SolidColorBrush solid)
            {
                var brush = BootStrapper.Current.Compositor.CreateColorBrush(solid.Color);

                _line.StrokeBrush = brush;
                _triangle1.StrokeBrush = brush;
                _triangle2.StrokeBrush = brush;
            }
        }

        private CompositionPath GetPolygon(params Vector2[] pt)
        {
            CanvasGeometry result;
            using (var builder = new CanvasPathBuilder(null))
            {
                builder.BeginFigure(pt[0]);

                for (int i = 1; i < pt.Length; i++)
                {
                    builder.AddLine(pt[i]);
                }

                builder.EndFigure(CanvasFigureLoop.Closed);
                result = CanvasGeometry.CreatePath(builder);
            }

            return new CompositionPath(result);
        }

        #region IsPrevious

        public bool IsPrevious
        {
            get => (bool)GetValue(IsPreviousProperty);
            set => SetValue(IsPreviousProperty, value);
        }

        public static readonly DependencyProperty IsPreviousProperty =
            DependencyProperty.Register("IsPrevious", typeof(bool), typeof(PlaybackNextButton), new PropertyMetadata(false));

        #endregion
    }
}
