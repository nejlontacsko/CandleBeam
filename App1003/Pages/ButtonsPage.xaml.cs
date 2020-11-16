using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace App1003.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ButtonsPage : Page
    {
        MyMidi m;
        public ButtonsPage()
        {
            this.InitializeComponent();
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 7; j >= 0; j--)
                {
                    Button b = new Button()
                    {
                        Content = j * 8 + i,
                        Width = 100,
                        Height = 30,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(20, 20, 0, 0)
                    };
                    myGrid.Children.Add(b);
                    Grid.SetColumn(b, i);
                    Grid.SetRow(b, 7 - j);
                }
            }
            m = new MyMidi(midiInPortListBox, midiOutPortListBox, Dispatcher);
        }
        private async void midiInPortListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m.midiInPort = await MidiInPort.FromIdAsync(m.inputDeviceWatcher.DeviceInformationCollection[midiInPortListBox.SelectedIndex].Id);
            m.midiInPort.MessageReceived += MidiInPort_MessageReceived;
        }

        private async void midiOutPortListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            m.midiOutPort = await MidiOutPort.FromIdAsync(m.outputDeviceWatcher.DeviceInformationCollection[midiOutPortListBox.SelectedIndex].Id);

        private async void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            IMidiMessage receivedMidiMessage = args.Message;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Brush x = receivedMidiMessage.Type switch
                {
                    MidiMessageType.NoteOn =>
                        ((Button)myGrid.Children.First(x => ((Button)x).Content.ToString() == ((MidiNoteOnMessage)receivedMidiMessage).Note.ToString())).Background = new SolidColorBrush(Colors.MediumSpringGreen),
                    MidiMessageType.NoteOff =>
                        ((Button)myGrid.Children.First(x => ((Button)x).Content.ToString() == ((MidiNoteOffMessage)receivedMidiMessage).Note.ToString())).Background = new SolidColorBrush(Colors.LightGray),
                    _ => null
                };
            });
        }
    }
}
