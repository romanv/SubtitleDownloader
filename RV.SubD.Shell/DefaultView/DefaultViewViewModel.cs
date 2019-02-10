namespace RV.SubD.Shell.DefaultView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using Prism.Commands;
    using Prism.Mvvm;

    using RV.SubD.Core;
    using RV.SubD.Core.Data;
    using RV.SubD.Core.SitePlugins;
    using RV.SubD.Core.SitePlugins.Addic7ed;
    using RV.SubD.Core.Utils;

    [Export]
    public class DefaultViewViewModel : BindableBase
    {
        private readonly List<string> _supportedExtensions = new List<string> { ".mkv", ".mp4", ".avi" };
        private readonly List<string> _languages = new List<string> { "english" };

        private readonly List<ISearchPlugin> _searchPlugins;

        private TitleObject _title;

        private ICommand _cmdSearchSubtitle;
        private ICommand _cmdCancelSearch;

        private bool _searchInProgress;
        private bool _cancelButtonEnabled;

        private ObservableCollection<DownloadableSubtitleViewModel> _downloadableSubtitles =
            new ObservableCollection<DownloadableSubtitleViewModel>();

        private CancellationTokenSource _tokenSource;

        [ImportingConstructor]
        public DefaultViewViewModel()
        {
            _searchPlugins = new List<ISearchPlugin> { new Addic7ed() };

            ParseCommandLine();

            if (Title != null && Title.Title.Length > 0 && Title.IsValid)
            {
                CmdSearchSubtitle.Execute(null);
            }
        }

        public TitleObject Title
        {
            get
            {
                return _title;
            }

            set
            {
                SetProperty(ref _title, value);
            }
        }

        public bool SearchInProgress
        {
            get
            {
                return _searchInProgress;
            }

            set
            {
                SetProperty(ref _searchInProgress, value);
            }
        }

        public bool CancelButtonEnabled
        {
            get
            {
                return _cancelButtonEnabled;
            }

            set
            {
                SetProperty(ref _cancelButtonEnabled, value);
            }
        }

        public ObservableCollection<DownloadableSubtitleViewModel> DownloadableSubtitles
        {
            get
            {
                return _downloadableSubtitles;
            }

            set
            {
                SetProperty(ref _downloadableSubtitles, value);
            }
        }

        public ICommand CmdSearchSubtitle
            => _cmdSearchSubtitle ?? (_cmdSearchSubtitle = new DelegateCommand(OnCmdSearchSubtitle));

        public ICommand CmdCancelSearch
            => _cmdCancelSearch ?? (_cmdCancelSearch = new DelegateCommand(OnCmdCancelSearch));

        private void OnCmdCancelSearch()
        {
            _tokenSource.Cancel();
            CancelButtonEnabled = false;
        }

        private async void OnCmdSearchSubtitle()
        {
            if (!Title.IsValid)
            {
                return;
            }

            SearchInProgress = true;
            CancelButtonEnabled = true;

            try
            {
                await Task.Run(() => SearchSubtitlesAsync(Title));
            }
            catch (OperationCanceledException)
            {
                DownloadableSubtitles = new ObservableCollection<DownloadableSubtitleViewModel>();
            }
            catch (Exception ex)
            {
                Logger.LogLine(ex.Message);
                MessageBox.Show("Network error, check errors.log for details");
            }
            finally
            {
                SearchInProgress = false;
                CancelButtonEnabled = false;
            }
        }

        private async Task SearchSubtitlesAsync(TitleObject to)
        {
            _tokenSource = new CancellationTokenSource();

            var tasks =
                _searchPlugins.Select(
                    plugin => Task.Run(() => plugin.GetSubtitlesListAsync(to, _languages, _tokenSource.Token)))
                    .ToList();

            var searchResults = await Task.WhenAll(tasks);
            IList<DownloadableSubtitle> flatResults =
                searchResults.SelectMany(sr => sr).OrderByDescending(r => r.Downloads).ToList();

            DownloadableSubtitles =
                new ObservableCollection<DownloadableSubtitleViewModel>(
                    flatResults.Select(sub => new DownloadableSubtitleViewModel(sub)));
        }

        private void ParseCommandLine()
        {
            var args = Environment.GetCommandLineArgs();

            var tokenIndex = Array.FindIndex(args, a => a.StartsWith("--file"));
            if (tokenIndex > -1)
            {
                var filePath = args[tokenIndex].Replace("--file=", string.Empty);

                var ext = Path.GetExtension(filePath);
                if (!_supportedExtensions.Contains(ext))
                {
                    return;
                }

                var newTitle = ParseFileName(filePath);
                if (newTitle != null && newTitle.IsValid)
                {
                    Title = newTitle;
                }
            }
        }

        private TitleObject ParseFileName(string fileName)
        {
            return TitleParser.TryParse(out var to, fileName) ? to : null;
        }
    }
}
