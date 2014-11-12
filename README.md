RSS Reader
==========

This example application demonstrates how to build a simple RSS reader for
Windows Phone using Microsoft Silverlight.

While testing for compatibility with Windows Phone 8, it was found out that the
previous implementation did not work after upgrading the project to Windows
Phone 8.0. It seems that there is some kind of a bug in the Panorama control,
causing problems if PanoramaItems are created dynamically using ItemsSource
property. For example, the Panorama's SelectedIndex does not work while using
ItemsSource and also, when navigating back to the panorama, it always resets
back to the first PanoramaItem, not the one which was visible when navigating
deeper.

For this reason, there are now four explicitly defined PanoramaItems in the main
panorama (News, Leisure, Sports, and Tech), and there is a DataMember in
RSSCache for retrieving contents for each of the four pages. Bug report has been
filed to Microsoft regarding the issue.

![Main screen](doc/screenshots/small_main_screen_newbg.jpg?raw=true)&nbsp;
![Item screen](doc/screenshots/item_screen.jpg?raw=true)

This example application is hosted in GitHub:
https://github.com/Microsoft/rss-reader-wp

Developed with:
 * Microsoft Visual Studio 2010 Express for Windows Phone
 * Microsoft Visual Studio Express for Windows Phone 2012.

Compatible with:

 * Windows Phone 7
 * Windows Phone 8

Tested to work on:

 * Nokia Lumia 820 
 * Nokia Lumia 920


Instructions
------------

Make sure you have the following installed:

 * Windows 8
 * Windows Phone SDK 8.0
 * NuGet Package Manager (https://nuget.org/), Visual Studio extension to
 install and update third-party libraries and tools in Visual Studio

To build and run the sample:

 * Open the SLN file
   * File > Open Project, select the file RSSReader.sln
 * Install Windows Phone Toolkit for the project.
   * Right click solution RSSReader in Solution Explorer -> select Manage NuGet
     Packages for Solution
   * Search for 'wptoolkit' and install the 'Windows Phone toolkit' package 
 * Select the target, for example 'Emulator WVGA'.
 * Press F5 to build the project and run it on the Windows Phone Emulator.

To deploy the sample on Windows Phone device:
 * See the official documentation for deploying and testing applications on
 Windows Phone devices at
 http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff402565(v=vs.105).aspx


About the implementation
------------------------

Important folders:

| Folder | Description |
| ------ | ----------- |
| / | Contains the project file, the license information and this file (README.md) |
| RSSReader_WP7 | Root folder for the WP7 implementation files. |
| RSSReader_WP8 | Root folder for the WP8 implementation files. |
| RSSReader_WP7/Dependencies | DLLs needed by the WP7 application. |
| RSSReader_WP7/Model | Model implementation. |
| RSSReader_WP7/Properties | WP7 Application property files. |
| RSSReader_WP7/Resources | Graphic files. |
| RSSReader_WP7/Views | Views of the application (panorama, pivot, pages). |
| RSSReader_WP8/Dependencies | DLLs needed by the WP8 application. |
| RSSReader_WP8/Properties | WP8 Application property files. |

Important files:

| File | Description |
| ---- | ----------- |
| FeedPivot.xaml.cs | Class responsible for displaying available RSS feeds in a pivot. |
| RSSService.cs | Class responsible for parsing RSS feeds and creating feed pages formatted as HTML. |


Known issues
------------

No known issues.


License
-------

See the license file delivered with this project. The license is also available online at https://github.com/Microsoft/rss-reader-wp/blob/master/License.txt


Version history
---------------

 * 1.3.0 Add support for 720p resolution and NuGet Package Restore.
 * 1.2.0 Changes in implementation to support upgrading the application to Windows Phone 8.0.
 * 1.1.0 Bug fixes and changes based on reviews; published on the Nokia Developer website.
 * 1.0.3 Support for Windows Phone OS version 7.1.
 * 1.0.2 Layout changes and bug fixes.
 * 1.0.1 Bug fixes.
 * 1.0.0 First version.
