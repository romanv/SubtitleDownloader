namespace RV.SubD.Shell.DefaultView
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using Prism.Commands;
    using Prism.Mvvm;

    using RV.SubD.Core.Data;

    public class DownloadableSubtitleViewModel : BindableBase
    {
        private ICommand _cmdDownload;
        private ICommand _cmdCancelDownload;

        private CancellationTokenSource _tokenSource;

        private bool _isDownloading;
        private bool _isCancelButtonEnabled;

        public DownloadableSubtitleViewModel(DownloadableSubtitle subtitle)
        {
            Subtitle = subtitle;
        }

        public DownloadableSubtitle Subtitle { get; }

        public bool IsDownloading
        {
            get
            {
                return _isDownloading;
            }

            set
            {
                SetProperty(ref _isDownloading, value);
            }
        }

        public bool IsCancelButtonEnabled
        {
            get
            {
                return _isCancelButtonEnabled;
            }

            set
            {
                SetProperty(ref _isCancelButtonEnabled, value);
            }
        }

        public ICommand CmdDownload => _cmdDownload ?? (_cmdDownload = new DelegateCommand(OnCmdDownload));

        public ICommand CmdCancelDownload
            => _cmdCancelDownload ?? (_cmdCancelDownload = new DelegateCommand(OnCmdCancelDownload));

        private static string GetSubFileName(string originalFilePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
            var filePath = Path.GetDirectoryName(originalFilePath);

            if (filePath == null)
            {
                throw new ArgumentException("File path is incorrent: " + originalFilePath);
            }

            // if file does not have subtitles, just use it's name with sub extension
            var basicName = Path.Combine(filePath, $"{fileName}.srt");

            if (!File.Exists(basicName))
            {
                return basicName;
            }

            // otherwise try and find first free index for file name like Modern.Family.S01E01.-INDEX-.srt
            var index = 1;

            while (File.Exists(Path.Combine(filePath, $"{fileName}.{index}.srt")) && index < 256)
            {
                index++;
            }

            return Path.Combine(filePath, $"{fileName}.{index}.srt");
        }

        private void OnCmdCancelDownload()
        {
            _tokenSource.Cancel();
            IsCancelButtonEnabled = false;
        }

        private async void OnCmdDownload()
        {
            try
            {
                await DownloadSubtitleAsync();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Cancelled");
            }
            catch (Exception)
            {
                MessageBox.Show("Download error");
            }
            finally
            {
                IsDownloading = false;
                IsCancelButtonEnabled = false;
            }
        }

        private async Task DownloadSubtitleAsync()
        {
            _tokenSource = new CancellationTokenSource();

            IsDownloading = true;
            IsCancelButtonEnabled = true;

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                var requestUrl = "http://" + Subtitle.DownloadUrl;

                using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(requestUrl, UriKind.Absolute)))
                {
                    request.Headers.Referrer = Subtitle.ReferrerUri;
                    var sendTask = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _tokenSource.Token);
                    var response = sendTask.Result.EnsureSuccessStatusCode();
                    var httpStream = await response.Content.ReadAsStreamAsync();

                    var subFileName = GetSubFileName(Subtitle.OriginalFilePath);

                    using (var fileStream = File.Create(subFileName))
                    {
                        httpStream.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }
            }
        }
    }
}
