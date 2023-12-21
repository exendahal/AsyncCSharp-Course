using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCSharp_Course
{
    class WebClientWrapper
    {
        private WebClient wc = new WebClient();

        private async Task LongRunningOperation(CancellationToken t)
        {
            if (!t.IsCancellationRequested)
            {
                using (CancellationTokenRegistration ctr = t.Register(() => { wc.CancelAsync(); }))
                {
                    wc.DownloadStringAsync(new Uri("http://www.engineerspock.com"));
                }
            }
        }
        
    }
}