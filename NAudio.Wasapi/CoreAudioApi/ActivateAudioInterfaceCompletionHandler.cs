using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wasapi.CoreAudioApi.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NAudio.Wasapi.CoreAudioApi
{
    public class ActivateAudioInterfaceCompletionHandler<TClient> : IActivateAudioInterfaceCompletionHandler, IAgileObject where TClient : IAudioClient
    {
        private readonly Action<TClient> initializeAction;
        private readonly TaskCompletionSource<TClient> tcs = new TaskCompletionSource<TClient>();

        public ActivateAudioInterfaceCompletionHandler(
            Action<TClient> initializeAction)
        {
            this.initializeAction = initializeAction;
        }

        public void ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation)
        {
            // First get the activation results, and see if anything bad happened then
            activateOperation.GetActivateResult(out var hr, out var unk);
            if (hr != 0)
            {
                tcs.TrySetException(Marshal.GetExceptionForHR(hr, new IntPtr(-1)));
                return;
            }

            var pAudioClient = (TClient)unk;

            // Next try to call the client's (synchronous, blocking) initialization method.
            try
            {
                initializeAction(pAudioClient);
                tcs.SetResult(pAudioClient);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }


        }


        public TaskAwaiter<TClient> GetAwaiter()
        {
            return tcs.Task.GetAwaiter();
        }
    }
}
