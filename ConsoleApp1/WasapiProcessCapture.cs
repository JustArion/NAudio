namespace ConsoleApp1;

using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wasapi.CoreAudioApi;
using NAudio.Wave;

public class WasapiProcessCapture : WasapiCapture
{
    private const int BUFFER_MS = 100;
    public WasapiProcessCapture(Process process, bool includeProcess = true) : base(CreateAudioClientFor(process, includeProcess), false, BUFFER_MS)
    {
        waveFormat = Format;
    }

    public static WaveFormat Format = new WaveFormat(44100, 16, 2);

    private static AudioClient CreateAudioClientFor(Process process, bool includeProcess)
    {
        var activationParams = new AudioClientActivationParams
        {
            ActivationType = AudioClientActivationType.ProcessLoopback,
            ProcessLoopbackParams = new()
            {
                ProcessLoopbackMode = includeProcess ? ProcessLoopbackMode.IncludeTargetProcessTree : ProcessLoopbackMode.ExcludeTargetProcessTree,
                TargetProcessId = (uint)process.Id
            }
        };
        
        var paramsHandle = GCHandle.Alloc(activationParams, GCHandleType.Pinned);
        var activateParams = new PropVariant
        {
            vt = (short)VarEnum.VT_BLOB,
            blobVal = new()
            {
                Length = Marshal.SizeOf<AudioClientActivationParams>(),
                Data = paramsHandle.AddrOfPinnedObject()
            }
        };
        var activateHandle = GCHandle.Alloc(activateParams, GCHandleType.Pinned);
        

        try
        {
            var icbh = new ActivateAudioInterfaceCompletionHandler<IAudioClient>(_ =>
            {
                Console.WriteLine("Done");
            });
            
            var guid = typeof(IAudioClient).GUID;
        
            NativeMethods.ActivateAudioInterfaceAsync(@"VAD\Process_Loopback", guid, activateHandle.AddrOfPinnedObject(), icbh, out _);

            var client = new AudioClient(icbh.GetAwaiter().GetResult());

            var duration = 10000 * BUFFER_MS;
            client.Initialize(AudioClientShareMode.Shared, GetFlags(), duration, 0, Format, Guid.Empty);
            return client;
            
        }
        finally
        {
            if (activateHandle.IsAllocated)
                activateHandle.Free();
            if (paramsHandle.IsAllocated) 
                paramsHandle.Free();
        }
    }

    private static AudioClientStreamFlags GetFlags()
    {
        return AudioClientStreamFlags.Loopback | AudioClientStreamFlags.EventCallback |
               AudioClientStreamFlags.AutoConvertPcm | AudioClientStreamFlags.SrcDefaultQuality;
    }

    protected override AudioClientStreamFlags GetAudioClientStreamFlags()
    {
        return AudioClientStreamFlags.Loopback | AudioClientStreamFlags.EventCallback | base.GetAudioClientStreamFlags();
    }    
}