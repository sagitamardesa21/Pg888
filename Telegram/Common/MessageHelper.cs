//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Controls;
using Telegram.Converters;
using Telegram.Native;
using Telegram.Navigation;
using Telegram.Navigation.Services;
using Telegram.Services;
using Telegram.Td.Api;
using Telegram.ViewModels;
using Telegram.Views;
using Telegram.Views.Folders;
using Telegram.Views.Folders.Popups;
using Telegram.Views.Host;
using Telegram.Views.Popups;
using Telegram.Views.Settings;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources.Core;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Point = Windows.Foundation.Point;
using User = Telegram.Td.Api.User;

namespace Telegram.Common
{
    public class MessageHelper
    {
        public static bool IsAnyCharacterRightToLeft(string s)
        {
            if (s == null)
            {
                return false;
            }

            //if (s.Length > 2)
            //{
            //    s = s.Substring(s.Length - 2);
            //}

            for (int i = 0; i < s.Length; i += char.IsSurrogatePair(s, i) ? 2 : 1)
            {
                var codepoint = char.ConvertToUtf32(s, i);
                if (IsRandALCat(codepoint))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRandALCat(int c)
        {
            bool hasRandALCat = false;
            if (c is >= 0x5BE and <= 0x10B7F)
            {
                if (c <= 0x85E)
                {
                    if (c == 0x5BE) hasRandALCat = true;
                    else if (c == 0x5C0) hasRandALCat = true;
                    else if (c == 0x5C3) hasRandALCat = true;
                    else if (c == 0x5C6) hasRandALCat = true;
                    else if (c is >= 0x5D0 and <= 0x5EA) hasRandALCat = true;
                    else if (c is >= 0x5F0 and <= 0x5F4) hasRandALCat = true;
                    else if (c == 0x608) hasRandALCat = true;
                    else if (c == 0x60B) hasRandALCat = true;
                    else if (c == 0x60D) hasRandALCat = true;
                    else if (c == 0x61B) hasRandALCat = true;
                    else if (c is >= 0x61E and <= 0x64A) hasRandALCat = true;
                    else if (c is >= 0x66D and <= 0x66F) hasRandALCat = true;
                    else if (c is >= 0x671 and <= 0x6D5) hasRandALCat = true;
                    else if (c is >= 0x6E5 and <= 0x6E6) hasRandALCat = true;
                    else if (c is >= 0x6EE and <= 0x6EF) hasRandALCat = true;
                    else if (c is >= 0x6FA and <= 0x70D) hasRandALCat = true;
                    else if (c == 0x710) hasRandALCat = true;
                    else if (c is >= 0x712 and <= 0x72F) hasRandALCat = true;
                    else if (c is >= 0x74D and <= 0x7A5) hasRandALCat = true;
                    else if (c == 0x7B1) hasRandALCat = true;
                    else if (c is >= 0x7C0 and <= 0x7EA) hasRandALCat = true;
                    else if (c is >= 0x7F4 and <= 0x7F5) hasRandALCat = true;
                    else if (c == 0x7FA) hasRandALCat = true;
                    else if (c is >= 0x800 and <= 0x815) hasRandALCat = true;
                    else if (c == 0x81A) hasRandALCat = true;
                    else if (c == 0x824) hasRandALCat = true;
                    else if (c == 0x828) hasRandALCat = true;
                    else if (c is >= 0x830 and <= 0x83E) hasRandALCat = true;
                    else if (c is >= 0x840 and <= 0x858) hasRandALCat = true;
                    else if (c == 0x85E) hasRandALCat = true;
                }
                else if (c == 0x200F) hasRandALCat = true;
                else if (c >= 0xFB1D)
                {
                    if (c == 0xFB1D) hasRandALCat = true;
                    else if (c is >= 0xFB1F and <= 0xFB28) hasRandALCat = true;
                    else if (c is >= 0xFB2A and <= 0xFB36) hasRandALCat = true;
                    else if (c is >= 0xFB38 and <= 0xFB3C) hasRandALCat = true;
                    else if (c == 0xFB3E) hasRandALCat = true;
                    else if (c is >= 0xFB40 and <= 0xFB41) hasRandALCat = true;
                    else if (c is >= 0xFB43 and <= 0xFB44) hasRandALCat = true;
                    else if (c is >= 0xFB46 and <= 0xFBC1) hasRandALCat = true;
                    else if (c is >= 0xFBD3 and <= 0xFD3D) hasRandALCat = true;
                    else if (c is >= 0xFD50 and <= 0xFD8F) hasRandALCat = true;
                    else if (c is >= 0xFD92 and <= 0xFDC7) hasRandALCat = true;
                    else if (c is >= 0xFDF0 and <= 0xFDFC) hasRandALCat = true;
                    else if (c is >= 0xFE70 and <= 0xFE74) hasRandALCat = true;
                    else if (c is >= 0xFE76 and <= 0xFEFC) hasRandALCat = true;
                    else if (c is >= 0x10800 and <= 0x10805) hasRandALCat = true;
                    else if (c == 0x10808) hasRandALCat = true;
                    else if (c is >= 0x1080A and <= 0x10835) hasRandALCat = true;
                    else if (c is >= 0x10837 and <= 0x10838) hasRandALCat = true;
                    else if (c == 0x1083C) hasRandALCat = true;
                    else if (c is >= 0x1083F and <= 0x10855) hasRandALCat = true;
                    else if (c is >= 0x10857 and <= 0x1085F) hasRandALCat = true;
                    else if (c is >= 0x10900 and <= 0x1091B) hasRandALCat = true;
                    else if (c is >= 0x10920 and <= 0x10939) hasRandALCat = true;
                    else if (c == 0x1093F) hasRandALCat = true;
                    else if (c == 0x10A00) hasRandALCat = true;
                    else if (c is >= 0x10A10 and <= 0x10A13) hasRandALCat = true;
                    else if (c is >= 0x10A15 and <= 0x10A17) hasRandALCat = true;
                    else if (c is >= 0x10A19 and <= 0x10A33) hasRandALCat = true;
                    else if (c is >= 0x10A40 and <= 0x10A47) hasRandALCat = true;
                    else if (c is >= 0x10A50 and <= 0x10A58) hasRandALCat = true;
                    else if (c is >= 0x10A60 and <= 0x10A7F) hasRandALCat = true;
                    else if (c is >= 0x10B00 and <= 0x10B35) hasRandALCat = true;
                    else if (c is >= 0x10B40 and <= 0x10B55) hasRandALCat = true;
                    else if (c is >= 0x10B58 and <= 0x10B72) hasRandALCat = true;
                    else if (c is >= 0x10B78 and <= 0x10B7F) hasRandALCat = true;
                }
            }

            return hasRandALCat;
        }

        public static bool TryCreateUri(string url, out Uri uri)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return true;
            }

            return Uri.TryCreate("http://" + url, UriKind.Absolute, out uri);
        }

        public static bool IsTelegramUrl(Uri uri)
        {
            var host = uri.Host;

            var splitHostName = uri.Host.Split('.');
            if (splitHostName.Length >= 2)
            {
                host = splitHostName[splitHostName.Length - 2] + "." +
                       splitHostName[splitHostName.Length - 1];
            }

            if (Constants.TelegramHosts.Contains(host))
            {
                return true;
            }

            return IsTelegramScheme(uri);
        }

        public static bool IsTelegramScheme(Uri uri)
        {
            return string.Equals(uri.Scheme, "tg", StringComparison.OrdinalIgnoreCase);
        }

        public static async void OpenTelegramUrl(IClientService clientService, INavigationService navigation, Uri uri)
        {
            var url = uri.ToString();
            if (url.Contains("telegra.ph"))
            {
                navigation.NavigateToInstant(url);
                return;
            }

            var response = await clientService.SendAsync(new GetInternalLinkType(url));
            if (response is InternalLinkType internalLink)
            {
                OpenTelegramUrl(clientService, navigation, internalLink);
            }
        }

        public static void OpenTelegramUrl(IClientService clientService, INavigationService navigation, InternalLinkType internalLink)
        {
            if (internalLink is InternalLinkTypeActiveSessions)
            {
                navigation.Navigate(typeof(SettingsSessionsPage));
            }
            else if (internalLink is InternalLinkTypeAuthenticationCode authenticationCode)
            {
                var state = clientService.GetAuthorizationState();
                if (state is AuthorizationStateWaitCode)
                {
                    clientService.Send(new CheckAuthenticationCode(authenticationCode.Code));
                }
            }
            else if (internalLink is InternalLinkTypeBackground background)
            {
                NavigateToBackground(clientService, navigation, background.BackgroundName);
            }
            else if (internalLink is InternalLinkTypeBotStart botStart)
            {
                NavigateToBotStart(clientService, navigation, botStart.BotUsername, botStart.StartParameter, botStart.Autostart, false);
            }
            else if (internalLink is InternalLinkTypeBotStartInGroup botStartInGroup)
            {
                // Not yet supported: AdministratorRights
                NavigateToBotStart(clientService, navigation, botStartInGroup.BotUsername, botStartInGroup.StartParameter, false, true);
            }
            else if (internalLink is InternalLinkTypeChangePhoneNumber)
            {
                navigation.Navigate(typeof(SettingsProfilePage));
            }
            else if (internalLink is InternalLinkTypeChatInvite chatInvite)
            {
                NavigateToInviteLink(clientService, navigation, chatInvite.InviteLink);
            }
            else if (internalLink is InternalLinkTypeChatFolderInvite chatFolderInvite)
            {
                NavigateToChatFolderInviteLink(clientService, navigation, chatFolderInvite.InviteLink);
            }
            else if (internalLink is InternalLinkTypeChatFolderSettings)
            {
                navigation.Navigate(typeof(FoldersPage));
            }
            else if (internalLink is InternalLinkTypeGame game)
            {
                NavigateToUsername(clientService, navigation, game.BotUsername, null, game.GameShortName);
            }
            else if (internalLink is InternalLinkTypeInstantView instantView)
            {
                navigation.NavigateToInstant(instantView.Url);
            }
            else if (internalLink is InternalLinkTypeInvoice invoice)
            {
                NavigateToInvoice(navigation, invoice.InvoiceName);
            }
            else if (internalLink is InternalLinkTypeLanguagePack languagePack)
            {
                NavigateToLanguage(clientService, navigation, languagePack.LanguagePackId);
            }
            else if (internalLink is InternalLinkTypeMessage message)
            {
                NavigateToMessage(clientService, navigation, message.Url);
            }
            else if (internalLink is InternalLinkTypeMessageDraft messageDraft)
            {
                NavigateToShare(messageDraft.Text, messageDraft.ContainsLink);
            }
            else if (internalLink is InternalLinkTypePassportDataRequest)
            {

            }
            else if (internalLink is InternalLinkTypePremiumFeatures premiumFeatures)
            {
                navigation.ShowPromo(new PremiumSourceLink(premiumFeatures.Referrer));
            }
            else if (internalLink is InternalLinkTypePrivacyAndSecuritySettings)
            {
                navigation.Navigate(typeof(SettingsPrivacyAndSecurityPage));
            }
            else if (internalLink is InternalLinkTypePhoneNumberConfirmation phoneNumberConfirmation)
            {
                NavigateToConfirmPhone(clientService, phoneNumberConfirmation.PhoneNumber, phoneNumberConfirmation.Hash);
            }
            else if (internalLink is InternalLinkTypeProxy proxy)
            {
                NavigateToProxy(clientService, proxy.Server, proxy.Port, proxy.Type);
            }
            else if (internalLink is InternalLinkTypePublicChat publicChat)
            {
                NavigateToUsername(clientService, navigation, publicChat.ChatUsername, null, null);
            }
            else if (internalLink is InternalLinkTypeQrCodeAuthentication)
            {

            }
            else if (internalLink is InternalLinkTypeSettings)
            {

            }
            else if (internalLink is InternalLinkTypeStickerSet stickerSet)
            {
                NavigateToStickerSet(stickerSet.StickerSetName);
            }
            else if (internalLink is InternalLinkTypeTheme theme)
            {
                NavigateToTheme(clientService, theme.ThemeName);
            }
            else if (internalLink is InternalLinkTypeThemeSettings)
            {
                navigation.Navigate(typeof(SettingsAppearancePage));
            }
            else if (internalLink is InternalLinkTypeUnknownDeepLink unknownDeepLink)
            {
                NavigateToUnknownDeepLink(clientService, unknownDeepLink.Link);
            }
            else if (internalLink is InternalLinkTypeUserPhoneNumber phoneNumber)
            {
                NavigateToPhoneNumber(clientService, navigation, phoneNumber.PhoneNumber);
            }
            else if (internalLink is InternalLinkTypeUserToken userToken)
            {
                NavigateToUserToken(clientService, navigation, userToken.Token);
            }
            else if (internalLink is InternalLinkTypeVideoChat videoChat)
            {
                NavigateToUsername(clientService, navigation, videoChat.ChatUsername, videoChat.InviteHash, null);
            }
            else if (internalLink is InternalLinkTypeWebApp webApp)
            {
                NavigateToWebApp(clientService, navigation, webApp.BotUsername, webApp.StartParameter, webApp.WebAppShortName);
            }
        }

        private static async void NavigateToWebApp(IClientService clientService, INavigationService navigation, string botUsername, string startParameter, string webAppShortName)
        {
            var response = await clientService.SendAsync(new SearchPublicChat(botUsername));
            if (response is Chat chat && clientService.TryGetUser(chat, out User user))
            {
                if (user.Type is not UserTypeBot)
                {
                    return;
                }

                var responss = await clientService.SendAsync(new SearchWebApp(user.Id, webAppShortName));
                if (responss is FoundWebApp foundWebApp)
                {
                    // TODO: confirmation
                    return;

                    var responsa = await clientService.SendAsync(new GetWebAppLinkUrl(0, user.Id, webAppShortName, startParameter, Theme.Current.Parameters, Strings.AppName, false));
                    if (responsa is HttpUrl url)
                    {
                        await new WebBotPopup(user, url.Url).ShowQueuedAsync();
                    }
                }
                else
                {
                    // TODO: error
                }
            }
        }

        private static async void NavigateToUnknownDeepLink(IClientService clientService, string url)
        {
            var response = await clientService.SendAsync(new GetDeepLinkInfo(url));
            if (response is DeepLinkInfo info)
            {
                var confirm = await MessagePopup.ShowAsync(info.Text, Strings.AppName, Strings.OK, info.NeedUpdateApplication ? Strings.UpdateApp : null);
                if (confirm == ContentDialogResult.Secondary)
                {
                    await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?PFN=" + Package.Current.Id.FamilyName));
                }
            }
        }

        private static async void NavigateToBackground(IClientService clientService, INavigationService navigation, string slug)
        {
            await navigation.ShowPopupAsync(typeof(BackgroundPopup), new BackgroundParameters(slug));

            //var response = await clientService.SendAsync(new SearchBackground(slug));
            //if (response is Background background)
            //{

            //}
        }

        private static async void NavigateToMessage(IClientService clientService, INavigationService navigation, string url)
        {
            var response = await clientService.SendAsync(new GetMessageLinkInfo(url));
            if (response is MessageLinkInfo info && info.ChatId != 0)
            {
                if (info.Message != null)
                {
                    if (info.MessageThreadId != 0)
                    {
                        navigation.NavigateToThread(info.ChatId, info.Message.Id, message: info.Message.Id);
                    }
                    else
                    {
                        navigation.NavigateToChat(info.ChatId, message: info.Message.Id);
                    }
                }
                else
                {
                    navigation.NavigateToChat(info.ChatId);
                }
            }
            else
            {
                await MessagePopup.ShowAsync(Strings.LinkNotFound, Strings.AppName, Strings.OK);
            }
        }

        private static async void NavigateToTheme(IClientService clientService, string slug)
        {
            await MessagePopup.ShowAsync(Strings.ThemeNotSupported, Strings.Theme, Strings.OK);
        }

        private static void NavigateToInvoice(INavigationService navigation, string invoiceName)
        {
            navigation.NavigateToInvoice(new InputInvoiceName(invoiceName));
        }

        public static async void NavigateToLanguage(IClientService clientService, INavigationService navigation, string languagePackId)
        {
            var response = await clientService.SendAsync(new GetLanguagePackInfo(languagePackId));
            if (response is LanguagePackInfo info)
            {
                if (info.Id == SettingsService.Current.LanguagePackId)
                {
                    var confirm = await MessagePopup.ShowAsync(string.Format(Strings.LanguageSame, info.Name), Strings.Language, Strings.OK, Strings.Settings);
                    if (confirm != ContentDialogResult.Secondary)
                    {
                        return;
                    }

                    navigation.Navigate(typeof(SettingsLanguagePage));
                }
                else if (info.TotalStringCount == 0)
                {
                    await MessagePopup.ShowAsync(string.Format(Strings.LanguageUnknownCustomAlert, info.Name), Strings.LanguageUnknownTitle, Strings.OK);
                }
                else
                {
                    var message = info.IsOfficial
                        ? Strings.LanguageAlert
                        : Strings.LanguageCustomAlert;

                    var start = message.IndexOf('[');
                    var end = message.IndexOf(']');
                    if (start != -1 && end != -1)
                    {
                        message = message.Insert(end + 1, $"({info.TranslationUrl})");
                    }

                    var confirm = await MessagePopup.ShowAsync(string.Format(message, info.Name, (int)Math.Ceiling(info.TranslatedStringCount / (float)info.TotalStringCount * 100)), Strings.LanguageTitle, Strings.Change, Strings.Cancel);
                    if (confirm != ContentDialogResult.Primary)
                    {
                        return;
                    }

                    var set = await LocaleService.Current.SetLanguageAsync(info, true);
                    if (set is Ok)
                    {
                        //ApplicationLanguages.PrimaryLanguageOverride = info.Id;
                        //ResourceContext.GetForCurrentView().Reset();
                        //ResourceContext.GetForViewIndependentUse().Reset();

                        //TLWindowContext.Current.NavigationServices.Remove(NavigationService);
                        //BootStrapper.Current.NavigationService.Reset();

                        foreach (var window in WindowContext.ActiveWrappers)
                        {
                            window.Dispatcher.Dispatch(() =>
                            {
                                ResourceContext.GetForCurrentView().Reset();
                                ResourceContext.GetForViewIndependentUse().Reset();

                                if (window.Content is RootPage root)
                                {
                                    window.Dispatcher.Dispatch(() =>
                                    {
                                        root.UpdateComponent();
                                    });
                                }
                            });
                        }
                    }
                }
            }
        }

        public static async void NavigateToSendCode(IClientService clientService, string phoneCode)
        {
            var state = clientService.GetAuthorizationState();
            if (state is AuthorizationStateWaitCode)
            {
                if (clientService.Options.TryGetValue("x_firstname", out string firstValue))
                {
                }

                if (clientService.Options.TryGetValue("x_lastname", out string lastValue))
                {
                }

                var response = await clientService.SendAsync(new CheckAuthenticationCode(phoneCode));
                if (response is Error error)
                {
                    if (error.MessageEquals(ErrorType.PHONE_NUMBER_INVALID))
                    {
                        await MessagePopup.ShowAsync(error.Message, Strings.InvalidPhoneNumber, Strings.OK);
                    }
                    else if (error.MessageEquals(ErrorType.PHONE_CODE_EMPTY) || error.MessageEquals(ErrorType.PHONE_CODE_INVALID))
                    {
                        await MessagePopup.ShowAsync(error.Message, Strings.InvalidCode, Strings.OK);
                    }
                    else if (error.MessageEquals(ErrorType.PHONE_CODE_EXPIRED))
                    {
                        await MessagePopup.ShowAsync(error.Message, Strings.CodeExpired, Strings.OK);
                    }
                    else if (error.MessageEquals(ErrorType.FIRSTNAME_INVALID))
                    {
                        await MessagePopup.ShowAsync(error.Message, Strings.InvalidFirstName, Strings.OK);
                    }
                    else if (error.MessageEquals(ErrorType.LASTNAME_INVALID))
                    {
                        await MessagePopup.ShowAsync(error.Message, Strings.InvalidLastName, Strings.OK);
                    }
                    else if (error.Message.StartsWith("FLOOD_WAIT"))
                    {
                        await MessagePopup.ShowAsync(Strings.FloodWait, Strings.AppName, Strings.OK);
                    }
                    else if (error.Code != -1000)
                    {
                        await MessagePopup.ShowAsync(error.Message, Strings.AppName, Strings.OK);
                    }

                    Logs.Logger.Warning(Logs.LogTarget.API, "account.signIn error " + error);
                }
            }
            else
            {
                if (phoneCode.Length > 3)
                {
                    phoneCode = phoneCode.Substring(0, 3) + "-" + phoneCode.Substring(3);
                }

                await MessagePopup.ShowAsync(string.Format(Strings.OtherLoginCode, phoneCode), Strings.AppName, Strings.OK);
            }
        }

        public static async void NavigateToShare(FormattedText text, bool hasUrl)
        {
            await SharePopup.GetForCurrentView().ShowAsync(text);
        }

        public static async void NavigateToProxy(IClientService clientService, string server, int port, ProxyType type)
        {
            string userText = string.Empty;
            string passText = string.Empty;
            string secretText = string.Empty;
            string secretInfo = string.Empty;

            if (type is ProxyTypeHttp http)
            {
                userText = !string.IsNullOrEmpty(http.Username) ? $"{Strings.UseProxyUsername}: {http.Username}\n" : string.Empty;
                passText = !string.IsNullOrEmpty(http.Password) ? $"{Strings.UseProxyPassword}: {http.Password}\n" : string.Empty;
            }
            else if (type is ProxyTypeSocks5 socks5)
            {
                userText = !string.IsNullOrEmpty(socks5.Username) ? $"{Strings.UseProxyUsername}: {socks5.Username}\n" : string.Empty;
                passText = !string.IsNullOrEmpty(socks5.Password) ? $"{Strings.UseProxyPassword}: {socks5.Password}\n" : string.Empty;
            }
            else if (type is ProxyTypeMtproto mtproto)
            {
                secretText = !string.IsNullOrEmpty(mtproto.Secret) ? $"{Strings.UseProxySecret}: {mtproto.Secret}\n" : string.Empty;
                secretInfo = !string.IsNullOrEmpty(mtproto.Secret) ? $"\n\n{Strings.UseProxyTelegramInfo2}" : string.Empty;
            }

            var confirm = await MessagePopup.ShowAsync($"{Strings.EnableProxyAlert}\n\n{Strings.UseProxyAddress}: {server}\n{Strings.UseProxyPort}: {port}\n{userText}{passText}{secretText}\n{Strings.EnableProxyAlert2}{secretInfo}", Strings.Proxy, Strings.ConnectingConnectProxy, Strings.Cancel);
            if (confirm == ContentDialogResult.Primary)
            {
                clientService.Send(new AddProxy(server ?? string.Empty, port, true, type));
            }
        }

        public static async void NavigateToConfirmPhone(IClientService clientService, string phone, string hash)
        {
            //var response = await clientService.SendConfirmPhoneCodeAsync(hash, false);
            //if (response.IsSucceeded)
            //{
            //    var state = new SignInSentCodePage.NavigationParameters
            //    {
            //        PhoneNumber = phone,
            //        //Result = response.Result,
            //    };

            //    App.Current.NavigationService.Navigate(typeof(SignInSentCodePage), state);

            //    //Telegram.Api.Helpers.Execute.BeginOnUIThread(delegate
            //    //{
            //    //    if (frame != null)
            //    //    {
            //    //        frame.CloseBlockingProgress();
            //    //    }
            //    //    TelegramViewBase.NavigateToConfirmPhone(result);
            //    //});
            //}
            //else
            //{
            //    //if (error.CodeEquals(ErrorCode.BAD_REQUEST) && error.TypeEquals(ErrorType.USERNAME_NOT_OCCUPIED))
            //    //{
            //    //    return;
            //    //}
            //    //Telegram.Api.Helpers.Logs.Log.Write(string.Format("account.sendConfirmPhoneCode error {0}", error));
            //};
        }

        public static async void NavigateToStickerSet(string text)
        {
            await StickersPopup.ShowAsync(text);
        }

        public static async void NavigateToPhoneNumber(IClientService clientService, INavigationService navigation, string phoneNumber)
        {
            await NavigateToUserByResponse(clientService, navigation, new SearchUserByPhoneNumber(phoneNumber));
        }

        public static async void NavigateToUserToken(IClientService clientService, INavigationService navigation, string userToken)
        {
            await NavigateToUserByResponse(clientService, navigation, new SearchUserByToken(userToken));
        }

        private static async Task NavigateToUserByResponse(IClientService clientService, INavigationService navigation, Function request)
        {
            var response = await clientService.SendAsync(request);
            if (response is User user)
            {
                var chat = await clientService.SendAsync(new CreatePrivateChat(user.Id, false)) as Chat;
                if (chat != null)
                {
                    navigation.Navigate(typeof(ProfilePage), chat.Id);
                }
                else
                {
                    await MessagePopup.ShowAsync(Strings.NoUsernameFound, Strings.AppName, Strings.OK);
                }
            }
            else
            {
                await MessagePopup.ShowAsync(Strings.NoUsernameFound, Strings.AppName, Strings.OK);
            }
        }

        public static async void NavigateToBotStart(IClientService clientService, INavigationService navigation, string username, string startParameter, bool autoStart, bool group)
        {
            var response = await clientService.SendAsync(new SearchPublicChat(username));
            if (response is Chat chat && clientService.TryGetUser(chat, out User user))
            {
                if (group)
                {
                    await SharePopup.GetForCurrentView().ShowAsync(user, startParameter);
                }
                else if (autoStart)
                {
                    clientService.Send(new SendBotStartMessage(user.Id, chat.Id, startParameter));
                    navigation.NavigateToChat(chat);
                }
                else
                {
                    navigation.NavigateToChat(chat, accessToken: startParameter);
                }
            }
            else
            {
                await MessagePopup.ShowAsync(Strings.NoUsernameFound, Strings.AppName, Strings.OK);
            }
        }

        public static async void NavigateToUsername(IClientService clientService, INavigationService navigation, string username, string videoChat, string game)
        {
            var response = await clientService.SendAsync(new SearchPublicChat(username));
            if (response is Chat chat)
            {
                if (game != null)
                {

                }
                else if (clientService.TryGetUser(chat, out User user))
                {
                    if (user.Type is UserTypeBot)
                    {
                        navigation.NavigateToChat(chat);
                    }
                    else
                    {
                        navigation.Navigate(typeof(ProfilePage), chat.Id);
                    }
                }
                else if (videoChat != null)
                {
                    navigation.NavigateToChat(chat, state: new NavigationState { { "videoChat", videoChat } });
                }
                else
                {
                    navigation.NavigateToChat(chat);
                }
            }
            else
            {
                await MessagePopup.ShowAsync(Strings.NoUsernameFound, Strings.AppName, Strings.OK);
            }
        }

        public static async void NavigateToInviteLink(IClientService clientService, INavigationService navigation, string link)
        {
            var response = await clientService.CheckChatInviteLinkAsync(link);
            if (response is ChatInviteLinkInfo info)
            {
                if (info.ChatId != 0)
                {
                    navigation.NavigateToChat(info.ChatId);
                }
                else
                {
                    var dialog = new JoinChatPopup(clientService, info);

                    var confirm = await dialog.ShowQueuedAsync();
                    if (confirm != ContentDialogResult.Primary)
                    {
                        return;
                    }

                    var import = await clientService.SendAsync(new JoinChatByInviteLink(link));
                    if (import is Chat chat)
                    {
                        navigation.NavigateToChat(chat);
                    }
                    else if (import is Error error)
                    {
                        if (error.MessageEquals(ErrorType.FLOOD_WAIT))
                        {
                            await MessagePopup.ShowAsync(Strings.FloodWait, Strings.AppName, Strings.OK);
                        }
                        else if (error.MessageEquals(ErrorType.USERS_TOO_MUCH))
                        {
                            await MessagePopup.ShowAsync(Strings.JoinToGroupErrorFull, Strings.AppName, Strings.OK);
                        }
                        else
                        {
                            await MessagePopup.ShowAsync(Strings.JoinToGroupErrorNotExist, Strings.AppName, Strings.OK);
                        }
                    }
                }
            }
            else if (response is Error error)
            {
                if (error.MessageEquals(ErrorType.FLOOD_WAIT))
                {
                    await MessagePopup.ShowAsync(Strings.FloodWait, Strings.AppName, Strings.OK);
                }
                else
                {
                    await MessagePopup.ShowAsync(Strings.JoinToGroupErrorNotExist, Strings.AppName, Strings.OK);
                }
            }
        }

        public static async void NavigateToChatFolderInviteLink(IClientService clientService, INavigationService navigation, string link)
        {
            var response = await clientService.SendAsync(new CheckChatFolderInviteLink(link));
            if (response is ChatFolderInviteLinkInfo info)
            {
                var tsc = new TaskCompletionSource<object>();

                var confirm = await navigation.ShowPopupAsync(typeof(AddFolderPopup), info, tsc);
                if (confirm == ContentDialogResult.Primary)
                {
                    var result = await tsc.Task;
                    if (result is IList<long> chats)
                    {
                        if (info.ChatFolderInfo.Id == 0)
                        {
                            var import = await clientService.SendAsync(new AddChatFolderByInviteLink(link, chats));
                            if (import is Error error)
                            {
                                if (error.MessageEquals(ErrorType.CHATLISTS_TOO_MUCH))
                                {
                                    navigation.ShowLimitReached(new PremiumLimitTypeShareableChatFolderCount());
                                }
                                else if (error.MessageEquals(ErrorType.FILTER_INCLUDE_TOO_MUCH))
                                {
                                    navigation.ShowLimitReached(new PremiumLimitTypeChatFolderChosenChatCount());
                                }
                                else if (error.MessageEquals(ErrorType.CHANNELS_TOO_MUCH))
                                {
                                    navigation.ShowLimitReached(new PremiumLimitTypeSupergroupCount());
                                }
                                else
                                {
                                    await MessagePopup.ShowAsync(Strings.FolderLinkExpiredAlert, Strings.AppName, Strings.OK);
                                }
                            }
                        }
                        else if (chats.Count > 0)
                        {
                            clientService.Send(new ProcessChatFolderNewChats(info.ChatFolderInfo.Id, chats));
                        }
                    }
                }
            }
            else if (response is Error error)
            {
                await MessagePopup.ShowAsync(Strings.FolderLinkExpiredAlert, Strings.AppName, Strings.OK);
            }
        }

        public static bool IsValidUsername(string username)
        {
            if (username.Length <= 2)
            {
                return false;
            }
            if (username.Length > 32)
            {
                return false;
            }
            if (username[0] != '@')
            {
                return false;
            }
            for (int i = 1; i < username.Length; i++)
            {
                if (!IsValidUsernameSymbol(username[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidCommandSymbol(char symbol)
        {
            return (symbol >= 'a' && symbol <= 'z') || (symbol >= 'A' && symbol <= 'Z') || (symbol >= '0' && symbol <= '9') || symbol == '_';
        }

        public static bool IsValidUsernameSymbol(char symbol)
        {
            return (symbol >= 'a' && symbol <= 'z') || (symbol >= 'A' && symbol <= 'Z') || (symbol >= '0' && symbol <= '9') || symbol == '_';
        }

        public static async void OpenUrl(IClientService clientService, INavigationService navigationService, string url, bool untrust)
        {
            if (TryCreateUri(url, out Uri uri))
            {
                if (IsTelegramUrl(uri))
                {
                    OpenTelegramUrl(clientService, navigationService, uri);
                }
                else
                {
                    //if (message?.Media is TLMessageMediaWebPage webpageMedia)
                    //{
                    //    if (webpageMedia.WebPage is TLWebPage webpage && webpage.HasCachedPage && webpage.Url.Equals(navigation))
                    //    {
                    //        var service = WindowWrapper.Current().NavigationServices.GetByFrameId("Main");
                    //        if (service != null)
                    //        {
                    //            service.Navigate(typeof(InstantPage), webpageMedia);
                    //            return;
                    //        }
                    //    }
                    //}

                    if (untrust)
                    {
                        var confirm = await MessagePopup.ShowAsync(string.Format(Strings.OpenUrlAlert, url), Strings.OpenUrlTitle, Strings.Open, Strings.Cancel);
                        if (confirm != ContentDialogResult.Primary)
                        {
                            return;
                        }
                    }

                    try
                    {
                        await Launcher.LaunchUriAsync(uri);
                    }
                    catch { }
                }
            }
        }

        #region Entity

        public static void Hyperlink_ContextRequested(ITranslateService service, UIElement sender, ContextRequestedEventArgs args)
        {
            var text = sender as RichTextBlock;
            if (args.TryGetPosition(sender, out Point point))
            {
                var items = Hyperlink_ContextRequested(service, text, point);
                if (items.Count > 0)
                {
                    var flyout = new MenuFlyout();

                    foreach (var item in items)
                    {
                        flyout.Items.Add(item);
                    }

                    // We don't want to unfocus the text are when the context menu gets opened
                    flyout.ShowAt(sender, new FlyoutShowOptions { Position = point, ShowMode = FlyoutShowMode.Transient });
                    args.Handled = true;
                }
                else
                {
                    args.Handled = false;
                }
            }
            else
            {
                args.Handled = false;
            }
        }

        public static IList<MenuFlyoutItemBase> Hyperlink_ContextRequested(ITranslateService service, RichTextBlock text, Point point)
        {
            if (point.X < 0 || point.Y < 0)
            {
                point = new Point(Math.Max(point.X, 0), Math.Max(point.Y, 0));
            }

            var items = new List<MenuFlyoutItemBase>();

            var length = text.SelectedText.Length;
            if (length > 0)
            {
                var link = text.SelectedText;

                var copy = new MenuFlyoutItem { Text = Strings.Copy, DataContext = link, Icon = new FontIcon { Glyph = Icons.DocumentCopy, FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily } };
                copy.Click += LinkCopy_Click;

                items.Add(copy);

                if (service != null && service.CanTranslate(link))
                {
                    var translate = new MenuFlyoutItem { Text = Strings.TranslateMessage, DataContext = link, Tag = service, Icon = new FontIcon { Glyph = Icons.Translate, FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily } };
                    translate.Click += LinkTranslate_Click;

                    items.Add(translate);
                }
            }
            else
            {
                var hyperlink = text.GetHyperlinkFromPoint(point);
                if (hyperlink == null)
                {
                    return items;
                }

                var link = GetEntityData(hyperlink);
                if (link == null)
                {
                    return items;
                }

                var type = GetEntityType(hyperlink);
                if (type is null or TextEntityTypeUrl or TextEntityTypeTextUrl)
                {
                    var open = new MenuFlyoutItem { Text = Strings.Open, DataContext = link, Icon = new FontIcon { Glyph = Icons.OpenIn, FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily } };

                    var action = GetEntityAction(hyperlink);
                    if (action != null)
                    {
                        open.Click += (s, args) => action();
                    }
                    else
                    {
                        open.Click += LinkOpen_Click;
                    }

                    items.Add(open);
                }

                var copy = new MenuFlyoutItem { Text = Strings.Copy, DataContext = link, Icon = new FontIcon { Glyph = Icons.DocumentCopy, FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily } };
                copy.Click += LinkCopy_Click;

                items.Add(copy);
            }

            return items;
        }

        public static void Hyperlink_ContextRequested(UIElement sender, string link, ContextRequestedEventArgs args)
        {
            if (args.TryGetPosition(sender, out Point point))
            {
                if (point.X < 0 || point.Y < 0)
                {
                    point = new Point(Math.Max(point.X, 0), Math.Max(point.Y, 0));
                }

                var open = new MenuFlyoutItem { Text = Strings.Open, DataContext = link, Icon = new FontIcon { Glyph = Icons.OpenIn, FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily } };
                var copy = new MenuFlyoutItem { Text = Strings.Copy, DataContext = link, Icon = new FontIcon { Glyph = Icons.DocumentCopy, FontFamily = BootStrapper.Current.Resources["TelegramThemeFontFamily"] as FontFamily } };

                open.Click += LinkOpen_Click;
                copy.Click += LinkCopy_Click;

                var flyout = new MenuFlyout();
                flyout.Items.Add(open);
                flyout.Items.Add(copy);

                // We don't want to unfocus the text are when the context menu gets opened
                flyout.ShowAt(sender, new FlyoutShowOptions { Position = point, ShowMode = FlyoutShowMode.Transient });

                args.Handled = true;
            }
        }

        private static async void LinkOpen_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var entity = item.DataContext as string;

            if (TryCreateUri(entity, out Uri uri))
            {
                try
                {
                    await Launcher.LaunchUriAsync(uri);
                }
                catch { }
            }
        }

        private static void LinkCopy_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var entity = item.DataContext as string;

            var dataPackage = new DataPackage();
            dataPackage.SetText(entity);
            ClipboardEx.TrySetContent(dataPackage);
        }

        private static async void LinkTranslate_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var entity = item.DataContext as string;
            var service = item.Tag as ITranslateService;

            var language = LanguageIdentification.IdentifyLanguage(entity);
            var popup = new TranslatePopup(service, entity, language, LocaleService.Current.CurrentCulture.TwoLetterISOLanguageName, true);
            await popup.ShowQueuedAsync();
        }

        public static void CopyText(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            ClipboardEx.TrySetContent(dataPackage);
        }



        public static Action GetEntityAction(DependencyObject obj)
        {
            return (Action)obj.GetValue(EntityActionProperty);
        }

        public static void SetEntityAction(DependencyObject obj, Action value)
        {
            obj.SetValue(EntityActionProperty, value);
        }

        public static readonly DependencyProperty EntityActionProperty =
            DependencyProperty.RegisterAttached("EntityAction", typeof(Action), typeof(MessageHelper), new PropertyMetadata(null));





        public static string GetEntityData(DependencyObject obj)
        {
            return (string)obj.GetValue(EntityDataProperty);
        }

        public static void SetEntityData(DependencyObject obj, string value)
        {
            obj.SetValue(EntityDataProperty, value);
        }

        public static readonly DependencyProperty EntityDataProperty =
            DependencyProperty.RegisterAttached("EntityData", typeof(string), typeof(MessageHelper), new PropertyMetadata(null));





        public static TextEntityType GetEntityType(DependencyObject obj)
        {
            return (TextEntityType)obj.GetValue(EntityTypeProperty);
        }

        public static void SetEntityType(DependencyObject obj, TextEntityType value)
        {
            obj.SetValue(EntityTypeProperty, value);
        }

        public static readonly DependencyProperty EntityTypeProperty =
            DependencyProperty.RegisterAttached("EntityType", typeof(TextEntityType), typeof(MessageHelper), new PropertyMetadata(null));





        #endregion
    }

    public enum MessageCommandType
    {
        Invoke,
        Mention,
        Hashtag
    }
}
