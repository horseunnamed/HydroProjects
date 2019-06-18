using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Core.Channels;
using Microsoft.Win32;

namespace ChannelsEditor
{

    class MainViewModel : ViewModel
    {
        private MainModel _model;
        private BitmapImage _channelsBitmap;
        private string _statusMessage;
        private IList<Channel> _channels;
        private Channel _selectedChannel;

        public BitmapImage ChannelsBitmap
        {
            get => _channelsBitmap;
            set => SetProperty(ref _channelsBitmap, value);
        }

        public IList<Channel> Channels
        {
            get => _channels;
            set => SetProperty(ref _channels, value);
        }

        public Channel SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                SetProperty(ref _selectedChannel, value);
                ChannelsBitmap = _model.DrawChannels(value).ToBitmapImage();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public DelegateCommand LoadChannelsCommand { get; }
        public DelegateCommand PointSelected { get; }

        public MainViewModel()
        {
            LoadChannelsCommand = new DelegateCommand(p => OnLoadChannels());
            PointSelected = new DelegateCommand( 
                execute: p =>
                {
                    var pos = Mouse.GetPosition((IInputElement)p);
                    OnBitmapPointSelected(pos.X, pos.Y);
                }, 
                canExecute: p => _channelsBitmap != null);
        }

        private void OnLoadChannels()
        {
            var dialog = new OpenFileDialog();
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _model = new MainModel(CgInteraction.ReadChannelsTreeFromCg(dialog.FileName));
                Channels = _model.GetAllChannels();
                ChannelsBitmap = _model.DrawChannels(null).ToBitmapImage();
            }
        }

        private void OnBitmapPointSelected(double x, double y)
        {
            var point = new ChannelPoint((int)x, (int)_channelsBitmap.Height - (int)y - 1);
            var selectedChannel = _model.GetChannelAt(point);
            SelectedChannel = selectedChannel;
            StatusMessage = CreateStatusMessage(selectedChannel?.Id);
        }

        private string CreateStatusMessage(long? selectedChannelId)
        {
            if (selectedChannelId.HasValue)
            {
                var channel = _model.GetChannelById(selectedChannelId.Value);
                return $@"Selected channel ID: {selectedChannelId}, Origin: (X = {channel.Points[0].X}, Y = {channel.Points[0].Y})";
            }

            return "";
        }
    }
}
