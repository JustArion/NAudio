namespace ConsoleApp1;

using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Utils;
using NAudio.Wasapi.CoreAudioApi;
using NAudio.Wasapi.CoreAudioApi.Interfaces;

public class WasapiProcessLoopbackCapture : IActivateAudioInterfaceCompletionHandler
{
    public static async Task<AudioClient> ActivateAudioInterface(uint pid = 19648, bool includeProcessTree = true)
    {
        // min ver: audioclientactivationparams.h 
        // #if WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_APP)
        // #if (NTDDI_VERSION >= NTDDI_WIN10_FE)     
        
        var activationParams = new AudioClientActivationParams
        {
            ActivationType = AudioClientActivationType.ProcessLoopback,
            ProcessLoopbackParams = new()
            {
                ProcessLoopbackMode = includeProcessTree ? ProcessLoopbackMode.IncludeTargetProcessTree : ProcessLoopbackMode.ExcludeTargetProcessTree,
                TargetProcessId = pid
            }
        };

        var gcHandle = GCHandle.Alloc(activationParams, GCHandleType.Pinned);

        try
        {
            var icbh = new ActivateAudioInterfaceCompletionHandler<IAudioClient>(ActivateCompletedACH);
            
            // get the guid attribute and its value
            var guid = typeof(IAudioClient).GUID;
        
            NativeMethods.ActivateAudioInterfaceAsync(@"VAD\Process_Loopback", guid, gcHandle.AddrOfPinnedObject(), icbh, out _);

            var ac = await icbh;

            return new AudioClient(ac);
        }
        finally
        {
            if (gcHandle.IsAllocated) 
                gcHandle.Free();
        }
    }

    private static void ActivateCompletedACH(IAudioClient obj)
    {
        
    }

    public void ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation)
    {

        
        
    }
}