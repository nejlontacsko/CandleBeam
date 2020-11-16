using System;
using System.Diagnostics;
using Windows.Devices.Midi;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace App1003.Pages
{
    public partial class SandboxPage
    {
        public async void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            IMidiMessage receivedMidiMessage = args.Message;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                double x = receivedMidiMessage.Type switch
                {
                    MidiMessageType.ControlChange =>
                        ((MidiControlChangeMessage)receivedMidiMessage).Controller switch
                        {
                            48 => redSlider.Value = ((MidiControlChangeMessage)receivedMidiMessage).ControlValue * 2.008,
                            49 => greenSlider.Value = ((MidiControlChangeMessage)receivedMidiMessage).ControlValue * 2.008,
                            50 => blueSlider.Value = ((MidiControlChangeMessage)receivedMidiMessage).ControlValue * 2.008,
                            56 => masterSlider.Value = ((MidiControlChangeMessage)receivedMidiMessage).ControlValue * 2.008,
                            51 => redSlider2.Value = ((MidiControlChangeMessage)receivedMidiMessage).ControlValue * 2.008,
                            52 => greenSlider2.Value = ((MidiControlChangeMessage)receivedMidiMessage).ControlValue * 2.008,
                            53 => blueSlider2.Value = ((MidiControlChangeMessage)receivedMidiMessage).ControlValue * 2.008,
                            _ => 1
                        },
                    MidiMessageType.NoteOn =>
                        ((MidiNoteOnMessage)receivedMidiMessage).Note switch
                        {
                            65 => row = row > 0 ? row - 1 : 0,
                            64 => row = row < 7 ? row + 1 : 7,
                            66 => blueSlider.StartFlash(),
                            98 => masterSlider.StartFlash(),
                            _ => 2
                        },
                    MidiMessageType.NoteOff =>
                        ((MidiNoteOffMessage)receivedMidiMessage).Note switch
                        {
                            66 => blueSlider.StopFlash(),
                            98 => masterSlider.StopFlash(),
                            _ => 3
                        },
                    _ => 0
                };
            });
            if (receivedMidiMessage.Type == MidiMessageType.NoteOn)
                redraw();
        }

        private async void redraw()
        {
            Random r = new Random();
            for (byte i = 0; i < 64; i++)
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, i, (byte)((r.Next(0, 10) > 6) ? 5 : 0)));
            for (byte i = 0; i < 8; i++)
                m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, (byte)(i + 8 * row), 1));
            /*m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, (byte)(6 + 8 * row), 6));
            m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, (byte)(2 + 8 * row), 5));
            m.midiOutPort.SendMessage(new MidiNoteOnMessage(0, (byte)(7 + 8 * row), 3));*/
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                int rect = 7;
                foreach (Rectangle item in ((RelativePanel)canvas2.Children[0]).Children)
                {
                    if (rect == row)
                        item.Fill = new SolidColorBrush(Colors.MediumSpringGreen);
                    else
                        item.Fill = new SolidColorBrush(Colors.LightGray);
                    rect--;
                }
            });
        }
    }
}