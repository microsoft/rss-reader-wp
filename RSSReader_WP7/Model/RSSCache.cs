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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

namespace RSSReader.Model
{
    /// <summary>
    /// The RSSCache class. The cache is stored in an
    /// ObservableCollection that is displayed in the 
    /// MainView's ListBox. 
    /// </summary>
    [DataContract]
    public class RSSCache
    {
        /// <summary>
        /// Class member for cache of RSS pages
        /// </summary>
        private ObservableCollection<RSSPage> cache;

        /// <summary>
        /// Property for cache of RSS pages
        /// </summary>
        [DataMember]
        public ObservableCollection<RSSPage> Cache
        {
            get
            {
                if (cache == null)
                {
                    cache = new ObservableCollection<RSSPage>();
                }
                return cache;
            }
            set
            {
                if (cache != value)
                {
                    cache = value;
                }
            }
        }

        /// <summary>
        /// Property for first RSS page
        /// </summary>
        [DataMember]
        public RSSPage FirstPage
        {
            get
            {
                return cache[0];
            }
            set {}
        }

        /// <summary>
        /// Property for second RSS page
        /// </summary>
        [DataMember]
        public RSSPage SecondPage
        {
            get
            {
                return cache[1];
            }
            set { }
        }

        /// <summary>
        /// Property for third RSS page
        /// </summary>
        [DataMember]
        public RSSPage ThirdPage
        {
            get
            {
                return cache[2];
            }
            set { }
        }

        /// <summary>
        /// Property for fourth RSS page
        /// </summary>
        [DataMember]
        public RSSPage FourthPage
        {
            get
            {
                return cache[3];
            }
            set { }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RSSCache()
        {
        }

        /// <summary>
        /// Method to a new RSS page to cache
        /// </summary>
        /// <param name="pages">Page to add</param>
        public void AddPages(List<RSSPage> pages)
        {
            foreach (RSSPage p in pages)
            {
                Cache.Add(p);
            }
        }

        /// <summary>
        /// Loads the cache from disk
        /// </summary>
        /// <returns>RSSCache as it was persisted</returns>
        public static RSSCache Load()
        {
            RSSCache cache = null;
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("rssCache.dat", System.IO.FileMode.OpenOrCreate, file))
                {
                    if (stream.Length > 0)
                    {
                        App.Log("Reading cache from file");
                        DataContractSerializer serializer = new DataContractSerializer(typeof(RSSCache));
                        cache = serializer.ReadObject(stream) as RSSCache;
                    }
                }
            }

            // File was not found, create a new cache
            if (cache == null)
            {
                App.Log("Creating a new cache");
                cache = new RSSCache();
            }

            return cache;
        }

        /// <summary>
        /// Persists the cache to disk
        /// </summary>
        public void Save()
        {
            App.Log("Persisting cache to file");
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("rssCache.dat", System.IO.FileMode.Create, file))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(RSSCache));
                    stream.Flush();
                    serializer.WriteObject(stream, this);
                }
            }
        }
    }
}
