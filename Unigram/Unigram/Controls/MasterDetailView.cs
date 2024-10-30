//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using Unigram.Collections;
using Unigram.Navigation;
using Unigram.Navigation.Services;
using Unigram.Views;

namespace Unigram.Controls
{
    public sealed class MasterDetailView : ContentControl, IDisposable
    {
        private MasterDetailPanel AdaptivePanel;
        private Frame DetailFrame;
        private ContentPresenter MasterPresenter;
        private Grid DetailPresenter;
        private BreadcrumbBar DetailHeaderPresenter;
        private FrameworkElement BackgroundPart;
        private Border BorderPart;

        public NavigationService NavigationService { get; private set; }
        public Frame ParentFrame { get; private set; }

        private readonly MvxObservableCollection<NavigationStackItem> _backStack = new();
        private readonly NavigationStackItem _currentPage = new(null, null, null, false);

        private long _titleToken;

        public MasterDetailView()
        {
            DefaultStyleKey = typeof(MasterDetailView);

            Loaded += OnLoaded;
        }

        #region Initialize

        public void Initialize(string key, Frame parent, int session)
        {
            var service = WindowContext.Current.NavigationServices.GetByFrameId(key + session) as NavigationService;
            if (service == null)
            {
                service = BootStrapper.Current.NavigationServiceFactory(BootStrapper.BackButton.Ignore, BootStrapper.ExistingContent.Exclude, session, key + session, false) as NavigationService;
                service.Frame.DataContext = new object();
                service.FrameFacade.BackRequested += OnBackRequested;
                service.BackStackChanged += OnBackStackChanged;
            }

            NavigationService = service;
            DetailFrame = NavigationService.Frame;
            ParentFrame = parent;
        }

        public void Dispose()
        {
            Loaded -= OnLoaded;

            var service = NavigationService;
            if (service != null)
            {
                service.FrameFacade.BackRequested -= OnBackRequested;
                service.BackStackChanged -= OnBackStackChanged;
            }

            var panel = AdaptivePanel;
            if (panel != null)
            {
                panel.ViewStateChanged -= OnViewStateChanged;
            }

            var frame = DetailFrame;
            if (frame != null)
            {
                frame.Navigated -= OnNavigated;
            }
        }

        private void OnBackRequested(object sender, BackRequestedRoutedEventArgs args)
        {
            //var type = BackStackType.Navigation;
            //if (_backStack.Count > 0)
            //{
            //    type = _backStack.Last.Value;
            //    _backStack.RemoveLast();
            //}

            if (ParentFrame.Content is INavigatingPage masterPaging && CurrentState != MasterDetailState.Minimal)
            {
                masterPaging.OnBackRequesting(args);
                if (args.Handled)
                {
                    return;
                }
            }

            if (DetailFrame.Content is INavigablePage detailPage /*&& type == BackStackType.Navigation*/)
            {
                detailPage.OnBackRequested(args);
                if (args.Handled)
                {
                    return;
                }
            }

            // TODO: maybe checking for the actual width is not the perfect way,
            // but if it is 0 it means that the control is not loaded, and the event shouldn't be handled
            if (CanGoBack && ActualWidth > 0 /*&& type == BackStackType.Navigation*/)
            {
                DetailFrame.GoBack();
                args.Handled = true;
            }
            else if (ParentFrame.Content is INavigablePage masterPage /*&& type == BackStackType.Hamburger*/)
            {
                masterPage.OnBackRequested(args);
                if (args.Handled)
                {
                    return;
                }
            }
            else if (ParentFrame.CanGoBack && ActualWidth > 0)
            {
                ParentFrame.GoBack();
                args.Handled = true;
            }
        }

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (CurrentState != MasterDetailState.Minimal)
            {
                OnViewStateChanged();
            }
        }

        private void UpdateMasterVisibility()
        {
            if (CurrentState == MasterDetailState.Minimal && DetailFrame?.CurrentSourcePageType == BlankPageType)
            {
                MasterPresenter.Visibility = Visibility.Visible;
            }
            else if (CurrentState is MasterDetailState.Compact or MasterDetailState.Expanded)
            {
                MasterPresenter.Visibility = Visibility.Visible;
            }
            else
            {
                MasterPresenter.Visibility = Visibility.Collapsed;
            }
        }

        private bool _backgroundCollapsed;

        public void ShowHideBackground(bool show, bool animate)
        {
            if (_backgroundCollapsed != show || BackgroundPart == null)
            {
                return;
            }

            _backgroundCollapsed = !show;

            var visual = ElementCompositionPreview.GetElementVisual(BackgroundPart);
            var border = ElementCompositionPreview.GetElementVisual(BorderPart);
            var bread = ElementCompositionPreview.GetElementVisual(DetailHeaderPresenter);

            if (animate)
            {
                BackgroundPart.Visibility = Visibility.Visible;
                BorderPart.Visibility = Visibility.Visible;
                DetailHeaderPresenter.Visibility = Visibility.Visible;

                var batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, args) =>
                {
                    if (show)
                    {
                        _backgroundCollapsed = false;
                        DetailHeaderPresenter.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BackgroundPart.Visibility = Visibility.Collapsed;
                        BorderPart.Visibility = Visibility.Collapsed;
                    }
                };

                var opacity = visual.Compositor.CreateScalarKeyFrameAnimation();
                opacity.InsertKeyFrame(show ? 0 : 1, 0);
                opacity.InsertKeyFrame(show ? 1 : 0, 1);

                var fadeIn = visual.Compositor.CreateScalarKeyFrameAnimation();
                fadeIn.InsertKeyFrame(show ? 0 : 1, 1);
                fadeIn.InsertKeyFrame(show ? 1 : 0, 0);

                visual.StartAnimation("Opacity", opacity);
                border.StartAnimation("Opacity", opacity);
                bread.StartAnimation("Opacity", fadeIn);

                batch.End();
            }
            else
            {
                visual.Opacity = show ? 1 : 0;
                border.Opacity = show ? 1 : 0;
                bread.Opacity = show ? 0 : 1;

                BackgroundPart.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                BorderPart.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                DetailHeaderPresenter.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        protected override void OnApplyTemplate()
        {
            VisualStateManager.GoToState(this, "ResetState", false);

            MasterPresenter = GetTemplateChild("MasterFrame") as ContentPresenter;
            DetailPresenter = GetTemplateChild(nameof(DetailPresenter)) as Grid;
            DetailHeaderPresenter = GetTemplateChild(nameof(DetailHeaderPresenter)) as BreadcrumbBar;
            BackgroundPart = GetTemplateChild(nameof(BackgroundPart)) as FrameworkElement;
            BorderPart = GetTemplateChild(nameof(BorderPart)) as Border;
            AdaptivePanel = GetTemplateChild(nameof(AdaptivePanel)) as MasterDetailPanel;
            AdaptivePanel.ViewStateChanged += OnViewStateChanged;

            DetailHeaderPresenter.ItemsSource = _backStack;
            DetailHeaderPresenter.ItemClicked += DetailHeaderPresenter_ItemClicked;

            MasterPresenter.RegisterPropertyChangedCallback(VisibilityProperty, OnVisibilityChanged);

            BackgroundPart.Visibility = _backgroundCollapsed ? Visibility.Collapsed : Visibility.Visible;
            BorderPart.Visibility = _backgroundCollapsed ? Visibility.Collapsed : Visibility.Visible;

            var detailVisual = ElementCompositionPreview.GetElementVisual(DetailPresenter);
            detailVisual.Clip = BootStrapper.Current.Compositor.CreateInsetClip(0, -40, 0, 0);

            if (DetailFrame != null)
            {
                var parent = VisualTreeHelper.GetParent(DetailFrame) as UIElement;
                if (parent != null && parent != DetailPresenter)
                {
                    VisualTreeHelper.DisconnectChildrenRecursive(parent);
                }

                //Grid.SetRow(DetailFrame, 1);
                try
                {
                    DetailFrame.Navigated += OnNavigated;
                    DetailPresenter.Children.Add(DetailFrame);

                    if (DetailFrame.CurrentSourcePageType == null)
                    {
                        DetailFrame.Navigate(BlankPageType);
                    }
                    else
                    {
                        NavigationService.InsertToBackStack(0, BlankPageType);
                    }
                }
                catch { }
            }

            if (ActualWidth > 0 && CurrentState != MasterDetailState.Minimal)
            {
                OnViewStateChanged();
            }
        }

        private void DetailHeaderPresenter_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            var index = args.Index + 1;
            var count = _backStack.Count - 1;

            while (count - index > 0)
            {
                NavigationService.RemoveFromBackStack(DetailFrame.BackStackDepth - 1);
                count--;
            }

            NavigationService.GoBack();
        }

        private void OnVisibilityChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (MasterPresenter.Visibility == Visibility.Visible)
            {
                Update?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnNavigating(object sender, NavigatingEventArgs e)
        {
            if (e.Content is HostedPage hosted)
            {
                hosted.UnregisterPropertyChangedCallback(HostedPage.TitleProperty, _titleToken);
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is HostedPage hosted)
            {
                DetailHeader = hosted.Header;
                DetailFooter = hosted.Footer;

                if (hosted.Header == null)
                {
                    _titleToken = hosted.RegisterPropertyChangedCallback(HostedPage.TitleProperty, OnTitleChanged);

                    if (string.IsNullOrEmpty(hosted.Title))
                    {
                        _backStack.Clear();
                    }
                    else
                    {
                        _currentPage.Title = hosted.Title;

                        _backStack.ReplaceWith(BuildBackStack(hosted.IsNavigationRoot));
                        _backStack.Add(_currentPage);
                    }
                }
                else
                {
                    _backStack.Clear();
                }
            }
            else
            {
                DetailHeader = null;
                DetailFooter = null;

                _backStack.Clear();
            }

            if (AdaptivePanel == null)
            {
                return;
            }

            UpdateMasterVisibility();
        }

        private void OnTitleChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (sender is HostedPage hosted)
            {
                if (string.IsNullOrEmpty(hosted.Title))
                {
                    _backStack.Clear();
                }
                else if (_backStack.Count > 0)
                {
                    _currentPage.Title = hosted.Title;
                }
                else
                {
                    _currentPage.Title = hosted.Title;

                    _backStack.ReplaceWith(BuildBackStack(hosted.IsNavigationRoot));
                    _backStack.Add(_currentPage);
                }
            }
        }

        private void OnBackStackChanged(object sender, EventArgs e)
        {
            if (DetailFrame.Content is HostedPage hosted && hosted.Header == null)
            {
                _backStack.ReplaceWith(BuildBackStack(hosted.IsNavigationRoot));
                _backStack.Add(_currentPage);
            }
            else if (_backStack.Count > 0)
            {
                _backStack.Clear();
            }
        }

        private IEnumerable<NavigationStackItem> BuildBackStack(bool root)
        {
            if (root)
            {
                yield break;
            }

            var index = NavigationService.BackStack.FindLastIndex(x => x.IsRoot);
            var k = Math.Max(index, 0);

            for (int i = k; i < NavigationService.BackStack.Count; i++)
            {
                var item = NavigationService.BackStack[i];
                if (item.Title != null)
                {
                    yield return item;
                }
            }
        }

        private void OnViewStateChanged(object sender, EventArgs e)
        {
            OnViewStateChanged();
        }

        private void OnViewStateChanged()
        {
            VisualStateManager.GoToState(this, AdaptivePanel.CurrentState == MasterDetailState.Minimal ? "Minimal" : "Expanded", false);
            ViewStateChanged?.Invoke(this, EventArgs.Empty);

            UpdateMasterVisibility();
        }

        #region Public methods

        public bool CanGoBack
        {
            get
            {
                return DetailFrame.CanGoBack;

                // BEFORE BACK NAVIGATION IN FILLED (WIDE) STATE FIX.
                // return DetailFrame.CanGoBack && AdaptiveStates.CurrentState.Name == NarrowState;
            }
        }

        public MasterDetailState CurrentState
        {
            get
            {
                if (AdaptivePanel == null)
                {
                    return MasterDetailState.Expanded;
                }

                return AdaptivePanel.CurrentState;
            }
        }

        public event EventHandler ViewStateChanged;
        public event EventHandler Update;

        #endregion

        #region BlankType
        public Type BlankPageType
        {
            get => (Type)GetValue(BlankPageTypeProperty);
            set => SetValue(BlankPageTypeProperty, value);
        }

        public static readonly DependencyProperty BlankPageTypeProperty =
            DependencyProperty.Register("BlankPageType", typeof(Type), typeof(MasterDetailView), new PropertyMetadata(typeof(BlankPage)));
        #endregion

        #region AllowCompact

        public bool AllowCompact
        {
            get => AdaptivePanel?.AllowCompact ?? true;
            set
            {
                if (AdaptivePanel != null)
                {
                    AdaptivePanel.AllowCompact = value;
                }
            }
        }

        #endregion

        #region Banner

        public UIElement Banner
        {
            get => (UIElement)GetValue(PageHeaderProperty);
            set => SetValue(PageHeaderProperty, value);
        }

        public static readonly DependencyProperty PageHeaderProperty =
            DependencyProperty.Register("Banner", typeof(UIElement), typeof(MasterDetailView), new PropertyMetadata(null));

        #endregion

        #region DetailHeader

        public UIElement DetailHeader
        {
            get => (UIElement)GetValue(DetailHeaderProperty);
            set => SetValue(DetailHeaderProperty, value);
        }

        public static readonly DependencyProperty DetailHeaderProperty =
            DependencyProperty.Register("DetailHeader", typeof(UIElement), typeof(MasterDetailView), new PropertyMetadata(null));

        #endregion

        #region DetailFooter

        public UIElement DetailFooter
        {
            get { return (UIElement)GetValue(DetailFooterProperty); }
            set { SetValue(DetailFooterProperty, value); }
        }

        public static readonly DependencyProperty DetailFooterProperty =
            DependencyProperty.Register("DetailFooter", typeof(UIElement), typeof(MasterDetailView), new PropertyMetadata(null));

        #endregion
    }

    public enum MasterDetailState
    {
        Unknown,
        Minimal,
        Compact,
        Expanded
    }
}
