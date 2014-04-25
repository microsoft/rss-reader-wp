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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;

namespace RSSReader.Model
{
    /// <summary>
    /// RSS feed & item service, handles retrieving and caching of RSS items
    /// </summary>
    public static class RSSService
    {
        /// <summary>
        /// The cache expire time in minutes
        /// </summary>
        private static readonly double EXPIRE_TIME = 30;
    
        /// <summary>
        /// The main data model
        /// </summary>
        public static RSSCache CachedItems { get; set; }

        /// <summary>
        /// Currently selected RSSItem
        /// </summary>
        public static RSSItem SelectedItem { get; set; }

        /// <summary>
        /// Returns global instance of the RSS cache
        /// </summary>
        /// <returns>RSS cache instance</returns>
        public static RSSCache GetDataModel()
        {
            return (RSSCache)Application.Current.Resources["RSSPagesDataSource"];
        }

        /// <summary>
        /// Initializes the feeds. Called from App.xaml.cs 
        /// when the application starts
        /// </summary>
        public static void InitializeFeeds()
        {
            RSSCache pages = GetDataModel();
            
            CachedItems = RSSCache.Load();

            foreach (RSSPage page in CachedItems.Cache)
            {
                pages.Cache.Add(page);
            }

            CachedItems = pages;

            if (CachedItems.Cache.Count == 0)
            {
                FirstLaunch();
            }
        }

        /// <summary>
        /// Persists the cache to disk
        /// </summary>
        public static void PersistCache()
        {
            CachedItems.Save();
        }

        /// <summary>
        /// Method for doing initialization related to first launch of the application
        /// </summary>
        private static void FirstLaunch()
        {
            // On the first launch, just add everything from the OPML file
            StreamResourceInfo xml = App.GetResourceStream(new Uri("/RSSReader;component/sample-opml.xml", UriKind.Relative));
            List<RSSPage> rssPages = ParseOPML(xml.Stream);

            // Add the pages to the data model
            CachedItems.AddPages(rssPages);

            GetFeedImages();
        }

        /// <summary>
        /// Return a RSS page from cache based on id
        /// </summary>
        /// <param name="pageId">Id of the page to return</param>
        /// <returns>RSS page</returns>
        public static RSSPage GetRSSPage(int pageId)
        {
            return CachedItems.Cache[pageId];
        }

        /// <summary>
        /// Returns a RSS feed from cache based on page id and feed id
        /// </summary>
        /// <param name="pageId">Id of the page where the feed resides</param>
        /// <param name="feedId">Id of the feed</param>
        /// <returns>RSS feed</returns>
        public static RSSFeed GetRSSFeed(int pageId, int feedId)
        {
            RSSFeed feed = null;
            int count = 0;
            foreach (RSSFeed f in CachedItems.Cache[pageId].Feeds)
            {
                if (!f.IsVisible)
                {
                    continue;
                }
                else
                {
                    if (count == feedId)
                    {
                        feed = f;
                        break;
                    }
                    count++;
                }
            }

            return feed;
        }

        /// <summary>
        /// Method for retrieving RSS feed information, especially the image of the feed
        /// </summary>
        /// <param name="feed">Feed whose information to fetch</param>
        /// <param name="onGetRSSFeedImageUriCompleted">Action to take when information has been retrieved</param>
        /// <param name="onError">Action to take when an error occurs</param>
        public static void GetRSSFeedImageUri(RSSFeed feed, Action<Uri, RSSFeed> onGetRSSFeedImageUriCompleted = null, Action<Exception> onError = null)
        {
            WebClient webClient = new WebClient();

            webClient.OpenReadCompleted += delegate(object sender, OpenReadCompletedEventArgs e)
            {
                try
                {
                    if (e.Error != null)
                    {
                        if (onError != null)
                        {
                            onError(e.Error);
                        }
                        return;
                    }
                    XmlReader response = XmlReader.Create(e.Result);
                    SyndicationFeed rssFeed = SyndicationFeed.Load(response);
                    if (onGetRSSFeedImageUriCompleted != null)
                    {
                        onGetRSSFeedImageUriCompleted(rssFeed.ImageUrl, feed);
                    }
                }
                catch (Exception error)
                {
                    if (onError != null)
                    {
                        onError(error);
                    }
                }
            };

            webClient.OpenReadAsync(new Uri(feed.URL));
        }

        /// <summary>
        /// Gets the RSS items
        /// </summary>
        /// <param name="feed">The RSS feed</param>
        /// <param name="onGetRSSItemsCompleted">Callback on complete</param>
        /// <param name="onError">Callback for errors</param>
        public static void GetRSSItems(int pageId, int feedId, bool useCache, Action<IEnumerable<RSSItem>> onGetRSSItemsCompleted = null, Action<Exception> onError = null)
        {
            DateTime validUntilThis = DateTime.Now;
            validUntilThis = validUntilThis.AddMinutes(-EXPIRE_TIME);

            RSSFeed feed = GetRSSFeed(pageId, feedId);
            bool feedExists = (feed != null);
            bool feedHasItems = (feedExists && feed.Items != null && feed.Items.Count > 0);
            bool cacheHasExpired = (DateTime.Compare(validUntilThis, feed.Timestamp) > 0);

            // First check if this valid items for this feed exist in the cache already
            if (feedExists && 
                feedHasItems &&
                useCache &&
                !cacheHasExpired &&
                onGetRSSItemsCompleted != null)
            {
                onGetRSSItemsCompleted(feed.Items);
            }
            // Items not found in cache, perform a web request
            else
            {
                WebClient webClient = new WebClient();

                webClient.OpenReadCompleted += delegate(object sender, OpenReadCompletedEventArgs e)
                {
                    try
                    {
                        if (e.Error != null)
                        {
                            if (onError != null)
                            {
                                onError(e.Error);
                            }
                            return;
                        }

                        List<RSSItem> rssItems = new List<RSSItem>();
                        XmlReader response = XmlReader.Create(e.Result);
                        SyndicationFeed rssFeed = SyndicationFeed.Load(response);
                        foreach (SyndicationItem syndicationItem in rssFeed.Items)
                        {
                            // Clean the title in case it includes line breaks
                            string title = syndicationItem.Title.Text;
                            title = title.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", " ");

                            // Check for "enclosure" tag that references an image
                            string image = "";
                            foreach (SyndicationLink link in syndicationItem.Links)
                            {
                                if (link.RelationshipType == "enclosure" &&
                                    link.MediaType.StartsWith( "image/" ) )
                                {
                                    image = link.Uri.ToString();
                                    break;
                                }
                            }

                            RSSItem rssItem = new RSSItem(
                                title,
                                syndicationItem.Summary.Text,
                                syndicationItem.Links[0].Uri.AbsoluteUri,
                                syndicationItem.PublishDate,
                                feed,
                                image);
                            rssItems.Add(rssItem);
                        }

                        // Cache the results
                        feed.Items = rssItems;
                        feed.Timestamp = DateTime.Now;

                        // Call the callback with the list of RSSItems
                        if (onGetRSSItemsCompleted != null)
                        {
                            onGetRSSItemsCompleted(rssItems);
                        }
                    }
                    catch (Exception error)
                    {
                        if (onError != null)
                        {
                            onError(error);
                        }
                    }
                };

                webClient.OpenReadAsync(new Uri(feed.URL));
            }
        }

        /// <summary>
        /// Method to initialize fetching of data & images of RSS feeds
        /// </summary>
        private static void GetFeedImages()
        {
            bool error = false;
            // Get images
            foreach (RSSPage page in CachedItems.Cache)
            {
                foreach (RSSFeed feed in page.Feeds)
                {
                    GetRSSFeedImageUri(
                        feed,
                        (uri, rssFeed) =>
                        {
                            // Assign the URI of the image to the feed if it exists
                            // Otherwise just ignore it, and the default image will be used
                            if (uri != null && Regex.IsMatch(uri.ToString(), "(jpg|jpeg|png)$"))
                            {
                                rssFeed.ImageURL = uri.ToString();
                            }
                        },
                        (exception) =>
                        {
                            error = true;
                        });
                }
            }
            if (error)
            {
                MessageBox.Show("Application failed to retrieve content from server. Please check your network connectivity.");
            }
        }

        /// <summary>
        /// Parses the OPML file and returns it as a list of RSSPages
        /// </summary>
        /// <param name="stream">The OPML file stream</param>
        /// <returns>List of RSSPages</returns>
        private static List<RSSPage> ParseOPML(Stream stream)
        {
            RSSCache cache = GetDataModel();
            List<RSSPage> rssCategories = null;

            if (cache.Cache.Count == 0)
            {
                XDocument xDocument = XDocument.Load(stream);

                // XML parsed using Linq
                rssCategories = (from outline in xDocument.Descendants("outline")
                                 where outline.Attribute("xmlUrl") == null
                                 select new RSSPage()
                                 {
                                     Title = outline.Attribute("title").Value,
                                     Feeds = (from o in outline.Descendants("outline")
                                             select new RSSFeed
                                             {
                                                 URL = o.Attribute("xmlUrl").Value,
                                                 ImageURL = "/Resources/rss-icon.jpg",
                                                 Title = o.Attribute("title").Value,
                                                 IsVisible = true
                                             }).ToList<RSSFeed>()
                                 }).ToList<RSSPage>();
            }

            return rssCategories;
        }

        /// <summary>
        /// Creates the primitive HTML page from the article text.
        /// Displays the first img-tag found (if any) and strips
        /// other HTML away.
        /// </summary>
        /// <param name="article">Article text</param>
        /// <returns>HTML page</returns>
        public static String CreateArticleHTML(RSSItem item)
        {

            String start = @"<html>
                                <head>
                                    <meta name=""Viewport"" content=""width=480; user-scaleable=no; initial-scale=1.0"" />
                                    <style>
                                        * { background: #fff !important; color: #000 !important; width: auto !important; font-size: 1em !important; }
                                        h1 { font-size: 1.125em !important }
                                        img { max-width: 200px !important; display: block; margin: 0 auto; }
                                        .timestamp { font-size: 0.875em !important; font-style: italic; display: block; margin-top: 2em; }
                                    </style>
                                </head>
                                <body>
                                    <table>
                                        <tr><td>";

            String end = @"</td></tr></table></body></html>";

            String headerTemplate = @"<tr><td><h1>{0}</h1></td></tr>";
            String imageTemplate = @"<tr><td>{0}";
            String articleTemplate = @"<span class=""article"">{0}</span></td></tr>";
            String timestampTemplate = @"<tr><td><span class=""timestamp"">{0}</span></td></tr>";

            String header = String.Format(headerTemplate, item.Title);

            // If RSS item references an image in "enclosure" tag, use it. Otherwise try to extract the first image from the article
            // Note about images: if we're using an image retrieved from closure, or the first image we retrieve
            // from the article, doesn't specify image width or height, IE will not apply max-width property. This will
            // result in us showing larger pictures than intended. This could be worked around by using JavaScript resize
            // the images.

            String image = "";
            if (item.Image != null && item.Image != "")
            {
                // Image from enclosure
                String imgTemplate = @"<img src=""{0}"" />";
                image = String.Format( imageTemplate, String.Format(imgTemplate, item.Image ));
            }
            else
            {
                Regex img = new Regex("<img.*?>", RegexOptions.IgnoreCase);
                MatchCollection matches = img.Matches(item.Text);
                if (matches.Count > 0 && matches[0] != null)
                {
                    // Image from article content
                    image = String.Format(imageTemplate, matches[0].Groups[0].Value);
                }
                else
                {
                    // No image
                    image = String.Format(imageTemplate, "");
                }

            }

            String article = String.Format( articleTemplate, HttpUtility.HtmlDecode(Regex.Replace(item.Text, "<.*?>", "") ) );

            String timestamp = String.Format(timestampTemplate, item.Datestamp);

            StringBuilder builder = new StringBuilder();
            builder.Append(start);
            builder.Append(header);
            builder.Append(image);
            builder.Append(article);
            builder.Append(timestamp);
            builder.Append(end);
            return builder.ToString();
        }
    }
}
