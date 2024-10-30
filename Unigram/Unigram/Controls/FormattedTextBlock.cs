//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Controls.Messages;
using Unigram.Services;
using Windows.Foundation;
using Windows.UI.Text;

namespace Unigram.Controls
{
    public class TextEntityClickEventArgs : EventArgs
    {
        public TextEntityClickEventArgs(TextEntityType type, object data)
        {
            Type = type;
            Data = data;
        }

        public TextEntityType Type { get; }

        public object Data { get; }
    }

    public class FormattedTextBlock : Control, IPlayerView
    {
        private IClientService _clientService;
        private FormattedText _formattedText;
        private double _fontSize;

        private readonly List<EmojiPosition> _positions = new();

        private bool _ignoreSpoilers = false;
        private bool _ignoreLayoutUpdated = true;

        private TextHighlighter _spoiler;

        private RichTextBlock TextBlock;
        private CustomEmojiCanvas CustomEmoji;

        private bool _templateApplied;

        public FormattedTextBlock()
        {
            DefaultStyleKey = typeof(FormattedTextBlock);
        }

        public bool AdjustLineEnding { get; set; }

        public bool IsPreformatted { get; private set; }

        public event EventHandler<TextEntityClickEventArgs> TextEntityClick;

        private ContextMenuOpeningEventHandler _contextMenuOpening;
        public event ContextMenuOpeningEventHandler ContextMenuOpening
        {
            add
            {
                if (TextBlock != null)
                {
                    TextBlock.ContextMenuOpening += value;
                }

                _contextMenuOpening += value;
            }
            remove
            {
                if (TextBlock != null)
                {
                    TextBlock.ContextMenuOpening -= value;
                }

                _contextMenuOpening -= value;
            }
        }

        protected override void OnApplyTemplate()
        {
            TextBlock = GetTemplateChild(nameof(TextBlock)) as RichTextBlock;
            TextBlock.ContextMenuOpening += _contextMenuOpening;

            _templateApplied = true;

            if (_clientService != null && _formattedText != null)
            {
                SetText(_clientService, _formattedText.Text, _formattedText.Entities, _fontSize);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _ignoreLayoutUpdated = false;
            return base.ArrangeOverride(finalSize);
        }

        public void Clear()
        {
            _clientService = null;
            _formattedText = null;

            _spoiler = null;

            _positions.Clear();

            if (TextBlock != null)
            {
                _ignoreLayoutUpdated = false;
                TextBlock.LayoutUpdated -= OnLayoutUpdated;
                TextBlock.Blocks.Clear();
            }

            UnloadObject(ref CustomEmoji);
        }

        public void Cleanup()
        {
            _positions.Clear();

            if (TextBlock != null)
            {
                _ignoreLayoutUpdated = true;
                TextBlock.LayoutUpdated -= OnLayoutUpdated;
            }

            UnloadObject(ref CustomEmoji);
        }

        private void Adjust()
        {
            if (TextBlock?.Blocks.Count > 0 && TextBlock.Blocks[0] is Paragraph existing)
            {
                existing.Inlines.Add(new LineBreak());
            }
        }

        public bool IgnoreSpoilers
        {
            get => _ignoreSpoilers;
            set
            {
                if (value == _ignoreSpoilers)
                {
                    return;
                }

                _ignoreSpoilers = value;

                if (value)
                {
                    SetText(_clientService, _formattedText?.Text, _formattedText?.Entities, _fontSize);
                    SetQuery(string.Empty);
                }
            }
        }

        public void SetFontSize(double fontSize)
        {
            _fontSize = fontSize;

            if (TextBlock?.Blocks.Count > 0 && TextBlock.Blocks[0] is Paragraph existing)
            {
                existing.FontSize = fontSize;
            }
        }

        public void SetQuery(string query)
        {
            if (TextBlock != null && TextBlock.IsLoaded && _formattedText != null)
            {
                TextBlock.TextHighlighters.Clear();

                if (query?.Length > 0)
                {
                    var find = _formattedText.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
                    if (find != -1)
                    {
                        var highligher = new TextHighlighter();
                        highligher.Foreground = new SolidColorBrush(Colors.White);
                        highligher.Background = new SolidColorBrush(Colors.Orange);
                        highligher.Ranges.Add(new TextRange { StartIndex = find, Length = query.Length });

                        TextBlock.TextHighlighters.Add(highligher);
                    }
                    else
                    {
                        TextBlock.TextHighlighters.Clear();
                    }
                }

                if (_spoiler != null)
                {
                    TextBlock.TextHighlighters.Add(_spoiler);
                }
            }
        }

        public void SetText(IClientService clientService, string text, IList<TextEntity> entities, double fontSize = 0)
        {
            entities ??= Array.Empty<TextEntity>();

            _clientService = clientService;
            _formattedText = new FormattedText(text, entities);
            _fontSize = fontSize;

            if (!_templateApplied || string.IsNullOrEmpty(text))
            {
                return;
            }

            _positions.Clear();
            TextHighlighter spoiler = null;

            var preformatted = false;

            var runs = TextStyleRun.GetRuns(text, entities);
            var previous = 0;

            var shift = 1;
            var close = false;

            var paragraph = new Paragraph();
            var inlines = paragraph.Inlines;

            var emojis = new HashSet<long>();

            foreach (var entity in runs)
            {
                if (entity.Offset > previous)
                {
                    inlines.Add(CreateDirectRun(text.Substring(previous, entity.Offset - previous), fontSize: fontSize));

                    // Run
                    shift++;
                    shift += entity.Offset - previous;

                    shift++;
                }

                if (entity.Length + entity.Offset > text.Length)
                {
                    previous = entity.Offset + entity.Length;
                    continue;
                }

                if (entity.HasFlag(Common.TextStyle.Monospace))
                {
                    var data = text.Substring(entity.Offset, entity.Length);

                    if (SettingsService.Current.Diagnostics.CopyFormattedCode && entity.Type is TextEntityTypeCode)
                    {
                        var hyperlink = new Hyperlink();
                        hyperlink.Click += (s, args) => Entity_Click(entity.Type, data);
                        hyperlink.Foreground = TextBlock.Foreground;
                        hyperlink.UnderlineStyle = UnderlineStyle.None;

                        hyperlink.Inlines.Add(CreateRun(data, fontFamily: new FontFamily("Consolas"), fontSize: fontSize));
                        inlines.Add(hyperlink);

                        // Hyperlink
                        shift++;
                        close = true;

                        // Run
                        shift++;
                        shift += entity.Length;
                    }
                    else
                    {
                        inlines.Add(CreateDirectRun(data, fontFamily: new FontFamily("Consolas"), fontSize: fontSize));
                        preformatted = entity.Type is TextEntityTypePre or TextEntityTypePreCode;

                        // Run
                        shift++;
                        shift += entity.Length;
                    }
                }
                else
                {
                    var local = inlines;

                    if (_ignoreSpoilers is false && entity.HasFlag(Common.TextStyle.Spoiler))
                    {
                        var hyperlink = new Hyperlink();
                        hyperlink.Click += (s, args) => Entity_Click(new TextEntityTypeSpoiler(), null);
                        hyperlink.Foreground = TextBlock.Foreground;
                        hyperlink.UnderlineStyle = UnderlineStyle.None;
                        hyperlink.FontFamily = App.Current.Resources["SpoilerFontFamily"] as FontFamily;
                        //hyperlink.Foreground = foreground;

                        spoiler ??= new TextHighlighter();
                        spoiler.Ranges.Add(new TextRange { StartIndex = entity.Offset, Length = entity.Length });

                        inlines.Add(hyperlink);
                        local = hyperlink.Inlines;

                        // Hyperlink
                        shift++;
                        close = true;
                    }
                    else if (entity.HasFlag(Common.TextStyle.Mention) || entity.HasFlag(Common.TextStyle.Url))
                    {
                        if (entity.Type is TextEntityTypeMentionName or TextEntityTypeTextUrl)
                        {
                            var hyperlink = new Hyperlink();
                            object data;
                            if (entity.Type is TextEntityTypeTextUrl textUrl)
                            {
                                data = textUrl.Url;
                                MessageHelper.SetEntityData(hyperlink, textUrl.Url);
                                MessageHelper.SetEntityType(hyperlink, entity.Type);

                                ToolTipService.SetToolTip(hyperlink, textUrl.Url);
                            }
                            else if (entity.Type is TextEntityTypeMentionName mentionName)
                            {
                                data = mentionName.UserId;
                            }

                            hyperlink.Click += (s, args) => Entity_Click(entity.Type, null);
                            hyperlink.Foreground = HyperlinkForeground ?? GetBrush("MessageForegroundLinkBrush");
                            hyperlink.UnderlineStyle = HyperlinkStyle;
                            hyperlink.FontWeight = HyperlinkFontWeight;

                            inlines.Add(hyperlink);
                            local = hyperlink.Inlines;
                        }
                        else
                        {
                            var hyperlink = new Hyperlink();
                            var original = entities.FirstOrDefault(x => x.Offset <= entity.Offset && x.Offset + x.Length >= entity.End);

                            var data = text.Substring(entity.Offset, entity.Length);

                            if (original != null)
                            {
                                data = text.Substring(original.Offset, original.Length);
                            }

                            hyperlink.Click += (s, args) => Entity_Click(entity.Type, data);
                            hyperlink.Foreground = HyperlinkForeground ?? GetBrush("MessageForegroundLinkBrush");
                            hyperlink.UnderlineStyle = HyperlinkStyle;
                            hyperlink.FontWeight = HyperlinkFontWeight;

                            //if (entity.Type is TextEntityTypeUrl || entity.Type is TextEntityTypeEmailAddress || entity.Type is TextEntityTypeBankCardNumber)
                            {
                                MessageHelper.SetEntityData(hyperlink, data);
                                MessageHelper.SetEntityType(hyperlink, entity.Type);
                            }

                            inlines.Add(hyperlink);
                            local = hyperlink.Inlines;
                        }

                        // Hyperlink
                        shift++;
                        close = true;
                    }

                    if (entity.Type is TextEntityTypeCustomEmoji customEmoji)
                    {
                        // Run
                        shift++;

                        _positions.Add(new EmojiPosition { X = shift, CustomEmojiId = customEmoji.CustomEmojiId });

                        inlines.Add(CreateDirectRun(text.Substring(entity.Offset, entity.Length), fontFamily: App.Current.Resources["SpoilerFontFamily"] as FontFamily, fontSize: fontSize));
                        emojis.Add(customEmoji.CustomEmojiId);

                        shift += entity.Length;
                    }
                    else
                    {
                        var run = CreateDirectRun(text.Substring(entity.Offset, entity.Length), fontSize: fontSize);

                        if (entity.HasFlag(Common.TextStyle.Underline))
                        {
                            run.TextDecorations |= TextDecorations.Underline;
                        }
                        if (entity.HasFlag(Common.TextStyle.Strikethrough))
                        {
                            run.TextDecorations |= TextDecorations.Strikethrough;
                        }

                        if (entity.HasFlag(Common.TextStyle.Bold))
                        {
                            run.FontWeight = FontWeights.SemiBold;
                        }
                        if (entity.HasFlag(Common.TextStyle.Italic))
                        {
                            run.FontStyle = FontStyle.Italic;
                        }

                        local.Add(run);

                        // Run
                        shift++;
                        shift += entity.Length;
                    }
                }

                previous = entity.Offset + entity.Length;
                shift++;

                if (close)
                {
                    shift++;
                    close = false;
                }
            }

            //ContentPanel.MaxWidth = preformatted ? double.PositiveInfinity : 432;
            IsPreformatted = preformatted;

            if (text.Length > previous)
            {
                inlines.Add(CreateDirectRun(text.Substring(previous), fontSize: fontSize));
            }

            if (spoiler?.Ranges.Count > 0)
            {
                spoiler.Foreground = new SolidColorBrush(Colors.Black);
                spoiler.Background = new SolidColorBrush(Colors.Black);

                _spoiler = spoiler;
            }
            else
            {
                _spoiler = null;
            }

            if (AutoFontSize)
            {
                paragraph.FontSize = Theme.Current.MessageFontSize;
            }

            TextBlock.Blocks.Clear();
            TextBlock.Blocks.Add(paragraph);

            if (AdjustLineEnding)
            {
                if (LocaleService.Current.FlowDirection == FlowDirection.LeftToRight && MessageHelper.IsAnyCharacterRightToLeft(text))
                {
                    TextBlock.FlowDirection = FlowDirection.RightToLeft;
                    Adjust();
                }
                else if (LocaleService.Current.FlowDirection == FlowDirection.RightToLeft && !MessageHelper.IsAnyCharacterRightToLeft(text))
                {
                    TextBlock.FlowDirection = FlowDirection.LeftToRight;
                    Adjust();
                }
                else
                {
                    TextBlock.FlowDirection = LocaleService.Current.FlowDirection;
                }
            }

            TextBlock.LayoutUpdated -= OnLayoutUpdated;

            if (emojis.Count > 0)
            {
                LoadObject(ref CustomEmoji, nameof(CustomEmoji));
                CustomEmoji.UpdateEntities(clientService, emojis);

                if (_playing)
                {
                    CustomEmoji.Play();
                }

                _ignoreLayoutUpdated = false;
                TextBlock.LayoutUpdated += OnLayoutUpdated;
            }
            else if (CustomEmoji != null)
            {
                UnloadObject(ref CustomEmoji);
            }
        }

        private void OnLayoutUpdated(object sender, object e)
        {
            if (_ignoreLayoutUpdated)
            {
                return;
            }

            if (_positions.Count > 0)
            {
                _ignoreLayoutUpdated = true;
                LoadCustomEmoji();
            }
            else
            {
                TextBlock.LayoutUpdated -= OnLayoutUpdated;
                UnloadObject(ref CustomEmoji);
            }
        }

        private void LoadCustomEmoji()
        {
            var positions = new List<EmojiPosition>();

            foreach (var item in _positions)
            {
                var pointer = TextBlock.ContentStart.GetPositionAtOffset(item.X, LogicalDirection.Forward);
                if (pointer == null)
                {
                    continue;
                }

                var rect = pointer.GetCharacterRect(LogicalDirection.Forward);

                positions.Add(new EmojiPosition
                {
                    CustomEmojiId = item.CustomEmojiId,
                    X = (int)rect.X,
                    Y = (int)rect.Y
                });
            }

            if (positions.Count < 1)
            {
                TextBlock.LayoutUpdated -= OnLayoutUpdated;
                UnloadObject(ref CustomEmoji);
            }
            else
            {
                LoadObject(ref CustomEmoji, nameof(CustomEmoji));
                CustomEmoji.UpdatePositions(positions);

                if (_playing)
                {
                    CustomEmoji.Play();
                }
            }
        }

#warning TODO: remove
        private Run CreateRun(string text, FontWeight? fontWeight = null, FontFamily fontFamily = null, double fontSize = 0)
        {
            var run = new Run();
            run.Text = text;

            if (fontWeight != null)
            {
                run.FontWeight = fontWeight.Value;
            }

            if (fontFamily != null)
            {
                run.FontFamily = fontFamily;
            }

            if (fontSize > 0)
            {
                run.FontSize = fontSize;
            }

            return run;
        }

        private Run CreateDirectRun(string text, FontWeight? fontWeight = null, FontFamily fontFamily = null, double fontSize = 0)
        {
            var run = new Run();
            run.Text = text;

            if (fontWeight != null)
            {
                run.FontWeight = fontWeight.Value;
            }

            if (fontFamily != null)
            {
                run.FontFamily = fontFamily;
            }

            if (fontSize > 0)
            {
                run.FontSize = fontSize;
            }

            return run;
        }

        private Brush GetBrush(string key)
        {
            //var message = _message;
            //if (message == null)
            //{
            //    return null;
            //}

            //if (message.IsOutgoing && !message.IsChannelPost)
            //{
            //    if (ActualTheme == ElementTheme.Light)
            //    {
            //        return ThemeOutgoing.Light[key].Brush;
            //    }
            //    else
            //    {
            //        return ThemeOutgoing.Dark[key].Brush;
            //    }
            //}
            //else
            if (ActualTheme == ElementTheme.Light)
            {
                return ThemeIncoming.Light[key].Brush;
            }
            else
            {
                return ThemeIncoming.Dark[key].Brush;
            }
        }

        private void Entity_Click(TextEntityType type, object data)
        {
            foreach (Paragraph block in TextBlock.Blocks)
            {
                foreach (var element in block.Inlines)
                {
                    if (element is Hyperlink)
                    {
                        ToolTipService.SetToolTip(element, null);
                    }
                }
            }

            TextEntityClick?.Invoke(this, new TextEntityClickEventArgs(type, data));
        }

        #region IPlayerView

        public bool IsAnimatable => CustomEmoji != null;

        public bool IsLoopingEnabled => true;

        private bool _playing;

        public bool Play()
        {
            _playing = true;
            CustomEmoji?.Play();

            return true;
        }

        public void Pause()
        {
            _playing = false;
            CustomEmoji?.Pause();
        }

        public void Unload()
        {
            _playing = false;
            CustomEmoji?.Unload();
        }

        #endregion

        #region XamlMarkupHelper

        private void LoadObject<T>(ref T element, /*[CallerArgumentExpression("element")]*/string name)
            where T : DependencyObject
        {
            element ??= GetTemplateChild(name) as T;
        }

        private void UnloadObject<T>(ref T element)
            where T : DependencyObject
        {
            if (element != null)
            {
                XamlMarkupHelper.UnloadObject(element);
                element = null;
            }
        }

        #endregion

        #region TextAlignment

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(FormattedTextBlock), new PropertyMetadata(TextAlignment.Left));

        #endregion

        #region TextStyle

        public Style TextStyle
        {
            get { return (Style)GetValue(TextStyleProperty); }
            set { SetValue(TextStyleProperty, value); }
        }

        public static readonly DependencyProperty TextStyleProperty =
            DependencyProperty.Register("TextStyle", typeof(Style), typeof(FormattedTextBlock), new PropertyMetadata(null));

        #endregion

        #region AutoPlay


        public bool AutoPlay
        {
            get { return (bool)GetValue(AutoPlayProperty); }
            set { SetValue(AutoPlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoPlay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register("AutoPlay", typeof(bool), typeof(FormattedTextBlock), new PropertyMetadata(false, OnAutoPlayChanged));

        private static void OnAutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FormattedTextBlock)d).OnAutoPlayChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnAutoPlayChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                EffectiveViewportChanged += OnEffectiveViewportChanged;
            }
            else
            {
                EffectiveViewportChanged -= OnEffectiveViewportChanged;
            }
        }

        private bool _withinViewport;

        private void OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            var within = args.BringIntoViewDistanceX == 0 || args.BringIntoViewDistanceY == 0;
            if (within && !_withinViewport)
            {
                _withinViewport = true;
                Play();
            }
            else if (_withinViewport && !within)
            {
                _withinViewport = false;
                Pause();
            }
        }

        #endregion

        #region IsTextSelectionEnabled

        public bool IsTextSelectionEnabled
        {
            get { return (bool)GetValue(IsTextSelectionEnabledProperty); }
            set { SetValue(IsTextSelectionEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsTextSelectionEnabledProperty =
            DependencyProperty.Register("IsTextSelectionEnabled", typeof(bool), typeof(FormattedTextBlock), new PropertyMetadata(true));

        #endregion

        #region Hyperlink

        public bool AutoFontSize { get; set; } = true;

        public UnderlineStyle HyperlinkStyle { get; set; } = UnderlineStyle.Single;

        public SolidColorBrush HyperlinkForeground { get; set; }

        public FontWeight HyperlinkFontWeight { get; set; } = FontWeights.Normal;

        #endregion
    }
}
