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
using RSSReader.Model;

namespace RSSReader.Views
{
    /// <summary>
    /// Page to customize which feeds are visible on RSS page
    /// </summary>
    public partial class SubscriptionsPage : PhoneApplicationPage
    {

        /// <summary>
        /// True when this object instance has been just created, otherwise false
        /// </summary>
        private bool isNewInstance = true;

        /// <summary>
        /// Page that is being customized
        /// </summary>
        private RSSPage page;

        /// <summary>
        /// Id of the page
        /// </summary>
        private int pageId;

        /// <summary>
        /// Constructor
        /// </summary>
        public SubscriptionsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Overridden OnNavigatedTo handler
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (isNewInstance)
            {
                pageId = int.Parse(NavigationContext.QueryString["pageId"]);
                page = RSSService.GetRSSPage(pageId);

                if (page != null)
                {
                    PageTitle.Text = page.Title;
                    subscriptionsListBox.ItemsSource = page.Feeds;
                }

                isNewInstance = false;
            }
        }

        /// <summary>
        /// Handler for cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelSettings_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        /// <summary>
        /// Handler for save button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveSettings_Click(object sender, EventArgs e)
        {
            // Update feed visibility
            for (int i = 0; i < subscriptionsListBox.Items.Count; i++)
            {
                page.Feeds[i].IsVisible = ((RSSFeed)subscriptionsListBox.Items[i]).IsVisible;
            }

            // Update the datamodel in the background, otherwise
            // the navigation might fail
            Deployment.Current.Dispatcher.BeginInvoke(() => 
            {
                page.FeedsView.View.Refresh();
            }); 

            NavigationService.GoBack();
        }
    }
}