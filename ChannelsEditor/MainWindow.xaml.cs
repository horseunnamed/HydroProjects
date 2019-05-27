using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChannelsEditor
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new MainViewModel();

        private Point? _lastCenterPositionOnTarget;
        private Point? _lastMousePositionOnTarget;

        public MainWindow()
        {
            DataContext = _viewModel;
            InitializeComponent();

            ImageScrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            ImageScrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            ImageSlider.ValueChanged += OnSliderValueChanged;
            ChannelsListBox.SelectionChanged += OnSelectionChanged;
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChannelsListBox.ScrollIntoView(ChannelsListBox.SelectedItem);
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _lastMousePositionOnTarget = Mouse.GetPosition(ChannelsImage);

            if (e.Delta > 0)
            {
                ImageSlider.Value += 1;
            }
            if (e.Delta < 0)
            {
                ImageSlider.Value -= 1;
            }

            e.Handled = true;
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ImageScaleTransform.ScaleX = e.NewValue;
            ImageScaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(ImageScrollViewer.ViewportWidth / 2, ImageScrollViewer.ViewportHeight / 2);
            _lastCenterPositionOnTarget = ImageScrollViewer.TranslatePoint(centerOfViewport, ImageGrid);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!_lastMousePositionOnTarget.HasValue)
                {
                    if (_lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(ImageScrollViewer.ViewportWidth/2, ImageScrollViewer.ViewportHeight/2);
                        var centerOfTargetNow = ImageScrollViewer.TranslatePoint(centerOfViewport, ImageGrid);

                        targetBefore = _lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = _lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(ImageGrid);

                    _lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    var dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    var dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    var multiplicatorX = e.ExtentWidth / ImageGrid.Width;
                    var multiplicatorY = e.ExtentHeight / ImageGrid.Height;

                    var newOffsetX = ImageScrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    var newOffsetY = ImageScrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    ImageScrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    ImageScrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

    }
}
