﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Unigram.Common;
using Unigram.Native;
using Unigram.ViewModels;
using Unigram.ViewModels.Delegates;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using static Unigram.Services.GenerationService;

namespace Unigram.Services.Factories
{
    public interface IMessageFactory
    {
        MessageViewModel Create(IMessageDelegate delegato, Message message);

        Task<InputMessageFactory> CreatePhotoAsync(StorageFile file, bool asFile, int ttl = 0, Rect? crop = null);
        Task<InputMessageFactory> CreateVideoAsync(StorageFile file, bool animated, bool asFile, int ttl = 0, MediaEncodingProfile profile = null, VideoTransformEffectDefinition transform = null);
        Task<InputMessageFactory> CreateVideoNoteAsync(StorageFile file, MediaEncodingProfile profile = null, VideoTransformEffectDefinition transform = null);
        Task<InputMessageFactory> CreateDocumentAsync(StorageFile file);
    }

    public class MessageFactory : IMessageFactory
    {
        private readonly IProtoService _protoService;
        private readonly IPlaybackService _playbackService;
        private readonly IEventAggregator _aggregator;

        public MessageFactory(IProtoService protoService, IPlaybackService playbackService, IEventAggregator aggregator)
        {
            _protoService = protoService;
            _playbackService = playbackService;
            _aggregator = aggregator;
        }

        public MessageViewModel Create(IMessageDelegate delegato, Message message)
        {
            return new MessageViewModel(_protoService, _playbackService, delegato, message);
        }



        public async Task<InputMessageFactory> CreatePhotoAsync(StorageFile file, bool asFile, int ttl = 0, Rect? crop = null)
        {
            using (var stream = await file.OpenReadAsync())
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
                if (decoder.FrameCount > 1)
                {
                    asFile = true;
                }
            }

            var size = await ImageHelper.GetScaleAsync(file, crop: crop);

            var generated = await file.ToGeneratedAsync(asFile ? ConversionType.Copy : ConversionType.Compress, crop.HasValue ? JsonConvert.SerializeObject(crop) : null);
            var thumbnail = default(InputThumbnail);

            if (asFile)
            {
                return new InputMessageFactory { InputFile = generated, Type = new FileTypeDocument(), Delegate = (inputFile, caption) => new InputMessageDocument(inputFile, thumbnail, caption) };
            }
            else
            {
                return new InputMessageFactory { InputFile = generated, Type = new FileTypePhoto(), Delegate = (inputFile, caption) => new InputMessagePhoto(generated, thumbnail, new int[0], size.Width, size.Height, caption, ttl) };
            }
        }

        public async Task<InputMessageFactory> CreateVideoAsync(StorageFile file, bool animated, bool asFile, int ttl = 0, MediaEncodingProfile profile = null, VideoTransformEffectDefinition transform = null)
        {
            var basicProps = await file.GetBasicPropertiesAsync();
            var videoProps = await file.Properties.GetVideoPropertiesAsync();

            //var thumbnail = await ImageHelper.GetVideoThumbnailAsync(file, videoProps, transform);

            var duration = (int)videoProps.Duration.TotalSeconds;
            var videoWidth = (int)videoProps.GetWidth();
            var videoHeight = (int)videoProps.GetHeight();

            if (profile != null)
            {
                videoWidth = videoProps.Orientation == VideoOrientation.Rotate180 || videoProps.Orientation == VideoOrientation.Normal ? (int)profile.Video.Width : (int)profile.Video.Height;
                videoHeight = videoProps.Orientation == VideoOrientation.Rotate180 || videoProps.Orientation == VideoOrientation.Normal ? (int)profile.Video.Height : (int)profile.Video.Width;
            }

            var conversion = new VideoConversion();
            if (profile != null)
            {
                conversion.Transcode = true;
                conversion.Mute = animated;
                conversion.Width = profile.Video.Width;
                conversion.Height = profile.Video.Height;
                conversion.Bitrate = profile.Video.Bitrate;

                if (transform != null)
                {
                    conversion.Transform = true;
                    conversion.Rotation = transform.Rotation;
                    conversion.OutputSize = transform.OutputSize;
                    conversion.Mirror = transform.Mirror;
                    conversion.CropRectangle = transform.CropRectangle;
                }
            }

            var generated = await file.ToGeneratedAsync(ConversionType.Transcode, JsonConvert.SerializeObject(conversion));
            var thumbnail = await file.ToThumbnailAsync(conversion, ConversionType.TranscodeThumbnail, JsonConvert.SerializeObject(conversion));

            if (asFile && ttl == 0)
            {
                return new InputMessageFactory { InputFile = generated, Type = new FileTypeDocument(), Delegate = (inputFile, caption) => new InputMessageDocument(inputFile, thumbnail, caption) };
            }
            else if (animated && ttl == 0)
            {
                return new InputMessageFactory { InputFile = generated, Type = new FileTypeAnimation(), Delegate = (inputFile, caption) => new InputMessageAnimation(inputFile, thumbnail, duration, videoWidth, videoHeight, caption) };
            }

            return new InputMessageFactory { InputFile = generated, Type = new FileTypeVideo(), Delegate = (inputFile, caption) => new InputMessageVideo(inputFile, thumbnail, new int[0], duration, videoWidth, videoHeight, true, caption, ttl) };
        }

        public async Task<InputMessageFactory> CreateVideoNoteAsync(StorageFile file, MediaEncodingProfile profile = null, VideoTransformEffectDefinition transform = null)
        {
            var basicProps = await file.GetBasicPropertiesAsync();
            var videoProps = await file.Properties.GetVideoPropertiesAsync();

            //var thumbnail = await ImageHelper.GetVideoThumbnailAsync(file, videoProps, transform);

            var duration = (int)videoProps.Duration.TotalSeconds;
            var videoWidth = (int)videoProps.GetWidth();
            var videoHeight = (int)videoProps.GetHeight();

            if (profile != null)
            {
                videoWidth = videoProps.Orientation == VideoOrientation.Rotate180 || videoProps.Orientation == VideoOrientation.Normal ? (int)profile.Video.Width : (int)profile.Video.Height;
                videoHeight = videoProps.Orientation == VideoOrientation.Rotate180 || videoProps.Orientation == VideoOrientation.Normal ? (int)profile.Video.Height : (int)profile.Video.Width;
            }

            var conversion = new VideoConversion();
            if (profile != null)
            {
                conversion.Transcode = true;
                conversion.Width = profile.Video.Width;
                conversion.Height = profile.Video.Height;
                conversion.Bitrate = profile.Video.Bitrate;

                if (transform != null)
                {
                    conversion.Transform = true;
                    conversion.Rotation = transform.Rotation;
                    conversion.OutputSize = transform.OutputSize;
                    conversion.Mirror = transform.Mirror;
                    conversion.CropRectangle = transform.CropRectangle;
                }
            }

            var generated = await file.ToGeneratedAsync(ConversionType.Transcode, JsonConvert.SerializeObject(conversion));
            var thumbnail = await file.ToThumbnailAsync(conversion, ConversionType.TranscodeThumbnail, JsonConvert.SerializeObject(conversion));

            return new InputMessageFactory { InputFile = generated, Type = new FileTypeVideoNote(), Delegate = (inputFile, caption) => new InputMessageVideoNote(inputFile, thumbnail, duration, Math.Min(videoWidth, videoHeight)) };
        }

        public async Task<InputMessageFactory> CreateDocumentAsync(StorageFile file)
        {
            var generated = await file.ToGeneratedAsync();
            var thumbnail = default(InputThumbnail);

            if (file.FileType.Equals(".webp", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var buffer = await FileIO.ReadBufferAsync(file);
                    var webp = WebPImage.DecodeFromBuffer(buffer);

                    var width = webp.PixelWidth;
                    var height = webp.PixelHeight;

                    return new InputMessageFactory { InputFile = generated, Type = new FileTypeSticker(), Delegate = (inputFile, caption) => new InputMessageSticker(inputFile, thumbnail, width, height) };
                }
                catch
                {
                    // Not really a sticker, go on sending as a file
                }
            }

            return new InputMessageFactory { InputFile = generated, Type = new FileTypeDocument(), Delegate = (inputFile, caption) => new InputMessageDocument(inputFile, thumbnail, caption) };
        }
    }

    public class InputMessageFactory
    {
        public InputFile InputFile { get; set; }
        public FileType Type { get; set; }
        public Func<InputFile, FormattedText, InputMessageContent> Delegate { get; set; }
    }
}
