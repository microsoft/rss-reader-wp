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
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using RSSReader.Model;

namespace RSSReader.Views
{

    /// <summary>
    /// Page that shows details a single RSS item
    /// </summary>
    public partial class ItemPage : PhoneApplicationPage
    {

        /// <summary>
        /// True when this object instance has been just created, otherwise false
        /// </summary>
        private bool isNewInstance = true;

        /// <summary>
        /// Item whose details to show
        /// </summary>
        RSSItem item;

        /// <summary>
        /// Constructor
        /// </summary>
        public ItemPage()
        {
            InitializeComponent();
            Loaded += ItemPage_Loaded;
        }

        /// <summary>
        /// Overridden OnNavigatedTo handler
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (isNewInstance)
            {
                if( State.ContainsKey( "CurrentItem" ) )
                {
                    item = (RSSItem)State["CurrentItem"];
                }
                else
                {
                    item = RSSService.SelectedItem;
                }

                isNewInstance = false;
            }
        }

        /// <summary>
        /// Overridden OnNavigatedFrom handler
        /// </summary>
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                State["CurrentItem"] = item;
            }
        }

        /// <summary>
        /// Handler for showing RSS item details when view is ready for it
        /// </summary>
        private void ItemPage_Loaded(object sender, RoutedEventArgs e)
        {
            browser.Navigated += ArticleWebBrowser_Navigated;
            if (item.Text != null)
            {
                browser.NavigateToString(RSSService.CreateArticleHTML(item));
            }
        }
        
        /// <summary>
        /// Sets the WebBrowser visible after the content has been loaded,
        /// to prevent the white flicker of empty WebBrowser that still loads it's content
        /// </summary>-
        private void ArticleWebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            browser.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handler for launch button
        /// </summary>
        private void LaunchInBrowser(object sender, EventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri( item.Url, UriKind.RelativeOrAbsolute);
            task.Show();
        }
    }
}