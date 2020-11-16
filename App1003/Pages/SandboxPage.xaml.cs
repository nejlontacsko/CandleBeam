using NejlonTacsko.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace App1003.Pages
{
    public sealed partial class SandboxPage : Page
    {
        byte i = 12;
        byte v = 5;

        int row = 0;

        MyMidi m;

        private static ArtNetDmx artnet = new ArtNetDmx(0);
        private static DatagramSocket socket = new DatagramSocket();

        private static DispatcherTimer senderTimer = new DispatcherTimer()
        {
            //Interval = new TimeSpan(250000)
            Interval = new TimeSpan(100000)
        };

        private static DispatcherTimer animation = new DispatcherTimer()
        {
            Interval = new TimeSpan(100000)
        };

        public SandboxPage()
        {
            this.InitializeComponent();
            m = new MyMidi(midiInPortListBox, midiOutPortListBox, Dispatcher);

            senderTimer.Tick += SendTick;
            animation.Tick += AnimTick;
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs e) =>
            canvas.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF,
                (byte)(redSlider.Value), (byte)(greenSlider.Value), (byte)(blueSlider.Value)));

        private async void midiInPortListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deviceInformationCollection = m.inputDeviceWatcher.DeviceInformationCollection;

            if (deviceInformationCollection == null)
                return;

            DeviceInformation devInfo = deviceInformationCollection[midiInPortListBox.SelectedIndex];

            if (devInfo == null)
                return;

            m.midiInPort = await MidiInPort.FromIdAsync(devInfo.Id);

            if (m.midiInPort == null)
            {
                Debug.WriteLine("Unable to create MidiInPort from input device");
                return;
            }
            m.midiInPort.MessageReceived += MidiInPort_MessageReceived;
        }

        private async void midiOutPortListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deviceInformationCollection = m.outputDeviceWatcher.DeviceInformationCollection;

            if (deviceInformationCollection == null)
                return;

            DeviceInformation devInfo = deviceInformationCollection[midiOutPortListBox.SelectedIndex];

            if (devInfo == null)
                return;

            m.midiOutPort = await MidiOutPort.FromIdAsync(devInfo.Id);

            if (m.midiOutPort == null)
            {
                Debug.WriteLine("Unable to create MidiOutPort from output device");
                return;
            }

            if (midiOutPortListBox.SelectedValue.ToString().Contains("DDJ-XP1"))
            {
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, 0x1e, 33));
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, 0x20, 33));
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, 0x22, 33));
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, 0x1b, 64));
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(1, 0x1e, 33));
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(1, 0x20, 33));
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(1, 0x22, 33));
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(1, 0x1b, 64));

                i = 16;
                v = 33;
            }
            else if (midiOutPortListBox.SelectedValue.ToString().Contains("APC MINI"))
            {
                i = 0;
                v = 5;
            }
            animation.Start();
        }
        private void AnimTick(object sender, object e)
        {
            if (midiOutPortListBox.SelectedValue.ToString().Contains("DDJ-XP1"))
            {
                for (int j = 4; j >= 0; j--)
                {
                    m.midiOutPort.SendMessage(new MidiNoteOnMessage(7, (byte)(i - j), v));
                    m.midiOutPort.SendMessage(new MidiNoteOnMessage(9, (byte)(i - j), v));
                }
                i -= 4;

                if (i <= 0)
                {
                    i = 16;
                    if (v == 99)
                    {
                        animation.Stop();
                    }
                    v = 99;
                }
            }
            else if (midiOutPortListBox.SelectedValue.ToString().Contains("APC MINI"))
            {
                for (int j = 0; j < 8; j++)
                    m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, i++, v));

                if (i == 64)
                {
                    i = 0;
                    if (v == 0)
                    {
                        animation.Stop();
                        redraw();
                    }
                    v = 0;
                }
            }
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            if (!senderTimer.IsEnabled)
            {
                senderTimer.Start();
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, 70, 2));
            }
            else
            {
                senderTimer.Stop();
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, 70, 0));
            }
        }

        private async void SendTick(object sender, object e)
        {
            Sllcp.OutDmx outDmx = Sllcp.Factory.CreateOutDmx(new IPAddress(0), 0);
            outDmx.SetChannel(30, (byte)(255 * masterSlider.Value / 255));
            outDmx.SetChannel(31, (byte)(0));
            outDmx.SetChannel(32, (byte)(redSlider.Value * masterSlider.Value / 255));
            outDmx.SetChannel(33, (byte)(greenSlider.Value * masterSlider.Value / 255));
            outDmx.SetChannel(34, (byte)(blueSlider.Value * masterSlider.Value / 255));
            outDmx.SetChannel(35, (byte)(0));
            outDmx.SetChannel(40, (byte)(255 * masterSlider.Value / 255));
            outDmx.SetChannel(41, (byte)(0));
            outDmx.SetChannel(42, (byte)(redSlider2.Value * masterSlider.Value / 255));
            outDmx.SetChannel(43, (byte)(greenSlider2.Value * masterSlider.Value / 255));
            outDmx.SetChannel(44, (byte)(blueSlider2.Value * masterSlider.Value / 255));
            outDmx.SetChannel(45, (byte)(0));

            /*artnet.SetChannel(30, (byte)(255 * masterSlider.Value / 255));
            artnet.SetChannel(31, (byte)(0));
            artnet.SetChannel(32, (byte)(redSlider.Value * masterSlider.Value / 255));
            artnet.SetChannel(33, (byte)(greenSlider.Value * masterSlider.Value / 255));
            artnet.SetChannel(34, (byte)(blueSlider.Value * masterSlider.Value / 255));
            artnet.SetChannel(35, (byte)(0));
            artnet.SetChannel(40, (byte)(255 * masterSlider.Value / 255));
            artnet.SetChannel(41, (byte)(0));
            artnet.SetChannel(42, (byte)(redSlider2.Value * masterSlider.Value / 255));
            artnet.SetChannel(43, (byte)(greenSlider2.Value * masterSlider.Value / 255));
            artnet.SetChannel(44, (byte)(blueSlider2.Value * masterSlider.Value / 255));
            artnet.SetChannel(45, (byte)(0));*/

            try
            {
                using (var serverDatagramSocket = new DatagramSocket())
                {
                    using (Stream outputStream = (await serverDatagramSocket.GetOutputStreamAsync(new HostName(ipBox.Text), "6454")).AsStreamForWrite())
                    {
                        //byte[] packet = artnet.GetPacket();
                        byte[] packet = Sllcp.GetPacket(outDmx);
                        await outputStream.WriteAsync(packet, 0, packet.Length);
                        await outputStream.FlushAsync();
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.Fail("Couldn't send socket." + ex);
                senderTimer.Stop();
            }

            startButton.Background = new SolidColorBrush(Color.FromArgb(0x7f, 0xff, 0x7f, 0x00));
        }

        private void nud_ValueChanged(object sender, ValueChangedEventArgs eventArgs) =>
            m.midiOutPort.SendMessage(new MidiNoteOnMessage(7, 0, ((NumericBox)sender).Value));

        private void dmxLengthListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sllcp.Length = ((ListBox)(sender)).SelectedValue.ToString() switch
            {
                "DMX-256" => Sllcp.DmxMode.Dmx256,
                "DMX-512" => Sllcp.DmxMode.Dmx512,
                "DMX-1024" => Sllcp.DmxMode.Dmx1024,
                "DMX-2048" => Sllcp.DmxMode.Dmx2048,
                _ => Sllcp.DmxMode.Dmx256,
            };
        }
    }
}
