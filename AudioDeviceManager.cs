using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Censorduck
{
    public class AudioDeviceManager
    {
        public ObservableCollection<WaveInCapabilities> AudioDevices { get; private set; }

        public AudioDeviceManager()
        {
            AudioDevices = new ObservableCollection<WaveInCapabilities>();
            EnumerateAudioDevices();
        }

        private void EnumerateAudioDevices()
        {
            for (int device = 0; device < WaveIn.DeviceCount; device++)
            {
                var capabilities = WaveIn.GetCapabilities(device);
                AudioDevices.Add(capabilities);
            }
        }

        public int GetUserSelectedDeviceIndex()
        {
            Console.WriteLine("Available Audio Devices:");
            for (int i = 0; i < AudioDevices.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {AudioDevices[i].ProductName}");
            }

            Console.Write("Choose an audio device (enter the corresponding number): ");
            int selectedDeviceIndex;
            while (!int.TryParse(Console.ReadLine(), out selectedDeviceIndex) ||
                   selectedDeviceIndex < 1 || selectedDeviceIndex > AudioDevices.Count)
            {
                Console.WriteLine("Invalid selection. Please enter a valid number.");
            }

            return selectedDeviceIndex - 1;
        }
    }

}
