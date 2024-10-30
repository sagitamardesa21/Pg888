//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Telegram.Collections;
using Telegram.Common;
using Telegram.Controls;
using Telegram.Converters;
using Telegram.Native;
using Telegram.Navigation;
using Telegram.Navigation.Services;
using Telegram.Services;
using Telegram.Services.Settings;
using Telegram.Td.Api;
using Telegram.ViewModels;
using Telegram.Views.Authorization;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Point = Windows.Foundation.Point;

namespace Telegram.Views.Host
{
    public sealed partial class RootPage : Page
    {
        private readonly ILifetimeService _lifetime;
        private NavigationService _navigationService;
        private long? _canGoBackToken;

        private RootDestination _navigationViewSelected;
        private readonly MvxObservableCollection<object> _navigationViewItems;

        public RootPage(NavigationService service)
        {
            RequestedTheme = SettingsService.Current.Appearance.GetCalculatedElementTheme();
            InitializeComponent();

            _lifetime = TLContainer.Current.Lifetime;

            _navigationViewSelected = RootDestination.Chats;
            _navigationViewItems = new MvxObservableCollection<object>
            {
                RootDestination.Status,
                // ------------
                RootDestination.Separator,
                // ------------
                RootDestination.ArchivedChats,
                RootDestination.SavedMessages,
                // ------------
                RootDestination.Separator,
                // ------------
                RootDestination.Chats,
                RootDestination.Contacts,
                RootDestination.Calls,
                RootDestination.Settings,
                // ------------
                RootDestination.Separator,
                // ------------
                RootDestination.Tips,
                RootDestination.News
            };

            NavigationViewList.ItemsSource = _navigationViewItems;

            service.Frame.Navigating += OnNavigating;
            service.Frame.Navigated += OnNavigated;

            _canGoBackToken = service.Frame.RegisterPropertyChangedCallback(Frame.CanGoBackProperty, OnCanGoBackChanged);
            _navigationService = service;

            InitializeNavigation(service.Frame);
            InitializeLocalization();

            Navigation.Content = _navigationService.Frame;

            DropShadowEx.Attach(ThemeShadow);
        }

        public void PopupOpened()
        {
            if (_navigationService.Frame.Content is IRootContentPage content)
            {
                content.PopupOpened();
            }
        }

        public void PopupClosed()
        {
            if (_navigationService.Frame.Content is IRootContentPage content)
            {
                content.PopupClosed();
            }
        }

        public void UpdateComponent()
        {
            _contentLoaded = false;
            Resources.Clear();
            InitializeComponent();

            _navigationViewSelected = RootDestination.Chats;
            NavigationViewList.ItemsSource = _navigationViewItems;

            InitializeNavigation(_navigationService.Frame);
            InitializeLocalization();

            Switch(_lifetime.ActiveItem);
        }

        public void Create()
        {
            var premium = 0;
            var count = 0;

            foreach (var session in TLContainer.Current.Lifetime.Items)
            {
                if (session.Settings.UseTestDC)
                {
                    continue;
                }

                if (session.ClientService.Options.IsPremium)
                {
                    premium++;
                }

                count++;
            }

            var limit = 3;

            if (count >= limit + premium)
            {
                _navigationService.ShowLimitReached(new PremiumLimitTypeConnectedAccounts());
                return;
            }

            Switch(_lifetime.Create());
        }

        public void Switch(ISessionService session)
        {
            _lifetime.ActiveItem = session;

            if (_navigationService != null)
            {
                Destroy(_navigationService);
            }

            Navigation.IsPaneOpen = false;

            var service = WindowContext.Current.NavigationServices.GetByFrameId($"{session.Id}") as NavigationService;
            if (service == null)
            {
                service = BootStrapper.Current.NavigationServiceFactory(BootStrapper.BackButton.Attach, BootStrapper.ExistingContent.Exclude, new Frame(), session.Id, $"{session.Id}", true) as NavigationService;
                service.Frame.Navigating += OnNavigating;
                service.Frame.Navigated += OnNavigated;

                _canGoBackToken = service.Frame.RegisterPropertyChangedCallback(Frame.CanGoBackProperty, OnCanGoBackChanged);

                switch (session.ClientService.GetAuthorizationState())
                {
                    case AuthorizationStateReady:
                        service.Navigate(typeof(MainPage));
                        break;
                    case AuthorizationStateWaitPhoneNumber:
                    case AuthorizationStateWaitOtherDeviceConfirmation:
                        service.Navigate(typeof(AuthorizationPage));
                        service.AddToBackStack(typeof(BlankPage));
                        break;
                    case AuthorizationStateWaitCode:
                        service.Navigate(typeof(AuthorizationCodePage));
                        break;
                    case AuthorizationStateWaitEmailAddress:
                        service.Navigate(typeof(AuthorizationEmailAddressPage));
                        break;
                    case AuthorizationStateWaitEmailCode:
                        service.Navigate(typeof(AuthorizationEmailCodePage));
                        break;
                    case AuthorizationStateWaitRegistration:
                        service.Navigate(typeof(AuthorizationRegistrationPage));
                        break;
                    case AuthorizationStateWaitPassword:
                        service.Navigate(typeof(AuthorizationPasswordPage));
                        break;
                }

                //if (service is TLRootNavigationService rootService)
                //{
                //    rootService.Handle(session.ClientService.GetAuthorizationState());
                //}

                var counters = session.ClientService.GetUnreadCount(new ChatListMain());
                if (counters != null)
                {
                    session.Aggregator.Publish(counters.UnreadChatCount);
                    session.Aggregator.Publish(counters.UnreadMessageCount);
                }

                session.Aggregator.Publish(new UpdateConnectionState(session.ClientService.GetConnectionState()));
            }
            else
            {
                // TODO: This should actually __never__ happen.
            }

            _navigationService = service;
            Navigation.Content = service.Frame;
        }

        private void Destroy(NavigationService master)
        {
            if (master.Frame.Content is IRootContentPage content)
            {
                content.Root = null;
                content.Dispose();
            }

            var detail = WindowContext.Current.NavigationServices.GetByFrameId($"Main{master.FrameFacade.FrameId}");
            if (detail != null)
            {
                detail.Navigate(typeof(BlankPage));
                detail.ClearCache();
            }

            var corpus = WindowContext.Current.NavigationServices.GetByFrameId($"Profile{master.FrameFacade.FrameId}");
            if (corpus != null)
            {
                corpus.Navigate(typeof(BlankPage));
                corpus.ClearCache();
            }

            if (_canGoBackToken is long token)
            {
                _canGoBackToken = null;
                master.Frame.UnregisterPropertyChangedCallback(Frame.CanGoBackProperty, token);
            }

            master.Frame.Navigating -= OnNavigating;
            master.Frame.Navigated -= OnNavigated;
            master.Frame.Navigate(typeof(BlankPage));

            WindowContext.Current.NavigationServices.Remove(master);
            WindowContext.Current.NavigationServices.Remove(detail);
            WindowContext.Current.NavigationServices.Remove(corpus);
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_navigationService?.Frame?.Content is IRootContentPage content)
            {
                content.Root = null;
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is IRootContentPage content)
            {
                content.Root = this;
                SetPaneToggleButtonVisibility(content.EvaluatePaneToggleButtonVisibility());
                InitializeNavigation(sender as Frame);
            }
            else
            {
                SetPaneToggleButtonVisibility(_navigationService.CanGoBack);
            }
        }

        private void OnCanGoBackChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (_navigationService.Content is not IRootContentPage)
            {
                SetPaneToggleButtonVisibility(_navigationService.CanGoBack);
            }
        }

        private void InitializeLocalization()
        {
            //NavigationChats.Text = Strings.Additional.Chats;
            //NavigationAbout.Content = Strings.Additional.About;
            //NavigationNews.Text = Strings.Additional.News;
        }

        private void InitializeNavigation(Frame frame)
        {
            if (frame?.Content is MainPage main)
            {
                InitializeUser(main.ViewModel.ClientService);
                InitializeSessions(main.ViewModel.ClientService, SettingsService.Current.IsAccountsSelectorExpanded, _lifetime.Items);
            }
        }

        private async void InitializeUser(IClientService clientService)
        {
            var user = clientService.GetUser(clientService.Options.MyId);
            user ??= await clientService.SendAsync(new GetMe()) as User;

            if (user == null)
            {
                return;
            }

            Photo.SetUser(clientService, user, 48);
            NameLabel.Text = user.FullName();
#if DEBUG
            PhoneLabel.Text = "+42 --- --- ----";
#else
            if (clientService.Options.TestMode)
            {
                PhoneLabel.Text = "+42 --- --- ----";
            }
            else
            {
                PhoneLabel.Text = PhoneNumber.Format(user.PhoneNumber);
            }
#endif
            Expanded.IsChecked = SettingsService.Current.IsAccountsSelectorExpanded;
            Automation.SetToolTip(Accounts, SettingsService.Current.IsAccountsSelectorExpanded ? Strings.AccDescrHideAccounts : Strings.AccDescrShowAccounts);
        }

        private void InitializeSessions(bool show, IList<ISessionService> items)
        {
            if (_navigationService.Content is MainPage main)
            {
                InitializeSessions(main.ViewModel.ClientService, SettingsService.Current.IsAccountsSelectorExpanded, _lifetime.Items);
            }
        }

        private void InitializeSessions(IClientService clientService, bool show, IList<ISessionService> items)
        {
            for (int i = 0; i < _navigationViewItems.Count; i++)
            {
                if (_navigationViewItems[i] is ISessionService)
                {
                    _navigationViewItems.RemoveAt(i);

                    if (i < _navigationViewItems.Count && _navigationViewItems[i] is RootDestination.Separator)
                    {
                        _navigationViewItems.RemoveAt(i);
                    }

                    i--;
                }
                else if (_navigationViewItems[i] is RootDestination.AddAccount)
                {
                    _navigationViewItems.RemoveAt(i);

                    if (i < _navigationViewItems.Count && _navigationViewItems[i] is RootDestination.Separator)
                    {
                        _navigationViewItems.RemoveAt(i);
                    }

                    i--;
                }
            }

            var index = 2;

            if (clientService.IsPremium is false)
            {
                if (_navigationViewItems[0] is RootDestination.Status)
                {
                    _navigationViewItems.RemoveAt(0);
                    _navigationViewItems.RemoveAt(0);
                }

                index = 0;
            }
            else if (_navigationViewItems[0] is not RootDestination.Status)
            {
                _navigationViewItems.Insert(0, RootDestination.Separator);
                _navigationViewItems.Insert(0, RootDestination.Status);
            }

            if (SettingsService.Current.HideArchivedChats is false)
            {
                if (_navigationViewItems[index] is RootDestination.ArchivedChats)
                {
                    _navigationViewItems.RemoveAt(index);
                }
            }
            else if (_navigationViewItems[index] is not RootDestination.ArchivedChats)
            {
                _navigationViewItems.Insert(index, RootDestination.ArchivedChats);
            }

            if (show && items != null)
            {
                _navigationViewItems.Insert(0, RootDestination.Separator);

                if (items.Count < 4)
                {
                    _navigationViewItems.Insert(0, RootDestination.AddAccount);
                }

                if (items.Count > 1)
                {
                    foreach (var item in items.OrderByDescending(x => { int index = Array.IndexOf(SettingsService.Current.AccountsSelectorOrder, x.Id); return index < 0 ? x.Id : index; }))
                    {
                        _navigationViewItems.Insert(0, item);
                    }
                }
            }
        }

        #region Recycling

        private void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            if (args.Item is ISessionService && (args.ItemContainer == null || args.ItemContainer is Controls.NavigationViewItem || args.ItemContainer is Controls.NavigationViewItemSeparator))
            {
                args.ItemContainer = new ListViewItem();
                args.ItemContainer.Style = NavigationViewList.ItemContainerStyle;
                args.ItemContainer.ContentTemplate = Resources["SessionItemTemplate"] as DataTemplate;
                args.ItemContainer.ContextRequested += OnContextRequested;
            }
            else if (args.Item is RootDestination destination)
            {
                if (destination == RootDestination.Separator && args.ItemContainer is not Controls.NavigationViewItemSeparator)
                {
                    args.ItemContainer = new Controls.NavigationViewItemSeparator();
                }
                else if (destination != RootDestination.Separator && args.ItemContainer is not Controls.NavigationViewItem)
                {
                    args.ItemContainer = new Controls.NavigationViewItem();
                    args.ItemContainer.ContextRequested += OnContextRequested;
                }
            }

            args.IsContainerPrepared = true;
        }

        private void OnContextRequested(UIElement sender, Windows.UI.Xaml.Input.ContextRequestedEventArgs args)
        {
            var container = sender as ListViewItem;
            if (container.Content is ISessionService session && !session.IsActive)
            {

            }
            else if (container.Content is RootDestination.AddAccount)
            {
                var alt = Window.Current.CoreWindow.IsKeyDown(Windows.System.VirtualKey.Menu);
                var ctrl = Window.Current.CoreWindow.IsKeyDown(Windows.System.VirtualKey.Control);
                var shift = Window.Current.CoreWindow.IsKeyDown(Windows.System.VirtualKey.Shift);

                if (alt && !ctrl && shift)
                {
                    var flyout = new MenuFlyout();

                    flyout.CreateFlyoutItem(() => Switch(_lifetime.Create(test: false)), "Production Server", new FontIcon { Glyph = Icons.Globe });
                    flyout.CreateFlyoutItem(() => Switch(_lifetime.Create(test: true)), "Test Server", new FontIcon { Glyph = Icons.Bug });

                    args.ShowAt(flyout, container);
                }
            }
            else if (container.Content is RootDestination.ArchivedChats)
            {
                if (_navigationService.Content is MainPage page)
                {
                    var flyout = new MenuFlyout();

                    flyout.CreateFlyoutItem(ToggleArchive, Strings.ArchiveMoveToChatList, new FontIcon { Glyph = Icons.Expand });
                    flyout.CreateFlyoutItem(page.ViewModel.MarkFolderAsRead, ChatFolderViewModel.Archive, Strings.MarkAllAsRead, new FontIcon { Glyph = Icons.MarkAsRead });

                    args.ShowAt(flyout, container);
                }
            }
        }

        private void ToggleArchive()
        {
            Navigation.IsPaneOpen = false;

            if (_navigationService.Content is MainPage page)
            {
                page.ToggleArchive();
            }
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }

            UpdateContainerContent(args.ItemContainer, args.Item);
        }

        private void UpdateContainerContent(SelectorItem container, object item)
        {
            if (item is ISessionService session)
            {
                var content = container.ContentTemplateRoot as Grid;
                if (content == null)
                {
                    return;
                }

                var user = session.ClientService.GetUser(session.UserId);
                if (user == null)
                {
                    return;
                }

                var title = content.FindName("TitleLabel") as TextBlock;
                title.Text = user.FullName();

                var photo = content.Children[0] as ProfilePicture;
                photo.SetUser(session.ClientService, user, 28);

                var identity = content.FindName("Identity") as IdentityIcon;
                identity.SetStatus(session.ClientService, user);

                AutomationProperties.SetName(container, user.FullName());
            }
            else if (item is RootDestination destination && _navigationService.Content is MainPage page)
            {
                var content = container as Controls.NavigationViewItem;
                if (content != null)
                {
                    content.IsChecked = destination == _navigationViewSelected;
                }

                switch (destination)
                {
                    case RootDestination.AddAccount:
                        content.Text = Strings.AddAccount;
                        content.Glyph = Icons.PersonAdd;
                        break;

                    case RootDestination.Chats:
                        content.Text = Strings.FilterChats;
                        content.Glyph = Icons.ChatMultiple;
                        break;
                    case RootDestination.Contacts:
                        content.Text = Strings.Contacts;
                        content.Glyph = Icons.People;
                        break;
                    case RootDestination.Calls:
                        content.Text = Strings.Calls;
                        content.Glyph = Icons.Phone;
                        break;
                    case RootDestination.Settings:
                        content.Text = Strings.Settings;
                        content.Glyph = Icons.Settings;
                        break;

                    case RootDestination.ArchivedChats:
                        content.Text = Strings.ArchivedChats;
                        content.Glyph = Icons.Archive;
                        break;
                    case RootDestination.SavedMessages:
                        content.Text = Strings.SavedMessages;
                        content.Glyph = Icons.Bookmark;
                        break;

                    case RootDestination.Status:
                        if (page.ViewModel.ClientService.TryGetUser(page.ViewModel.ClientService.Options.MyId, out User user))
                        {
                            content.Text = user.EmojiStatus == null ? Strings.SetEmojiStatus : Strings.ChangeEmojiStatus;
                            content.Glyph = user.EmojiStatus == null ? Icons.EmojiAdd : Icons.EmojiEdit;
                        }
                        break;

                    case RootDestination.Tips:
                        content.Text = Strings.TelegramFeatures;
                        content.Glyph = Icons.QuestionCircle;
                        break;
                    case RootDestination.News:
                        content.Text = Strings.News;
                        content.Glyph = Icons.Megaphone;
                        break;
                }
            }
        }

        #endregion

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            SettingsService.Current.IsAccountsSelectorExpanded = !SettingsService.Current.IsAccountsSelectorExpanded;

            InitializeSessions(SettingsService.Current.IsAccountsSelectorExpanded, _lifetime.Items);
            Expanded.IsChecked = SettingsService.Current.IsAccountsSelectorExpanded;

            Automation.SetToolTip(Accounts, SettingsService.Current.IsAccountsSelectorExpanded ? Strings.AccDescrHideAccounts : Strings.AccDescrShowAccounts);
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ISessionService session)
            {
                if (session.IsActive)
                {
                    return;
                }

                Switch(session);
            }
            else if (e.ClickedItem is RootDestination destination)
            {
                if (destination == RootDestination.AddAccount)
                {
                    Create();
                }
                else if (_navigationService?.Frame?.Content is IRootContentPage content)
                {
                    content.NavigationView_ItemClick(destination);
                }
            }

            Navigation.IsPaneOpen = false;

            var scroll = NavigationViewList.GetScrollViewer();
            scroll?.ChangeView(null, 0, null, true);
        }

        #region Exposed

        public void PresentContent(UIElement element)
        {
            Transition.Child = element;
        }

        public void UpdateSessions()
        {
            InitializeSessions(SettingsService.Current.IsAccountsSelectorExpanded, _lifetime.Items);
        }

        private bool _paneToggleButtonVisible;

        public void SetPaneToggleButtonVisibility(bool visible)
        {
            if (_paneToggleButtonVisible != visible)
            {
                _paneToggleButtonVisible = visible;
                Navigation.PaneToggleButtonVisibility = visible
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public void SetSelectedIndex(RootDestination value)
        {
            _navigationViewSelected = value;

            void SetChecked(RootDestination destination, RootDestination target)
            {
                var selector = NavigationViewList.ContainerFromItem(_navigationViewItems.FirstOrDefault(x => x is RootDestination y && y == destination)) as Controls.NavigationViewItem;
                if (selector != null)
                {
                    selector.IsChecked = destination == target;
                }
            }

            SetChecked(RootDestination.Chats, value);
            SetChecked(RootDestination.Contacts, value);
            SetChecked(RootDestination.Calls, value);
            SetChecked(RootDestination.Settings, value);
        }

        #endregion

        public void ShowEditor(ThemeCustomInfo theme)
        {
            var resize = ThemePage == null;

            FindName("ThemePage");
            ThemePage.Load(theme);

            if (resize)
            {
                MainColumn.Width = new GridLength(ActualWidth, GridUnitType.Pixel);

                var view = ApplicationView.GetForCurrentView();
                var size = view.VisibleBounds;

                view.TryResizeView(new Size(size.Width + 320, size.Height));
                ApplicationView.PreferredLaunchViewSize = new Size(size.Width, size.Height);

                MainColumn.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        public void HideEditor()
        {
            UnloadObject(ThemePage);

            var view = ApplicationView.GetForCurrentView();
            view.TryResizeView(ApplicationView.PreferredLaunchViewSize);
        }

        private void Theme_Click(object sender, RoutedEventArgs e)
        {
            var animate = true;
            if (animate)
            {
                var bitmap = ScreenshotManager.Capture();
                Transition.Background = new ImageBrush { ImageSource = bitmap, AlignmentX = AlignmentX.Center, AlignmentY = AlignmentY.Center, RelativeTransform = new ScaleTransform { ScaleY = -1, CenterY = 0.5 } };

                var actualWidth = (float)ActualWidth;
                var actualHeight = (float)ActualHeight;

                var transform = Theme.TransformToVisual(this);
                var point = transform.TransformPoint(new Point()).ToVector2();

                var width = MathF.Max(actualWidth - point.X, actualHeight - point.Y);
                var diaginal = MathF.Sqrt((width * width) + (width * width));

                var device = CanvasDevice.GetSharedDevice();

                var rect1 = CanvasGeometry.CreateRectangle(device, 0, 0, ActualTheme == ElementTheme.Light ? actualWidth : 0, ActualTheme == ElementTheme.Light ? actualHeight : 0);

                var elli1 = CanvasGeometry.CreateCircle(device, point.X + 24, point.Y + 24, ActualTheme == ElementTheme.Dark ? 0 : diaginal);
                var group1 = CanvasGeometry.CreateGroup(device, new[] { elli1, rect1 }, CanvasFilledRegionDetermination.Alternate);

                var elli2 = CanvasGeometry.CreateCircle(device, point.X + 24, point.Y + 24, ActualTheme == ElementTheme.Dark ? diaginal : 0);
                var group2 = CanvasGeometry.CreateGroup(device, new[] { elli2, rect1 }, CanvasFilledRegionDetermination.Alternate);

                var visual = ElementCompositionPreview.GetElementVisual(Transition);
                var ellipse = visual.Compositor.CreatePathGeometry(new CompositionPath(group2));
                var clip = visual.Compositor.CreateGeometricClip(ellipse);

                visual.Clip = clip;

                var batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, args) =>
                {
                    visual.Clip = null;
                    Transition.Background = null;
                };

                CompositionEasingFunction ease;
                if (ActualTheme == ElementTheme.Dark)
                {
                    ease = visual.Compositor.CreateCubicBezierEasingFunction(new Vector2(.42f, 0), new Vector2(1, 1));
                }
                else
                {
                    ease = visual.Compositor.CreateCubicBezierEasingFunction(new Vector2(0, 0), new Vector2(.58f, 1));
                }

                var anim = visual.Compositor.CreatePathKeyFrameAnimation();
                anim.InsertKeyFrame(0, new CompositionPath(group2), ease);
                anim.InsertKeyFrame(1, new CompositionPath(group1), ease);
                anim.Duration = TimeSpan.FromMilliseconds(500);

                ellipse.StartAnimation("Path", anim);
                batch.End();
            }

            SettingsService.Current.Appearance.ForceNightMode = ActualTheme != ElementTheme.Dark;
            SettingsService.Current.Appearance.RequestedTheme = ActualTheme != ElementTheme.Dark
                ? TelegramTheme.Dark
                : TelegramTheme.Light;

            SettingsService.Current.Appearance.UpdateNightMode();
        }

        private void Theme_ActualThemeChanged(FrameworkElement sender, object args)
        {
            Theme.IsChecked = sender.ActualTheme == ElementTheme.Dark;
        }

        public bool IsPaneOpen
        {
            get => Navigation.IsPaneOpen;
            set => Navigation.IsPaneOpen = value;
        }

        private bool _isSidebarEnabled;

        public void SetSidebarEnabled(bool value)
        {
            if (_isSidebarEnabled != value)
            {
                _isSidebarEnabled = value;
            }
        }

        private void UpdateNavigation()
        {
            foreach (SelectorItem container in NavigationViewList.ItemsPanelRoot.Children)
            {
                UpdateContainerContent(container, container.Content);

                if (container.Content is RootDestination.Status)
                {
                    return;
                }
            }
        }

        private void Navigation_PaneOpening(SplitView sender, object args)
        {
            UpdateNavigation();

            if (_navigationService?.Content is MainPage main)
            {
                InitializeUser(main.ViewModel.ClientService);
            }

            Theme.Visibility = Visibility.Visible;
            Accounts.Visibility = Visibility.Visible;

            ElementCompositionPreview.SetIsTranslationEnabled(Info, true);

            var theme = ElementCompositionPreview.GetElementVisual(Theme);
            var photo = ElementCompositionPreview.GetElementVisual(Photo);
            var info = ElementCompositionPreview.GetElementVisual(Info);
            var accounts = ElementCompositionPreview.GetElementVisual(Accounts);
            var compositor = theme.Compositor;

            var ease = compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1.0f));

            var offset1 = compositor.CreateVector3KeyFrameAnimation();
            offset1.InsertKeyFrame(0, new Vector3(-40, 0, 0), ease);
            offset1.InsertKeyFrame(1, new Vector3(200, 0, 0), ease);
            offset1.Duration = TimeSpan.FromMilliseconds(350);

            var offset2 = compositor.CreateVector3KeyFrameAnimation();
            offset2.InsertKeyFrame(0, new Vector3(-8, 0, 0), ease);
            offset2.InsertKeyFrame(1, new Vector3(0, 0, 0), ease);
            offset2.Duration = TimeSpan.FromMilliseconds(350);

            var opacity = compositor.CreateScalarKeyFrameAnimation();
            opacity.InsertKeyFrame(0, 0, ease);
            opacity.InsertKeyFrame(1, 1, ease);
            opacity.Duration = TimeSpan.FromMilliseconds(350);

            var scale = compositor.CreateVector3KeyFrameAnimation();
            scale.InsertKeyFrame(0, new Vector3(28f / 48f, 28f / 48f, 0), ease);
            scale.InsertKeyFrame(1, new Vector3(1, 1, 0), ease);
            scale.Duration = TimeSpan.FromMilliseconds(350);

            var clip = compositor.CreateScalarKeyFrameAnimation();
            clip.InsertKeyFrame(0, 180, ease);
            clip.InsertKeyFrame(1, 0, ease);
            clip.Duration = TimeSpan.FromMilliseconds(350);

            theme.StartAnimation("Offset", offset1);
            theme.StartAnimation("Opacity", opacity);

            photo.CenterPoint = new Vector3(_isSidebarEnabled ? 24 : 0, 24, 0);
            photo.StartAnimation("Scale", scale);

            info.CenterPoint = new Vector3(0, 32, 0);
            info.StartAnimation("Scale", scale);
            info.StartAnimation("Opacity", opacity);
            info.StartAnimation("Translation", offset2);

            accounts.Clip = compositor.CreateInsetClip();
            accounts.Clip.StartAnimation("RightInset", clip);
        }

        private void Navigation_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            Theme.Visibility = Visibility.Visible;
            Accounts.Visibility = Visibility.Visible;

            ElementCompositionPreview.SetIsTranslationEnabled(Info, true);

            var batch = Window.Current.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, args) =>
            {
                Theme.Visibility = Visibility.Collapsed;
                Accounts.Visibility = Visibility.Collapsed;
            };

            var theme = ElementCompositionPreview.GetElementVisual(Theme);
            var photo = ElementCompositionPreview.GetElementVisual(Photo);
            var info = ElementCompositionPreview.GetElementVisual(Info);
            var accounts = ElementCompositionPreview.GetElementVisual(Accounts);
            var compositor = theme.Compositor;

            var ease = compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1.0f));
            var offset1 = compositor.CreateVector3KeyFrameAnimation();
            offset1.InsertKeyFrame(0, new Vector3(200, 0, 0), ease);
            offset1.InsertKeyFrame(1, new Vector3(-40, 0, 0), ease);
            offset1.Duration = TimeSpan.FromMilliseconds(120);

            var offset2 = compositor.CreateVector3KeyFrameAnimation();
            offset2.InsertKeyFrame(0, new Vector3(0, 0, 0), ease);
            offset2.InsertKeyFrame(1, new Vector3(-8, 0, 0), ease);
            offset2.Duration = TimeSpan.FromMilliseconds(120);

            var opacity = compositor.CreateScalarKeyFrameAnimation();
            opacity.InsertKeyFrame(0, 1, ease);
            opacity.InsertKeyFrame(1, 0, ease);
            opacity.Duration = TimeSpan.FromMilliseconds(120);

            var scale = compositor.CreateVector3KeyFrameAnimation();
            scale.InsertKeyFrame(0, new Vector3(1, 1, 0), ease);

            if (_navigationViewSelected == RootDestination.Settings && !_isSidebarEnabled)
            {
                scale.InsertKeyFrame(1, new Vector3(0, 0, 0), ease);
            }
            else
            {
                scale.InsertKeyFrame(1, new Vector3(28f / 48f, 28f / 48f, 0), ease);
            }

            scale.Duration = TimeSpan.FromMilliseconds(120);

            var clip = compositor.CreateScalarKeyFrameAnimation();
            clip.InsertKeyFrame(0, 0, ease);
            clip.InsertKeyFrame(1, 180, ease);
            clip.Duration = TimeSpan.FromMilliseconds(120);

            theme.StartAnimation("Offset", offset1);
            theme.StartAnimation("Opacity", opacity);

            photo.CenterPoint = new Vector3(_isSidebarEnabled ? 24 : 0, 24, 0);
            photo.StartAnimation("Scale", scale);

            info.CenterPoint = new Vector3(0, 32, 0);
            info.StartAnimation("Scale", scale);
            info.StartAnimation("Opacity", opacity);
            info.StartAnimation("Translation", offset2);

            accounts.Clip = compositor.CreateInsetClip();
            accounts.Clip.StartAnimation("RightInset", clip);

            batch.End();
        }

        private void OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items[0] is ISessionService)
            {
                NavigationViewList.CanReorderItems = true;
            }
            else
            {
                NavigationViewList.CanReorderItems = false;
                e.Cancel = true;
            }
        }

        private void OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            NavigationViewList.CanReorderItems = false;

            if (args.DropResult == DataPackageOperation.Move && args.Items.Count == 1 && args.Items[0] is ISessionService session)
            {
                var items = _navigationViewItems;
                var index = items.IndexOf(session);

                var compare = items[index > 0 ? index - 1 : index + 1];
                if (compare is ISessionService)
                {
                    var sessions = _navigationViewItems.OfType<ISessionService>();
                    var ids = sessions.Select(x => x.Id);

                    SettingsService.Current.AccountsSelectorOrder = ids.ToArray();
                }
                else
                {
                    InitializeSessions(SettingsService.Current.IsAccountsSelectorExpanded, _lifetime.Items);
                }
            }
        }

        private void Navigation_BackRequested(Controls.NavigationView sender, object args)
        {
            if (_navigationService?.Frame?.Content is IRootContentPage content)
            {
                content.BackRequested();
            }
            else if (_navigationService != null && _navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
        }

        private void Photo_Click(object sender, RoutedEventArgs e)
        {
            IsPaneOpen = false;
        }
    }

    public interface IRootContentPage
    {
        RootPage Root { get; set; }

        void NavigationView_ItemClick(RootDestination destination);

        bool EvaluatePaneToggleButtonVisibility();

        void BackRequested();

        void Dispose();

        void PopupOpened();
        void PopupClosed();
    }

    public enum RootDestination
    {
        AddAccount,

        Status,

        ArchivedChats,
        SavedMessages,

        Chats,
        Contacts,
        Calls,
        Settings,

        Tips,
        News,

        Separator
    }
}
