/*
 * Copyright © 2011-2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Data;

namespace RSSReader.Model
{
    /// <summary>
    /// A page of RSS feeds
    /// </summary>
    [DataContract(IsReference = true)]
    public class RSSPage
    {
        /// <summary>
        /// The title of the page
        /// </summary>
        [DataMember]
        public String Title { get; set; }

        /// <summary>
        /// Class member for feeds of the page
        /// </summary>
        private List<RSSFeed> feeds;

        /// <summary>
        /// Property for feeds of the page
        /// </summary>
        [DataMember]
        public List<RSSFeed> Feeds 
        { 
            get
            {
                if (feeds == null)
                {
                    feeds = new List<RSSFeed>();
                }
                return feeds;
            }
            set
            {
                if (feeds != value)
                {
                    feeds = value;
                }
            }
        }

        /// <summary>
        /// Class member feeds of the page as CollectionViewSource
        /// </summary>
        private CollectionViewSource feedsView;

        /// <summary>
        /// Property for feeds of the page as CollectionViewSource
        /// </summary>
        public CollectionViewSource FeedsView
        {
            get
            {
                if (feedsView == null)
                {
                    feedsView = new CollectionViewSource();
                    feedsView.Source = Feeds;
                    feedsView.View.Filter = f =>
                        {
                            if (f == null) return true;
                            RSSFeed feed = (RSSFeed)f;
                            return feed.IsVisible;
                        };
                }
                return feedsView;
            }
            set
            {
                if (feedsView != value)
                {
                    feedsView = value;
                }
            }
        }
    }
}
