namespace RV.SubD.Shell.Resources
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class BaseWindow
    {
        public BaseWindow()
        {
            InitializeComponent();
        }

        private void Border_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((Mouse.DirectlyOver as Border)?.Name != "ContentBorder")
            {
                return;
            }

            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.DragMove();
        }

        private void CloseWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        private void MinimizeWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }

        private void MaximizeWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Maximized;
        }

        private void RestoreWindowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Normal;
        }
    }
}
