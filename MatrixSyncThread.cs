using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace libMatrix
{
    public partial class MatrixAPI
    {
        private bool isRunningSync = false;

        public bool shouldFullSync = false;
        
        public TimeSpan period = TimeSpan.FromMilliseconds(250);

        public CancellationTokenSource syncThreadCts;

        public Task StartSyncThreads() {
            syncThreadCts = new CancellationTokenSource();
            
            return Task.Run(async () => {
                if (!isRunningSync)
                {
                    isRunningSync = true;
                    try
                    {
                        if (shouldFullSync)
                        {
                            await ClientSync(true, true);
                            shouldFullSync = false;
                        }
                        else
                            await ClientSync(true);
                    }
                    catch (Exception e)
                    {

                        Debug.WriteLine("Sync Exception: " + e.Message);
                    }

                    FlushMessageQueue();

                    isRunningSync = false;
                }

                await Task.Delay(period, syncThreadCts.Token);
            }, syncThreadCts.Token);
        }

        public void StopSyncThreads()
        {
            syncThreadCts.Cancel();
            FlushMessageQueue();
        }
    }
}
