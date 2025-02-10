using System.Diagnostics;
using ConsoleApp1;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;


// record some audio
var cap = new WasapiProcessCapture(Process.GetProcessById(19648));
var buffer = new BufferedWaveProvider(WasapiProcessCapture.Format);
buffer.BufferDuration = TimeSpan.FromSeconds(3);
cap.DataAvailable += (s, a) =>
{
    Console.WriteLine("-");
    buffer.AddSamples(a.Buffer, 0, a.BytesRecorded);
};
cap.StartRecording();

Console.WriteLine("Recording");
await Task.Delay(TimeSpan.FromSeconds(3));
Console.WriteLine("Done");

cap.StopRecording();

// play it back
var tcs = new TaskCompletionSource();
var player = new WasapiOut();

player.PlaybackStopped += (s, a) => tcs.SetResult();
player.Init(buffer);
Console.WriteLine("playing");
player.Play();

await tcs.Task;

