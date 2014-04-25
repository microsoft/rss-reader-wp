/**
 * Copyright (c) 2011-2014 Microsoft Mobile. All rights reserved.
 *
 * Nokia, Nokia Connecting People, Nokia Developer, and HERE are trademarks
 * and/or registered trademarks of Nokia Corporation. Other product and company
 * names mentioned herein may be trademarks or trade names of their respective
 * owners.
 *
 * See the license text file delivered with this project for more information.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using RSSReader.Model;

namespace RSSReader.Views
{
    /// <summary>
    /// Main panorama page of the application
    /// </summary>
    public partial class MainPano : PhoneApplicationPage
    {
        /// <summary>
        /// True when this object instance has been just created, otherwise false
        /// </summary>
        private bool isNewInstance = true;

        /// <summary>
        /// Const for identifying panorama index in transient storage
        /// </summary>
        private static readonly string MAIN_PANORAMA_INDEX = "mainPanoramaIndex";

        /// <summary>
        /// Member variable for saving and restoring current panorama index
        /// </summary>
        private int panoramaIndex = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainPano()
        {
            InitializeComponent();

            RSSFeedsPanorama.Loaded += new RoutedEventHandler(RSSFeedsPanorama_Loaded);
        }

        /// <summary>
        /// Overridden OnNavigatedTo handler
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (isNewInstance)
            {
                // Restoration of panorama index cannot be done here as panorama items are loaded from a collection
                // that might not be ready when OnNavigatedTo() is called. We need to do the restoration upon
                // Loaded event. We need to take the panorama index out of State here and store it to member variable
                // because State is guaranteed to be accessible only during OnNavigatedTo() and OnNavigatedFrom()
                if (State.ContainsKey(MAIN_PANORAMA_INDEX))
                {
                    panoramaIndex = (int)State[MAIN_PANORAMA_INDEX];
                }
                else
                {
                    panoramaIndex = -1;
                }

                isNewInstance = false;
            }

        }

        /// <summary>
        /// Overridden OnNavigatedFrom handler
        /// </summary>
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                // Remember the PanoramaItem we were on
                State[MAIN_PANORAMA_INDEX] = RSSFeedsPanorama.SelectedIndex;
            }
        }

        /// <summary>
        /// Handler for restoring panorama state after tombstoning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RSSFeedsPanorama_Loaded(object sender, RoutedEventArgs e)
        {

            if (App.WasTombstoned && panoramaIndex != -1 )
            {
                RSSFeedsPanorama.DefaultItem = RSSFeedsPanorama.Items[panoramaIndex];
                panoramaIndex = -1;
            }
        }

        /// <summary>
        /// Go to the Subscriptions edit page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SubscriptionsPage.xaml?pageId=" + RSSFeedsPanorama.SelectedIndex.ToString(), UriKind.Relative));
        }

        /// <summary>
        /// Handler called when user taps one of the RSS feed icons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            int pageIndex = RSSFeedsPanorama.SelectedIndex;
            PanoramaItem item = RSSFeedsPanorama.SelectedItem as PanoramaItem;
            RSSPage page = item.DataContext as RSSPage;

            RSSFeed feed = button.DataContext as RSSFeed;
            int feedIndex = page.Feeds.IndexOf(feed);

            if (feedIndex >= 0) // Act only on buttons on current page
            {
                NavigationService.Navigate(new Uri("/Views/FeedPivot.xaml?id=" + feedIndex.ToString() + "&page=" + pageIndex.ToString(), UriKind.Relative));
            }
        }
    }
}
