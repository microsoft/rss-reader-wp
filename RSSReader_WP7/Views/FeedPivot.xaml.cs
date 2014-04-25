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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using RSSReader.Model;

namespace RSSReader.Views
{
    /// <summary>
    /// Page that shows the feed pivot
    /// </summary>
    public partial class FeedPivot : PhoneApplicationPage
    {
        /// <summary>
        /// True when this object instance has been just created, otherwise false
        /// </summary>
        private bool isNewInstance = true;

        /// <summary>
        /// Feed currently shown to the user
        /// </summary>
        int feedId;

        /// <summary>
        /// Feed that was shown to the user originally when he/she entered the view
        /// </summary>
        int originalFeedId;

        /// <summary>
        /// Id of the page shown in the pivot
        /// </summary>
        int pageId;

        /// <summary>
        /// Instance of the page shown in the pivot
        /// </summary>
        RSSPage page;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public FeedPivot()
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
                originalFeedId = feedId = int.Parse(NavigationContext.QueryString["id"]);
                pageId = int.Parse(NavigationContext.QueryString["page"]);
                page = RSSService.GetRSSPage(pageId);

                InitializePivotViews(page);

                // Switch to the previously selected PivotItem, if it has been stored
                if (State.ContainsKey("feedPivotIndex"))
                {
                    RSSItemsPivot.SelectedIndex = (int)State["feedPivotIndex"];
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
                // Remember the PivotItem we were on
                State["feedPivotIndex"] = RSSItemsPivot.SelectedIndex;
            }
        }

        /// <summary>
        /// Method that initializes all pivot items
        /// </summary>
        /// <param name="page"></param>
        private void InitializePivotViews(RSSPage page)
        {
            // Perform initialization only on the 1st time
            if (RSSItemsPivot.Items.Count == 0)
            {

                // Initialize PivotItems
                // Initialize them starting from the one that was tapped
                // so that when the PivotItems show up, the active one
                // is already the tapped one. Otherwise we'd have to wait
                // for all of the PivotItems to load and then change the
                // SelectedIndex, which would result in animation in the
                // Pivot control.
                for (int i = 0; i < page.Feeds.Count; i++)
                {
                    int j = originalFeedId + i;
                    if (j >= page.Feeds.Count)
                    {
                        j = j % page.Feeds.Count;
                    }
                    CreatePivotItem(page.Feeds[j]);
                }
            }
        }

        /// <summary>
        /// Method that initializes a single pivot item
        /// </summary>
        /// <param name="f">Feed to initialize the pivot item from</param>
        private void CreatePivotItem(RSSFeed f)
        {
            if (f.IsVisible)
            {
                // Programmatically create the pivot item

                // Note that most of the time you'd want to do this with XAML and data binding,
                // or at least use a data template for the content of the pivot item. However in
                // this case it's better to create everything manually because we are updating the state
                // of pivot items in LoadItems(). This means that we have to access the pivot
                // item controls directly, which is very difficult to do in Silverlight if the
                // controls are being created through data template (this is because FindName()
                // method of FrameworkTemplate is not available in Silverlight). It would probably be
                // possible to go around the need of accessing controls directly through data-binding but
                // that would most likely lead to more code lines than the 16 we need here.
                PivotItem pivotItem = new PivotItem();
                pivotItem.Header = f.Title;
                pivotItem.Margin = new Thickness(0, 0, 0, 0);

                Grid grid = new Grid();
                grid.Margin = new Thickness(25, 0, 0, 0);

                // Add a progressbar for each pivot item, initially hidden
                ProgressBar progressBar = new ProgressBar();
                progressBar.IsIndeterminate = false;
                progressBar.Visibility = Visibility.Collapsed;
                grid.Children.Add(progressBar);

                ListBox listBox = new ListBox();
                listBox.ItemTemplate = (DataTemplate)Resources["RSSItemTemplate"];
                listBox.SelectionChanged += ItemSelectionChanged;
                // Enable tilt effect for the listbox.
                // TilEffect is part of Silverlight Toolkit which does not ship as part of this example. Please see
                // release notes for instructions how to install and use Silverlight Toolkit.
                TiltEffect.SetIsTiltEnabled(listBox, true);
                grid.Children.Add(listBox);

                pivotItem.Content = grid;
                RSSItemsPivot.Items.Add(pivotItem);

            }
        }

        /// <summary>
        /// Pivot selection changed, load feed items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FeedSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFeedId((sender as Pivot).SelectedIndex);
            LoadItems(true);
        }

        /// <summary>
        /// Helper method for updating pivot selection
        /// </summary>
        /// <param name="selectedIndex"></param>
        private void UpdateFeedId(int selectedIndex)
        {
            feedId = originalFeedId + selectedIndex;
            if (feedId >= page.Feeds.Count)
            {
                feedId = feedId % page.Feeds.Count;
            }
        }

        /// <summary>
        /// Item selected, go to ItemPage
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

                if (selected != App.NO_SELECTION)
                {
                    // Deselect the selected item
                    listBox.SelectedIndex = App.NO_SELECTION;
                    NavigationService.Navigate(new Uri("/Views/ItemPage.xaml?item=" + selected.ToString() + "&feed=" + feedId.ToString() + "&page=" + pageId.ToString(), UriKind.Relative));
                }
            }
        }

        /// <summary>
        /// Handler for refresh button
        /// </summary>
        private void AppBarRefresh(object sender, EventArgs e)
        {
            LoadItems(false);
        }

        /// <summary>
        /// Handler for search button
        /// </summary>
        private void AppBarSearch(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SearchPage.xaml?page=" + pageId.ToString() + "&feed=" + feedId.ToString(), UriKind.Relative));
        }
        
        /// <summary>
        /// Loads feed's items to view
        /// </summary>
        /// <param name="useCache">True if cache should be used, otherwise false</param>
        private void LoadItems(bool useCache)
        {
            PivotItem selectedItem = RSSItemsPivot.SelectedItem as PivotItem;
            Grid contentGrid = selectedItem.Content as Grid;
            ProgressBar progressBar = contentGrid.Children.First() as ProgressBar;
            ListBox listBox = contentGrid.Children.Last() as ListBox;
            listBox.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;

            RSSService.GetRSSItems(
                pageId,
                feedId,
                useCache,
                (rssItems) =>
                {
                    listBox.ItemsSource = rssItems;
                    listBox.Visibility = Visibility.Visible;
                    progressBar.Visibility = Visibility.Collapsed;
                    progressBar.IsIndeterminate = false;
                },
                (exception) =>
                {
                    MessageBox.Show("Application failed to retrieve content from server. Please check your network connectivity.");
                }
            );
        }
             
    }
}
