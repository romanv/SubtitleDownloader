namespace RV.SubD.Shell
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows.Input;

    using Prism.Commands;
    using Prism.Mvvm;
    using Prism.Regions;

    [Export]
    public class ShellViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        private ICommand _cmdNavigateTo;

        [ImportingConstructor]
        public ShellViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public ICommand CmdNavigateTo
            => _cmdNavigateTo ?? (_cmdNavigateTo = new DelegateCommand<string>(OnCmdNavigateTo));

        private void OnCmdNavigateTo(string s)
        {
            var uri = new Uri($"/{s}View", UriKind.Relative);
            _regionManager.RequestNavigate("ContentRegion", uri);
        }
    }
}
