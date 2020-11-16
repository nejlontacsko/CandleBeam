using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.UI.ViewManagement;
using NejlonTacsko.Network;

namespace App1003.Pages
{
    public sealed partial class MainPage : Page
    {
        ApplicationView appView;

        Page[] pages = {
            new SandboxPage(),
            new MixerPage(),
            new ButtonsPage(),
            new CuelistPage(),
            new DacsPage(),
            new FiguresPage(),
            new ControllersPage(),
            new DevicesPage(),
            new TopographyPage(),
            new ProtocolPage(),
            new SettingsPage()
        };

        public MainPage()
        {
            InitializeComponent();
            appView = ApplicationView.GetForCurrentView();

            appView.TitleBar.BackgroundColor = Colors.DodgerBlue;
            navi.Content = pages[0];

            Sllcp.PollReply reply = Sllcp.Factory.CreatePollReply(
                "Dj Fody", "WinController",
                Sllcp.DmxMode.Dmx512,
                true, true,
                Sllcp.DeviceCode.Controller,
                0, 0, 2, 0, 0, 1);
            Debug.WriteLine(reply.ToString());
            Sllcp.Poll poll = Sllcp.Factory.CreatePollRequest();
            Debug.WriteLine(poll.ToString());

            //Sllcp.PollReply rep = new Sllcp.PollReply();

            List<Sllcp.PollReply> replies = new List<Sllcp.PollReply>(50);
            for (int i = 0; i < 50; i++)
                replies.Add(Sllcp.Factory.CreatePollReply(
                    "Dj Fody", "WinController",
                    Sllcp.DmxMode.Dmx512,
                    true, true,
                    Sllcp.DeviceCode.Controller,
                    0, 0, 2, 0, 0, 1));
            List<Sllcp.PollResults> results = Sllcp.Factory.CreatePollResults(replies);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
            {
            Debug.WriteLine(((SymbolIcon)((NavigationViewItem)args.SelectedItem).Icon).Symbol);
            navi.Content = pages[((SymbolIcon)((NavigationViewItem)args.SelectedItem).Icon).Symbol switch
            {
                Symbol.Sort => 1,
                Symbol.SelectAll => 2,
                Symbol.AllApps => 3,
                Symbol.Camera => 4,
                Symbol.Emoji => 5,
                Symbol.MusicInfo => 6,
                Symbol.Pictures => 7,
                Symbol.Shuffle => 8,
                Symbol.MapDrive => 9,
                Symbol.Setting => 10,
                _ => 0
            }];
            }
    }
}
