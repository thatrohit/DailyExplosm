using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace DailyExplosm
{
    public partial class History : PhoneApplicationPage
    {
        IsolatedStorageSettings appStorage = IsolatedStorageSettings.ApplicationSettings;
        bool isVisited = false;
        Dictionary<string, string> historyDict;

        public History()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (isVisited) return;

            if (!appStorage.Contains("browsingHistory"))
                return;

            historyDict = (Dictionary<string, string>)appStorage["browsingHistory"];
            for (int i = historyDict.Keys.Count - 1; i >= 0; i--)
            {
                TextBlock tb = new TextBlock
                {
                    Text = historyDict.Keys.ElementAt(i),
                    VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    FontSize = 18,
                    Margin = new Thickness(0,0,6,0)
                };
                Border br = new Border
                {
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 33, 36, 43)),
                    Height = 120,
                    Width = 120,
                    Margin = new Thickness(12),
                    Child = tb,
                    Tag = tb.Text
                };
                br.Tap += LoadComicInfoForDate;
                DatesPanel.Children.Add(br);
            }
        }

        private void LoadComicInfoForDate(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string dictKey = ((Border)sender).Tag.ToString();
            //date|number|name
            List<string> comicInfoList = historyDict[dictKey].ToString().Split('^').ToList();
            ComicInfoPanel.Children.Clear();
            for(int i = 0; i < comicInfoList.Count; i++)
            {
                Border br = new Border
                {
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 140, 0)),
                    Height = 80,
                    Width = 80,
                    CornerRadius = new CornerRadius(40),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    Margin = new Thickness(0,20,0,0),
                    Child = new TextBlock {
                        Text = "#" + comicInfoList[i].Split('|')[1],
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        FontSize = 18
                    },
                    Tag = comicInfoList[i].Split('|')[1]
                };

                TextBlock date = new TextBlock
                {
                    Text = comicInfoList[i].Split('|')[0],
                    Tag = comicInfoList[i].Split('|')[1],
                    Margin = new Thickness(100, -40, 0, 0),
                    FontSize = 18,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 40, 40, 40))
                };

                TextBlock name = new TextBlock
                {
                    Text = comicInfoList[i].Split('|')[2],
                    Margin = new Thickness(100, -75, 0, 0),
                    Tag = comicInfoList[i].Split('|')[1],
                    FontSize = 24,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0,0,0))
                };

                br.Tap += GoToComic;
                name.Tap += GoToComic;
                date.Tap += GoToComic;

                ComicInfoPanel.Children.Add(br);
                ComicInfoPanel.Children.Add(name);
                ComicInfoPanel.Children.Add(date);
            }
        }

        private void GoToComic(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string historyComicNumber = "";
            if (sender.GetType().Name == "Border")
                historyComicNumber = ((Border)sender).Tag.ToString();
            else
                historyComicNumber = ((TextBlock)sender).Tag.ToString();
            NavigationService.Navigate(new Uri("/MainPage.xaml?historyComicNumber=" + historyComicNumber, UriKind.Relative));
        }
    }
}