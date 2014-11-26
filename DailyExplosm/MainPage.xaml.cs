using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DailyExplosm.Resources;
using System.Text;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.IO.IsolatedStorage;

namespace DailyExplosm
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool isDrawerVisible = false, isFirstLoad = true;
        string browserHTML;
        DailyExplosm.Helpers comicObject;
        List<string> comicNumberList = new List<string>();
        IsolatedStorageSettings appStorage = IsolatedStorageSettings.ApplicationSettings;

        public MainPage()
        {
            InitializeComponent();
            
            //initialize helper object
            comicObject = new DailyExplosm.Helpers();
            loaderBrowser.NavigateToString(@"<div style='width:100%; height:100%; margin: auto; margin-top:10%'><img src='http://i.imgur.com/UU9z7Fx.gif' width='100%' alt='' /></div>");
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            //check for internet
            if (!(Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsCellularDataEnabled || Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsWiFiEnabled))
            {
                MessageBox.Show("Please connect to the internet.");
                App.Current.Terminate();
            }

            //show appbar in case coming back from browser
            ApplicationBar.IsVisible = true;
            LoadingAnim.Stop();

            //clear back stack
            if (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }

            //check if from history
            string historyComicNumber = null;
            NavigationContext.QueryString.TryGetValue("historyComicNumber", out historyComicNumber);
            if (historyComicNumber != null && isFirstLoad)
            {
                isFirstLoad = false;
                LoadComic("http://explosm.net/comics/" + historyComicNumber);
            }

            //check if navigated to self
            string navigatedToAuthor = null;
            NavigationContext.QueryString.TryGetValue("author", out navigatedToAuthor);

            if (navigatedToAuthor == "dave") tblockBrowsing.Text = "browsing Dave's comics";
            else if (navigatedToAuthor == "matt") tblockBrowsing.Text = "browsing Matt's comics";
            else if (navigatedToAuthor == "kris") tblockBrowsing.Text = "browsing Kris' comics";
            else if (navigatedToAuthor == "rob") tblockBrowsing.Text = "browsing Rob's comics";
            else tblockBrowsing.Text = "browsing c&h comics";

            if (navigatedToAuthor != null)
            {
               comicNumberList = await AuthorComicsPopulator.GetAuthorComicList(navigatedToAuthor);
            }

            //set first and last buttons
            if (!comicNumberList.Any())
            {
                imgFirst.Tag = "http://explosm.net/comics/15/";
                imgLast.Tag = "http://explosm.net/comics/new/";
            }
            else
            {
                imgFirst.Tag = "http://explosm.net/comics/" + comicNumberList[0] + "/";
                imgLast.Tag = "http://explosm.net/comics/" + comicNumberList.Last() + "/";
            }


            if (isFirstLoad)
            {
                if (!comicNumberList.Any())
                {
                    LoadComic("http://explosm.net/comics/new/");
                }
                else
                {
                    LoadComic("http://explosm.net/comics/" + comicNumberList.Last() + "/");
                }
            }
            isFirstLoad = false;
        }

        private async void LoadComic(string url)
        {
            //remove video button if exists
            if (ApplicationBar.Buttons.Count == 3)
            {
                ApplicationBar.Buttons.RemoveAt(2);
            }

            //please wait stuff
            LoadingAnim.Begin();
            border.Visibility = System.Windows.Visibility.Collapsed;
            loaderBrowser.Visibility = System.Windows.Visibility.Visible;
            tblockAuthorName.Text = "hold on...";
            comicName.Text = "loading comic name...";
            comicDate.Text = "";
            comicNumber.Text = "";

            //load newest comic
            await comicObject.GetSiteHTML(url);

            browserHTML = String.Format(@"<!doctype html>
                <html>
                <head>
                <meta name='viewport' content='width=device-width, initial-scale=1' />
                <title></title>
                </head>
                <body>
                <div style='width:100%; height:100%; margin: auto; margin-top:10%'><img src='{0}' width='100%' alt='' /></div>
                </body>
                </html>", comicObject.GetComicUrl());

            if (comicObject.IsAnimatedShort())
            {
                ApplicationBarIconButton b = new ApplicationBarIconButton();
                b.Text = "play";
                b.IconUri = new Uri("/Assets/AppBar/transport.play.png", UriKind.Relative);
                b.Click += PlayShort;
                ApplicationBar.Buttons.Add(b);
            }
            comicBrowser.NavigateToString(browserHTML.ToString());
            
            //load comic name, author, title etc
            tblockAuthorName.Text = comicObject.AuthorName();
            comicDate.Text = comicObject.GetComicDate().Contains('&') ? "" : comicObject.GetComicDate();
            comicNumber.Text = comicObject.ComicNumber();
            comicName.Text = comicObject.GetComicName();

            //load author photo
            switch(tblockAuthorName.Text)
            {
                case "Matt Melvin":
                    imgAuthor.Source = new BitmapImage(new Uri("/Assets/Images/matt.jpg", UriKind.Relative));
                    imgHeader.Source= new BitmapImage(new Uri("/Assets/Images/header_matt.jpg", UriKind.Relative));
                    break;
                case "Dave McElfatrick":
                    imgAuthor.Source = new BitmapImage(new Uri("/Assets/Images/dave.png", UriKind.Relative));
                    imgHeader.Source = new BitmapImage(new Uri("/Assets/Images/header_dave.jpg", UriKind.Relative));
                    break;
                case "Rob DenBleyker":
                    imgAuthor.Source = new BitmapImage(new Uri("/Assets/Images/rob.jpg", UriKind.Relative));
                    imgHeader.Source = new BitmapImage(new Uri("/Assets/Images/header_rob.jpg", UriKind.Relative));
                    break;
                case "Kris Wilson":
                    imgAuthor.Source = new BitmapImage(new Uri("/Assets/Images/kris.png", UriKind.Relative));
                    imgHeader.Source = new BitmapImage(new Uri("/Assets/Images/header_kris.png", UriKind.Relative));
                    break;
            }

            //get previous and next comic url
            if (!comicNumberList.Any())
            {
                imgPrev.Tag = comicObject.PreviousComicUrl();
                imgNext.Tag = comicObject.NextComicUrl();
            }
            else
            {
                if (comicNumberList.IndexOf(comicNumber.Text) == 0)
                    imgPrev.Tag = null;
                else
                    imgPrev.Tag = "http://explosm.net/comics/" + comicNumberList[comicNumberList.IndexOf(comicNumber.Text) - 1] + "/";

                if (comicNumberList.IndexOf(comicNumber.Text) == comicNumberList.IndexOf(comicNumberList.Last()))
                    imgNext.Tag = null;
                else
                    imgNext.Tag = "http://explosm.net/comics/" + comicNumberList[comicNumberList.IndexOf(comicNumber.Text) + 1] + "/";
            }

            //stop loaders
            border.Visibility = System.Windows.Visibility.Visible;
            loaderBrowser.Visibility = System.Windows.Visibility.Collapsed; 
            LoadingAnim.Stop();

            //save to history
            if(appStorage.Contains("browsingHistory"))
            {
                Dictionary<string, string> historyDict = (Dictionary<string, string>)appStorage["browsingHistory"];
                if (historyDict.ContainsKey(DateTime.Today.ToShortDateString()))
                {
                    if (!historyDict[DateTime.Today.ToShortDateString()].Contains(comicNumber.Text))
                    {
                        string existingValue = historyDict[DateTime.Today.ToShortDateString()];
                        historyDict.Remove(DateTime.Today.ToShortDateString());
                        historyDict.Add(DateTime.Today.ToShortDateString(), comicDate.Text + "|" + comicNumber.Text + "|" + comicName.Text + "^" + existingValue);
                    }
                }
                else
                {
                    historyDict.Add(DateTime.Today.ToShortDateString(), comicDate.Text + "|" + comicNumber.Text + "|" + comicName.Text);
                }
                appStorage.Remove("browsingHistory");
                appStorage.Add("browsingHistory", historyDict);
            }
            else
            {
                Dictionary<string, string> historyDict = new Dictionary<string, string>();
                historyDict.Add(DateTime.Today.ToShortDateString(), comicDate.Text + "|" + comicNumber.Text + "|" + comicName.Text);
                appStorage.Add("browsingHistory", historyDict);
            }
            
        }

        #region run time animations of navigation buttons
        private void LatestComic(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TapAnim.Stop();
            TapAnim.SetValue(System.Windows.Media.Animation.Storyboard.TargetNameProperty, "imgLast");
            TapAnim.Begin();

            Image img = (Image)sender;
            try
            {
                if (img.Tag == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            comicBrowser.NavigateToString("");
            LoadComic(img.Tag.ToString());
        }

        private void NextComic(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TapAnim.Stop();
            TapAnim.SetValue(System.Windows.Media.Animation.Storyboard.TargetNameProperty, "imgNext");
            TapAnim.Begin();

            Image img = (Image)sender;
            try
            {
                if (img.Tag == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            comicBrowser.NavigateToString("");
            LoadComic(img.Tag.ToString());
        }

        private void PreviousComic(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TapAnim.Stop();
            TapAnim.SetValue(System.Windows.Media.Animation.Storyboard.TargetNameProperty, "imgPrev");
            TapAnim.Begin();

            Image img = (Image)sender;
            try
            {
                if (img.Tag == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            comicBrowser.NavigateToString("");
            LoadComic(img.Tag.ToString());
        }

        private void FirstComic(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TapAnim.Stop();
            TapAnim.SetValue(System.Windows.Media.Animation.Storyboard.TargetNameProperty, "imgFirst");
            TapAnim.Begin();

            Image img = (Image)sender;
            try
            {
                if (img.Tag == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            comicBrowser.NavigateToString("");
            LoadComic(img.Tag.ToString());
        }

        private void LoadRandom(object sender, System.Windows.Input.GestureEventArgs e)
        {
            RandomIconAnimate.Begin();
            comicBrowser.NavigateToString("");
            if (!comicNumberList.Any())
                LoadComic("http://explosm.net/comics/random");
            else
            {
                Random rnd = new Random();
                LoadComic("http://explosm.net/comics/" + comicNumberList[rnd.Next(0, comicNumberList.Count - 1)]);
            }

        }
        #endregion

        #region drawer drag animations
        private void DrawerDrag(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (e.FinalVelocities.LinearVelocity.Y < -500)
            {
                if (isDrawerVisible)
                {
                    HideDrawer.Begin();
                    isDrawerVisible = false;
                    return;
                }
            }

            if (e.FinalVelocities.LinearVelocity.Y > 500)
            {
                if (!isDrawerVisible)
                {
                    ShowDrawer.Begin();
                    isDrawerVisible = true;
                    return;
                }
            }
        }
        #endregion

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (isDrawerVisible)
            {
                e.Cancel = true;
                HideDrawer.Begin();
                isDrawerVisible = false;
            }
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
            {
                comicBrowser.Margin = new Thickness(12, 0, 12, 0);
                LandscapeAnim.Begin();
                ApplicationBar.IsVisible = false;
            }
            else if (e.Orientation == PageOrientation.PortraitDown || e.Orientation == PageOrientation.PortraitUp)
            {
                comicBrowser.Margin = new Thickness(12, 140, 12, 0);
                HideDrawer.Begin();
                ApplicationBar.IsVisible = true;
            }
        }

        private void SaveComic(object sender, EventArgs e)
        {
            BitmapImage bmp = new BitmapImage(new Uri(comicObject.GetComicUrl(), UriKind.Absolute));
            bmp.CreateOptions = BitmapCreateOptions.BackgroundCreation;
            bmp.ImageOpened += (_s, _e) =>
                {
                    WriteableBitmap wb = new WriteableBitmap(bmp);
                    var fileStream = new MemoryStream();
                    wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 100, 100);
                    fileStream.Seek(0, SeekOrigin.Begin);

                    using (var mediaLibrary = new MediaLibrary())
                    {
                        mediaLibrary.SavePicture(comicObject.ComicNumber(), fileStream);
                    }
                };
        }

        private void ShareComic(object sender, EventArgs e)
        {
            ShareLinkTask share = new ShareLinkTask();
            share.LinkUri = new Uri("http://explosm.net/comics/" + comicObject.ComicNumber(), UriKind.Absolute);
            share.Show();
        }

        private void BrowseByKris(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml?author=kris", UriKind.Relative));
        }

        private void BrowseByDave(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml?author=dave", UriKind.Relative));
        }

        private void BrowseByMatt(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml?author=matt", UriKind.Relative));
        }

        private void BrowseByRob(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml?author=rob", UriKind.Relative));
        }

        private async void PlayShort(object sender, EventArgs e)
        {
            LoadingAnim.Stop();
            LoadingAnim.Begin();
            ApplicationBar.IsVisible = false;
            await comicObject.GetShortsSiteHTML();
            string youtubeUrl = "http://www.youtube.com/embed/" + comicObject.GetYoutubeUrl();
            WebBrowserTask browserTask = new WebBrowserTask();
            browserTask.Uri = new Uri(youtubeUrl);
            browserTask.Show();
        }

        private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        
            if (isDrawerVisible)
            {
                HideDrawer.Begin();
                isDrawerVisible = false;
                return;
            }
            
            if (!isDrawerVisible)
            {
                ShowDrawer.Begin();
                isDrawerVisible = true;
                return;
            }
        }

        private void AboutPage(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void GoToHistory(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/history.xaml", UriKind.Relative));
        }

    }
}