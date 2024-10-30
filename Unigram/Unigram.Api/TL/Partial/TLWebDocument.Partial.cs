﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Api.Helpers;

namespace Telegram.Api.TL
{
    public partial class TLWebDocument : ITLTransferable, INotifyPropertyChanged
    {
        #region Download/upload

        private double _uploadingProgress;
        public double UploadingProgress
        {
            get
            {
                return _uploadingProgress;
            }
            set
            {
                _uploadingProgress = value;
                RaisePropertyChanged(() => UploadingProgress);
                RaisePropertyChanged(() => Progress);
            }
        }

        private double _downloadingProgress;
        public double DownloadingProgress
        {
            get
            {
                return _downloadingProgress;
            }
            set
            {
                _downloadingProgress = value;
                RaisePropertyChanged(() => DownloadingProgress);
                RaisePropertyChanged(() => Progress);
            }
        }

        public double Progress
        {
            get
            {
                if (_downloadingProgress > 0)
                {
                    return _downloadingProgress;
                }

                return _uploadingProgress;
            }
        }

        private bool _isTransferring;
        public bool IsTransferring
        {
            get
            {
                return _isTransferring;
            }
            set
            {
                if (_isTransferring != value)
                {
                    _isTransferring = value;
                    RaisePropertyChanged(() => IsTransferring);
                }
            }
        }

        public double LastProgress { get; set; }

        public Progress<double> Download()
        {
            IsTransferring = true;

            return new Progress<double>((value) =>
            {
                IsTransferring = value < 1 && value > 0;
                DownloadingProgress = value;
                Debug.WriteLine(value);
            });
        }

        public Progress<double> Upload()
        {
            IsTransferring = true;

            return new Progress<double>((value) =>
            {
                IsTransferring = value < 1 && value > 0;
                UploadingProgress = value;
                Debug.WriteLine(value);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public override void RaisePropertyChanged(string propertyName)
        {
            Execute.OnUIThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        #endregion
    }
}
