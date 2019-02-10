namespace RV.SubD.Shell.DefaultView
{
    using System.ComponentModel.Composition;

    using Prism.Regions;

    /// <summary>
    /// Interaction logic for DefaultView.xaml
    /// </summary>
    [Export("DefaultView")]
    public partial class DefaultView : INavigationAware
    {
        public DefaultView()
        {
            InitializeComponent();
        }

        [Import]
        public DefaultViewViewModel ViewModel
        {
            set
            {
                DataContext = value;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // navigated event
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // navigated event
        }
    }
}
