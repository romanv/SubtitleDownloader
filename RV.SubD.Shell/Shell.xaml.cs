namespace RV.SubD.Shell
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using Prism.Mef.Regions;
    using Prism.Regions;

    using RV.SubD.Shell.Properties;

    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    [Export]
    public partial class Shell : IPartImportsSatisfiedNotification
    {
        private readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public Shell(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            InitializeComponent();
            LoadSettings();
        }

        [Import]
        public ShellViewModel ViewModel
        {
            set
            {
                DataContext = value;
            }
        }

        public void OnImportsSatisfied()
        {
            var viewToLoadUri = "/DefaultView";

            if (!string.IsNullOrEmpty(Settings.Default.ActiveView))
            {
                viewToLoadUri = $"/{Settings.Default.ActiveView}View";
            }

            var uri = new Uri(viewToLoadUri, UriKind.Relative);

            _regionManager.RequestNavigate("ContentRegion", uri);
        }

        private void LoadSettings()
        {
            Left = Settings.Default.WindowLeft;
            Top = Settings.Default.WindowTop;
            Width = Settings.Default.WindowWidth;
            Height = Settings.Default.WindowHeight;
            WindowState = Settings.Default.WindowMaximized ? WindowState.Maximized : WindowState.Normal;
        }

        private void Shell_OnClosing(object sender, CancelEventArgs e)
        {
            var activeViewName =
                ((_regionManager as MefRegionManager)?.Regions.First().ActiveViews.First() as Control)?.Name;

            Settings.Default.ActiveView = activeViewName;
            Settings.Default.WindowLeft = Left;
            Settings.Default.WindowTop = Top;
            Settings.Default.WindowHeight = Height;
            Settings.Default.WindowWidth = Width;
            Settings.Default.WindowMaximized = WindowState == WindowState.Maximized;
            Settings.Default.Save();
        }
    }
}
