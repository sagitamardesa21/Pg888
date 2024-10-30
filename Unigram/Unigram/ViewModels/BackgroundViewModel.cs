//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Collections;
using Unigram.Common;
using Unigram.Navigation.Services;
using Unigram.Services;
using Unigram.ViewModels.Delegates;
using Unigram.Views.Popups;
using Windows.Storage;
using Windows.UI;

namespace Unigram.ViewModels
{
    public class BackgroundViewModel : TLViewModelBase, IDelegable<IBackgroundDelegate>
    {
        public IBackgroundDelegate Delegate { get; set; }

        private bool _batchUpdate;

        public BackgroundViewModel(IClientService clientService, ISettingsService settingsService, IEventAggregator aggregator)
            : base(clientService, settingsService, aggregator)
        {
            Patterns = new MvxObservableCollection<Document>();

            ChangeRotationCommand = new RelayCommand(ChangeRotationExecute);

            ShareCommand = new RelayCommand(ShareExecute);
            DoneCommand = new RelayCommand(DoneExecute);
        }

        protected override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, NavigationState state)
        {
            Background background = parameter as Background;

            if (parameter is string data)
            {
                var split = data.Split('#');
                if (split[0] == Constants.WallpaperLocalFileName)
                {
                    var local = TdExtensions.GetLocalFile(System.IO.Path.Combine(ApplicationData.Current.TemporaryFolder.Path, Constants.WallpaperLocalFileName));
                    var document = new Document(Constants.WallpaperLocalFileName, "image/jpeg", null, null, local);

                    background = new Background(Constants.WallpaperLocalId, false, false, Constants.WallpaperLocalFileName, document, new BackgroundTypeWallpaper(false, false));
                }
                else if (split[0] == Constants.WallpaperColorFileName)
                {
                    background = new Background(Constants.WallpaperColorId, false, false, Constants.WallpaperColorFileName, null, new BackgroundTypeFill(new BackgroundFillSolid(0xdfe4e8)));
                }
                else if (Uri.TryCreate("tg://bg/" + parameter, UriKind.Absolute, out Uri uri))
                {
                    var type = TdBackground.FromUri(uri);
                    if (type is BackgroundTypeFill)
                    {
                        background = new Background(0, false, false, string.Empty, null, type);
                    }
                    else
                    {
                        var response = await ClientService.SendAsync(new SearchBackground(uri.Segments.Last()));
                        if (response is Background)
                        {
                            background = response as Background;
                            background.Type = type;
                        }
                        else if (response is Error error)
                        {

                        }
                    }
                }
            }

            if (background == null)
            {
                return;
            }

            Item = background;

            _batchUpdate = true;

            BackgroundFill fill = null;
            if (background.Type is BackgroundTypeFill typeFill)
            {
                fill = typeFill.Fill;
                Intensity = 100;
                IsBlurEnabled = false;
            }
            else if (background.Type is BackgroundTypePattern typePattern)
            {
                fill = typePattern.Fill;
                Intensity = typePattern.IsInverted ? -typePattern.Intensity : typePattern.Intensity;
                IsBlurEnabled = false;
            }
            else if (background.Type is BackgroundTypeWallpaper typeWallpaper)
            {
                fill = null;
                Intensity = 100;
                IsBlurEnabled = typeWallpaper.IsBlurred;
            }

            if (fill is BackgroundFillSolid fillSolid)
            {
                Color1 = fillSolid.Color.ToColor();
                Color2 = BackgroundColor.Empty;
                Rotation = 0;
            }
            else if (fill is BackgroundFillGradient fillGradient)
            {
                Color1 = fillGradient.TopColor.ToColor();
                Color2 = fillGradient.BottomColor.ToColor();
                Rotation = fillGradient.RotationAngle;
            }
            else if (fill is BackgroundFillFreeformGradient freeformGradient)
            {
                Color1 = freeformGradient.Colors[0].ToColor();
                Color2 = freeformGradient.Colors[1].ToColor();
                Color3 = freeformGradient.Colors[2].ToColor();

                if (freeformGradient.Colors.Count > 3)
                {
                    Color4 = freeformGradient.Colors[3].ToColor();
                }
            }

            _batchUpdate = false;
            Delegate?.UpdateBackground(_item);

            if (_item.Type is BackgroundTypePattern or BackgroundTypeFill)
            {
                var response = await ClientService.SendAsync(new GetBackgrounds());
                if (response is Backgrounds backgrounds)
                {
                    var patterns = backgrounds.BackgroundsValue.Where(x => x.Type is BackgroundTypePattern)
                                                               .Distinct(new EqualityComparerDelegate<Background>((x, y) =>
                                                               {
                                                                   return x.Document.DocumentValue.Id == y.Document.DocumentValue.Id;
                                                               }, obj =>
                                                               {
                                                                   return obj.Document.DocumentValue.Id;
                                                               }))
                                                               .Select(x => x.Document);

                    Patterns.ReplaceWith(new Document[] { null }.Union(patterns));
                    SelectedPattern = patterns.FirstOrDefault(x => x?.DocumentValue.Id == background.Document?.DocumentValue.Id);
                }
            }
        }

        public MvxObservableCollection<Document> Patterns { get; private set; }

        private Background _item;
        public Background Item
        {
            get => _item;
            set => Set(ref _item, value);
        }

        private bool _isBlurEnabled;
        public bool IsBlurEnabled
        {
            get => _isBlurEnabled;
            set => SetComponent(ref _isBlurEnabled, value);
        }

        private BackgroundColor _color1 = BackgroundColor.Empty;
        public BackgroundColor Color1
        {
            get => _color1;
            set => SetComponent(ref _color1, value);
        }

        private BackgroundColor _color2 = BackgroundColor.Empty;
        public BackgroundColor Color2
        {
            get => _color2;
            set => SetComponent(ref _color2, value);
        }

        private BackgroundColor _color3 = BackgroundColor.Empty;
        public BackgroundColor Color3
        {
            get => _color3;
            set => SetComponent(ref _color3, value);
        }

        private BackgroundColor _color4 = BackgroundColor.Empty;
        public BackgroundColor Color4
        {
            get => _color4;
            set => SetComponent(ref _color4, value);
        }

        private void SetComponent<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (_batchUpdate)
            {
                Set(ref storage, value, propertyName);
                return;
            }

            if (Set(ref storage, value, propertyName))
            {
                Item = new Background(Item.Id, false, Item.IsDark, Item.Name, Item.Document, Item.Type switch
                {
                    BackgroundTypeFill => new BackgroundTypeFill(GetFill()),
                    BackgroundTypePattern => new BackgroundTypePattern(GetFill(), _intensity < 0 ? 100 + _intensity : _intensity, _intensity < 0, false),
                    BackgroundTypeWallpaper => new BackgroundTypeWallpaper(_isBlurEnabled, false),
                    _ => null
                });
            }
        }

        public BackgroundFill GetFill()
        {
            if (!_color1.IsEmpty && !_color2.IsEmpty)
            {
                if (!_color3.IsEmpty && !_color4.IsEmpty)
                {
                    return new BackgroundFillFreeformGradient(new[] { _color1.Value, _color2.Value, _color3.Value, _color4.Value });
                }
                else if (!_color3.IsEmpty)
                {
                    return new BackgroundFillFreeformGradient(new[] { _color1.Value, _color2.Value, _color3.Value });
                }

                return new BackgroundFillGradient(_color1.Value, _color2.Value, _rotation);
            }
            else if (!_color1.IsEmpty)
            {
                return new BackgroundFillSolid(_color1.Value);
            }
            else if (!_color2.IsEmpty)
            {
                return new BackgroundFillSolid(_color2.Value);
            }

            return null;
        }

        public Color GetPatternForeground()
        {
            if (_intensity < 0)
            {
                return Colors.Black;
            }

            if (!_color1.IsEmpty && !_color2.IsEmpty)
            {
                return ColorEx.GetPatternColor(ColorEx.GetAverageColor(_color1, _color2));
            }
            else if (!_color1.IsEmpty)
            {
                return ColorEx.GetPatternColor(_color1);
            }
            else if (!_color2.IsEmpty)
            {
                return ColorEx.GetPatternColor(_color2);
            }

            return Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF);
        }

        private bool _isColor1Checked = true;
        public bool IsColor1Checked
        {
            get => _isColor1Checked;
            set => Set(ref _isColor1Checked, value);
        }

        private bool _isColor2Checked;
        public bool IsColor2Checked
        {
            get => _isColor2Checked;
            set => Set(ref _isColor2Checked, value);
        }

        private bool _isColor3Checked;
        public bool IsColor3Checked
        {
            get => _isColor3Checked;
            set => Set(ref _isColor3Checked, value);
        }

        private bool _isColor4Checked;
        public bool IsColor4Checked
        {
            get => _isColor4Checked;
            set => Set(ref _isColor4Checked, value);
        }

        private int _rotation;
        public int Rotation
        {
            get => _rotation;
            set => SetComponent(ref _rotation, value);
        }

        private int _intensity;
        public int Intensity
        {
            get => _intensity;
            set => SetComponent(ref _intensity, value);
        }

        private Document _selectedPattern;
        public Document SelectedPattern
        {
            get => _selectedPattern;
            set
            {
                Set(ref _selectedPattern, value);

                if (value?.DocumentValue.Id != _item.Document?.DocumentValue.Id && ((value != null && _item?.Type is BackgroundTypeFill) || _item?.Type is BackgroundTypePattern))
                {
                    if (value == null)
                    {
                        Item = new Background(Item.Id, false, Item.IsDark, Item.Name, null, new BackgroundTypeFill(GetFill()));
                    }
                    else
                    {
                        Item = new Background(Item.Id, false, Item.IsDark, Item.Name, value, new BackgroundTypePattern(GetFill(), _intensity < 0 ? 100 + _intensity : _intensity, _intensity < 0, false));
                    }

                    Delegate?.UpdateBackground(Item);
                }
            }
        }

        public Background GetPattern(Document value)
        {
            if (value == null)
            {
                return new Background(Item.Id, false, Item.IsDark, Item.Name, null, new BackgroundTypeFill(GetFill()));
            }

            return new Background(Item.Id, false, Item.IsDark, Item.Name, value, new BackgroundTypePattern(GetFill(), 50, _intensity < 0, false));
        }

        public void RemoveColor(int index)
        {
            if (index <= 0)
            {
                Color1 = Color2;
            }

            if (index <= 1)
            {
                Color2 = Color3;
            }

            if (index <= 2)
            {
                Color3 = Color4;
            }

            if (index <= 3)
            {
                Color4 = BackgroundColor.Empty;
            }

            IsColor1Checked = true;
        }

        public void AddColor()
        {
            if (Color2.IsEmpty)
            {
                Color2 = Color1;
                IsColor2Checked = true;
            }
            else if (Color3.IsEmpty)
            {
                Color3 = Color2;
                IsColor3Checked = true;
            }
            else if (Color4.IsEmpty)
            {
                Color4 = Color3;
                IsColor4Checked = true;
            }
        }

        public RelayCommand ChangeRotationCommand { get; }
        private void ChangeRotationExecute()
        {
            Rotation = (_rotation + 45) % 360;
        }

        public RelayCommand ShareCommand { get; }
        private async void ShareExecute()
        {
            var background = _item;
            if (background == null)
            {
                return;
            }

            var response = await ClientService.SendAsync(new GetBackgroundUrl(background.Name, background.Type));
            if (response is HttpUrl url)
            {
                await SharePopup.Create().ShowAsync(new Uri(url.Url), null);
            }
        }

        public RelayCommand DoneCommand { get; }
        private async void DoneExecute()
        {
            var background = _item;
            if (background == null)
            {
                return;
            }

            var dark = Settings.Appearance.IsDarkTheme();
            var freeform = dark ? new[] { 0x1B2836, 0x121A22, 0x1B2836, 0x121A22 } : new[] { 0xDBDDBB, 0x6BA587, 0xD5D88D, 0x88B884 };

            // This is a new background and it has to be uploaded to Telegram servers
            Task<BaseObject> task;
            if (background.Id == Constants.WallpaperLocalId)
            {
                var item = await ApplicationData.Current.TemporaryFolder.GetFileAsync(Constants.WallpaperLocalFileName);
                var generated = await item.ToGeneratedAsync(ConversionType.Copy, forceCopy: true);

                task = ClientService.SendAsync(new SetBackground(new InputBackgroundLocal(generated), new BackgroundTypeWallpaper(_isBlurEnabled, false), dark));
            }
            else
            {
                var fill = GetFill();
                if (background.Type is BackgroundTypeFill && fill is BackgroundFillFreeformGradient fillFreeform && fillFreeform.Colors.SequenceEqual(freeform))
                {
                    task = ClientService.SendAsync(new SetBackground(null, null, dark));
                }
                else
                {
                    BackgroundType type = null;
                    if (background.Type is BackgroundTypeFill)
                    {
                        type = new BackgroundTypeFill(fill);
                    }
                    else if (background.Type is BackgroundTypePattern)
                    {
                        type = new BackgroundTypePattern(fill, _intensity < 0 ? 100 + _intensity : _intensity, _intensity < 0, false);
                    }
                    else if (background.Type is BackgroundTypeWallpaper)
                    {
                        type = new BackgroundTypeWallpaper(_isBlurEnabled, false);
                    }

                    if (type == null)
                    {
                        return;
                    }

                    var input = background.Id == Constants.WallpaperLocalId || background.Id == Constants.WallpaperColorId
                        ? null
                        : new InputBackgroundRemote(background.Id);

                    task = ClientService.SendAsync(new SetBackground(input, type, dark));
                }
            }

            var response = await task;
            //if (response is Background)
            //{
            //    NavigationService.GoBack();
            //}
            //if (response is Error error)
            //{
            //    if (error.Code == 404)
            //    {
            //        NavigationService.GoBack();
            //    }
            //}
        }
    }

    public struct BackgroundColor
    {
        private BackgroundColor(int value, bool empty)
        {
            Value = value;
            IsEmpty = empty;
        }

        public static BackgroundColor FromValue(int value)
        {
            return new BackgroundColor(value, false);
        }

        public static BackgroundColor Empty = new BackgroundColor(0, true);

        public int Value;

        public bool IsEmpty;

        public static implicit operator Color(BackgroundColor rhs)
        {
            if (rhs.IsEmpty)
            {
                return Color.FromArgb(0, 0, 0, 0);
            }

            return rhs.Value.ToColor();
        }

        public static implicit operator BackgroundColor(Color lhs)
        {
            return FromValue((lhs.R << 16) + (lhs.G << 8) + lhs.B);
        }
    }
}
