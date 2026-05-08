using System;
using System.Windows;
using System.Windows.Controls;

namespace DynaAppX.WpfControls
{
    public partial class LoadingOverlay : UserControl
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoadingOverlay),
                new PropertyMetadata(false, OnIsLoadingChanged));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(LoadingOverlay),
                new PropertyMetadata("Loading..."));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var overlay = (LoadingOverlay)d;
            overlay.RootGrid.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public LoadingOverlay()
        {
            InitializeComponent();
        }

        public void Show(string message = "Loading...")
        {
            Message = message;
            IsLoading = true;
        }

        public void Hide()
        {
            IsLoading = false;
        }
    }
}