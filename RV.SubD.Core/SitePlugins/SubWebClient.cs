namespace RV.SubD.Core.SitePlugins
{
    using System;
    using System.Net;

    internal class SubWebClient : WebClient
    {
        private bool exceptionOcurred;

        private Uri responseUri;

        public Uri ResponseUri => this.responseUri;

        public int? Timeout { private get; set; }

        public bool ExceptionOccured => this.exceptionOcurred;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var httpWebRequest = base.GetWebRequest(address) as HttpWebRequest;

            if (httpWebRequest == null)
            {
                return null;
            }

            httpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (this.Timeout.HasValue)
            {
                httpWebRequest.Timeout = this.Timeout.Value;
            }

            return httpWebRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                this.exceptionOcurred = false;

                WebResponse webResponse = base.GetWebResponse(request);

                if (webResponse != null)
                {
                    this.responseUri = webResponse.ResponseUri;
                }

                return webResponse;
            }

            catch (WebException ex)
            {
                this.exceptionOcurred = true;

                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                {
                    throw;
                }

                this.responseUri = ex.Response.ResponseUri;
                return ex.Response;
            }
        }
    }
}
