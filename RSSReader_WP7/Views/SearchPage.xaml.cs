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
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using RSSReader.Model;

namespace RSSReader.Views
{
    /// <summary>
    /// Page for filtering RSS items based on a search string
    /// </summary>
    public partial class SearchPage : PhoneApplicationPage
    {

        /// <summary>
        /// True when this object instance has been just created, otherwise false
        /// </summary>
        private bool isNewInstance = true;

        /// <summary>
        /// Id of the feed containing RSS items
        /// </summary>
        private int feedId;

        /// <summary>
        /// Id of the page containing the RSS feed
        /// </summary>
        private int pageId;

        /// <summary>
        /// RSS items contained in the view
        /// </summary>
        public ObservableCollection<RSSItem> Items { get; set; }

        /// <summary>
        /// ViewSource used to show Items collection in a listbox
        /// </summary>
        public CollectionViewSource ItemsView { get; set; }

        /// <summary>
        /// A flag to prevent premature ItemSelectionChanged calls while still setting up the listbox
        /// </summary>
        private bool isLoadingFinished = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchPage()
        {
            InitializeComponent();

            Loaded += new System.Windows.RoutedEventHandler(SearchPage_Loaded);
        }

        /// <summary>
        /// Handler used to open virtual keyboard upon Loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SearchPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Set the focus to the TextBox to get the keyboard open
            SearchBox.Focus();
        }

        /// <summary>
        /// Loads the selected RSSFeed to the ListBox when
        /// navigated to this page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (isNewInstance)
            {

                Items = new ObservableCollection<RSSItem>();
                ItemsView = new CollectionViewSource();

                feedId = int.Parse(NavigationContext.QueryString["feed"]);
                pageId = int.Parse(NavigationContext.QueryString["page"]);

                RSSFeed feed = RSSService.GetRSSFeed(pageId, feedId);

                foreach (RSSItem item in feed.Items)
                {
                    Items.Add(item);
                }

                ItemsView.Source = Items;
                ItemListBox.ItemsSource = ItemsView.View;
                ItemListBox.SelectedIndex = App.NO_SELECTION;

                isLoadingFinished = true;

                isNewInstance = false;
            }

        }

        /// <summary>
        /// Handles moving to ItemPage after an RSSItem has been selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                RSSItem item = (RSSItem)e.AddedItems[0];
                RSSService.SelectedItem = item;

                ListBox listBox = (sender as ListBox);
                int selected = listBox.SelectedIndex;

                if (selected != App.NO_SELECTION && isLoadingFinished)
                {
                    // Deselect the selected item
                    listBox.SelectedIndex = App.NO_SELECTION;
                    isLoadingFinished = false;
                    NavigationService.Navigate(new Uri("/Views/ItemPage.xaml?item=" + selected.ToString() +
                        "&feed=" + feedId.ToString() + "&page=" + pageId.ToString(), UriKind.Relative));
                }
            }
        }

        /// <summary>
        /// Fired whenever search text has changed. Filters RSSItems
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchBox = (TextBox)sender;

            ItemsView.View.Filter = i =>
            {
                if (i == null) return true;
                RSSItem item = (RSSItem)i;
                return (item.Title.ToLower().Contains(searchBox.Text.ToLower()));
            };
        }
    }
}