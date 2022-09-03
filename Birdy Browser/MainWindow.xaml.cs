using Birdy_Browser;
using CefSharp;
using CefSharp.Structs;
using CefSharp.Wpf;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
//using static System.Net.Mime.MediaTypeNames;

namespace Birdy_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            CefSettings settings = new();
            SchemeHandlerFactory shf = new();
            shf.mw = this;
            settings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = "birdy",
                SchemeHandlerFactory = shf
            });
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
            ////settings.UserAgent = "Mozilla/5.0 (" + Environment.OSVersion.ToString() + "; Win" + (Environment.Is64BitOperatingSystem ? "64" : "32") + "; " + (Environment.Is64BitOperatingSystem ? "x64" : "x86") + ") AppleWebKit/537.36 (KHTML, like Gecko) Chrome/" + Cef.CefSharpVersion + " /CefSharpBrowser" + Cef.CefSharpVersion;
            //settings.UserAgent = "Mozilla/5.0 (" + Environment.OSVersion.ToString().Replace("Microsoft Windows", "Windows") + "; Win" + (Environment.Is64BitOperatingSystem ? "64" : "32") + "; " + (Environment.Is64BitOperatingSystem ? "x64" : "x86") + ") AppleWebKit/537.36 (KHTML, like Gecko) Chrome/" + Cef.ChromiumVersion + " Safari/537.36";
            //MessageBox.Show(settings.UserAgent);
            Cef.Initialize(settings);

            InitializeComponent();
            initSettings();
        }

        BrowserSettings bsettings;
        public BitmapImage stoploadBM;
        public BitmapImage reloadBM;
        public BitmapImage loadBM;
        public BitmapImage pgBM;
        public dynamic bookmarkList;
        public dynamic settings;
        public dynamic? theme = null;
        Brush fontColor = Brushes.Black;
        bool ctrlkey = false;
        bool shiftkey = false;
        CefSharp.Example.Handlers.DownloadHandler dh = new CefSharp.Example.Handlers.DownloadHandler();
        Dictionary<int, StackPanel> dowit = new();
        Dictionary<int, int> dowprog = new();

        public void initSettings()
        {
            Directory.CreateDirectory(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser");
            if (!File.Exists(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Bookmarks.data"))
            {
                File.WriteAllText(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Bookmarks.data","{}");
            }
            if (!File.Exists(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Settings.data"))
            {
                File.WriteAllText(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Settings.data", "{\"Theme\":\"\",\"LoadFavicons\":true,\"HomePageUrl\":\"birdy://home/\"}");
            }
            if (File.ReadAllText(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Settings.data") != "")
            {
                string json = File.ReadAllText(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Settings.data");
                settings = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                addnewsettingssecs();
                if (File.Exists((string)settings["Theme"]))
                {
                    string jjson = File.ReadAllText((string)settings["Theme"]);
                    theme = Newtonsoft.Json.JsonConvert.DeserializeObject(jjson);
                    var bgf = ((string)theme["FontColor"]).Split(",");
                    fontColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.Parse(bgf[0]), byte.Parse(bgf[1]), byte.Parse(bgf[2]), byte.Parse(bgf[3])));
                    //appmenu.Foreground = fontColor;
                    //ZoomPrct.Foreground = fontColor;
                    //zoomtxt.Foreground = fontColor;
                }
                if (settings["BookmarksBarVisible"] == 0)
                {
                    bbar.Visibility = Visibility.Collapsed;
                }
            }
            if (File.ReadAllText(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Bookmarks.data") != "")
            {
                string json = File.ReadAllText(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Bookmarks.data");
                bookmarkList = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                initBookmarksBar();
            }
        }

        void addnewsettingssecs()
        {
            string json = "{\"HomePageUrl\":\"https://www.startpage.com/\",\"SearchUrl\":\"https://google.com/search?q=%SEARCH%\",\"BookmarksBarVisible\":2}";
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            foreach (var kv in data)
            {
                if (!settings.ContainsKey(kv.Key))
                {
                    settings[kv.Key] = kv.Value;
                }
            }
        }

        public void initBookmarksBar()
        {
            bookmarksBar.Children.Clear();
            int id = 0;
            if (bookmarkList["BookmarksBar"] != null)
            {
                foreach (dynamic bmi in bookmarkList["BookmarksBar"])
                {
                    if (bmi["Type"] == "Item")
                    {
                        Border itemborder = new() { CornerRadius = new CornerRadius(6) };
                        StackPanel stack = new() { Orientation = Orientation.Horizontal };
                        Image iconImg = new() { Height = 15 };
                        try
                        {
                            iconImg.Source = new BitmapImage(new Uri((string)bmi["Icon"]));
                        }
                        catch
                        {

                        }
                        Label nameTg = new() { Content = bmi["Name"], Padding = new Thickness(3),Foreground = fontColor };
                        stack.Children.Add(iconImg);
                        stack.Children.Add(nameTg);
                        itemborder.Child = stack;
                        string urlitm = bmi["Url"];
                        new ClickEventAdder(itemborder).Click += (sender,e) => {
                            TabItem ctab = BrowserTab.SelectedItem as TabItem;
                            ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
                            cb.LoadUrl(urlitm);
                        };
                        ContextMenu context = new();
                        int idl = id;
                        MenuItem miML = new() { Header = "Move Left" };
                        miML.Click += (sender, e) =>
                        {
                            var olddata = bookmarkList["BookmarksBar"][idl];
                            bookmarkList["BookmarksBar"][idl] = bookmarkList["BookmarksBar"][idl - 1];
                            bookmarkList["BookmarksBar"][idl - 1] = olddata;
                            initBookmarksBar();
                        };
                        context.Items.Add(miML);
                        MenuItem miMR = new() { Header = "Move Right" };
                        miMR.Click += (sender, e) =>
                        {
                            var olddata = bookmarkList["BookmarksBar"][idl];
                            bookmarkList["BookmarksBar"][idl] = bookmarkList["BookmarksBar"][idl + 1];
                            bookmarkList["BookmarksBar"][idl + 1] = olddata;
                            initBookmarksBar();
                        };
                        context.Items.Add(miMR);
                        context.Items.Add(new Separator());
                        MenuItem miEdit = new() { Header = "Edit Bookmark"};
                        miEdit.Click += (sender, e) =>
                        {
                            bookmarkadd bookmarkadd = new();
                            bookmarkadd.nameTB.Text = bookmarkList["BookmarksBar"][idl]["Name"];
                            bookmarkadd.iconTB.Text = bookmarkList["BookmarksBar"][idl]["Icon"];
                            bookmarkadd.urlTB.Text = bookmarkList["BookmarksBar"][idl]["Url"];
                            bookmarkadd.conf.Click += (sender, e) =>
                            {
                                bookmarkList["BookmarksBar"][idl]["Name"] = bookmarkadd.nameTB.Text;
                                bookmarkList["BookmarksBar"][idl]["Icon"] = bookmarkadd.iconTB.Text;
                                bookmarkList["BookmarksBar"][idl]["Url"] = bookmarkadd.urlTB.Text;
                                initBookmarksBar();
                                bookmarkadd.Close();
                            };
                            bookmarkadd.ShowDialog();
                        };
                        context.Items.Add(miEdit);
                        MenuItem miDel = new() { Header = "Delete" };
                        miDel.Click += (sender, e) =>
                        {
                            bookmarkList["BookmarksBar"][idl].Remove();
                            initBookmarksBar();
                        };
                        context.Items.Add(miDel);
                        itemborder.ContextMenu = context;
                        bookmarksBar.Children.Add(itemborder);
                    }
                    id++;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bsettings = new();
            bsettings.WindowlessFrameRate = 60;
            bsettings.WebGl = CefState.Enabled;
            //BrowserTab.CloseTabCommand = new RelayCommand(_ => CloseTabCommandAction((TabItem)_));
            //BrowserTab.AddTabCommand = new RelayCommand(_ => AddTabCommandAction());
            dh.OnDownloadUpdatedFired += (object? sender, object[] its) => {
                DownloadItem itm = (DownloadItem)its[0];
                IDownloadItemCallback citm = (IDownloadItemCallback)its[1];
                if (itm.IsComplete || itm.IsCancelled || itm.IsValid == false || itm.IsInProgress == false)
                {
                    Dispatcher.Invoke(() =>
                    {
                        dowsmenu.Items.Remove(dowit[itm.Id]);
                        dowit.Remove(itm.Id);
                        dowprog.Remove(itm.Id);
                        calcprogtb();
                    });
                }
                else
                {
                    if (dowit.ContainsKey(itm.Id))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            (dowit[itm.Id].Children[0] as Label).Content = itm.FullPath != "" ? (new FileInfo(itm.FullPath)).Name : "";
                            (dowit[itm.Id].Children[1] as ProgressBar).Value = itm.PercentComplete;
                            (dowit[itm.Id].Children[2] as Label).Content = getsize(itm.ReceivedBytes) + "/" + getsize(itm.TotalBytes) + " (Speed: " + getsize(itm.CurrentSpeed) + ") " + itm.PercentComplete.ToString() + "%";
                            dowprog[itm.Id] = itm.PercentComplete;
                            calcprogtb();
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() => {
                            StackPanel sp = new();
                            Label name = new() { Content = itm.SuggestedFileName };
                            sp.Children.Add(name);
                            ProgressBar pb = new() { Width = 300, Height = 10, BorderThickness = new Thickness(0) };
                            sp.Children.Add(pb);
                            Label inf = new();
                            sp.Children.Add(inf);
                            dowsmenu.Items.Add(sp);
                            StackPanel stacksatus = new();
                            Button pauseres = new() { Content = "Pause" };
                            pauseres.Click += (sender, e) => {
                                if (pauseres.Content == "Pause")
                                {
                                    citm.Pause();
                                    pauseres.Content = "Resume";
                                }else
                                {
                                    citm.Resume();
                                    pauseres.Content = "Pause";
                                }
                            };
                            stacksatus.Children.Add(pauseres);
                            sp.Children.Add(stacksatus);
                            dowit.Add(itm.Id, sp);
                            dowprog.Add(itm.Id,0);
                            calcprogtb();
                        });
                    }
                }
            };
            if (Environment.GetCommandLineArgs().Length < 2)
            {
                AddTabCommandAction();
            }else
            {
                newTab(Environment.GetCommandLineArgs()[1]);
            }
            if (theme != null)
            {
                if (theme["IconTheme"] == "Black")
                {
                    blackIconTheme();
                } else { whiteIconTheme(); }

                if (theme["BackgroundType"] == "Color")
                {
                    var bg = ((string)theme["Background"]).Split(",");
                    toolbarplc.Background = new SolidColorBrush(Color.FromArgb(byte.Parse(bg[0]), byte.Parse(bg[1]), byte.Parse(bg[2]), byte.Parse(bg[3])));
                }
                if (theme["BackgroundType"] == "Image")
                {
                    ImageBrush image = new ImageBrush(new BitmapImage(new Uri((string)theme["Background"])));
                    if (theme["BackgroundStretch"] == "Fill")
                    {
                        image.Stretch = Stretch.UniformToFill;
                    }
                    if (theme["BackgroundStretch"] == "Stretch")
                    {
                        image.Stretch = Stretch.Fill;
                    }
                    if (theme["BackgroundStretch"] == "None")
                    {
                        image.Stretch = Stretch.None;
                    }
                    if (theme["BackgroundRepeat"] == true)
                    {
                        image.TileMode = TileMode.Tile;
                    }
                    toolbarplc.Background = image;
                }
                var bgb = ((string)theme["TextboxBackground"]).Split(",");
                urlTB.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.Parse(bgb[0]), byte.Parse(bgb[1]), byte.Parse(bgb[2]), byte.Parse(bgb[3])));
                var bgf = ((string)theme["TextboxForeground"]).Split(",");
                urlTB.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.Parse(bgf[0]), byte.Parse(bgf[1]), byte.Parse(bgf[2]), byte.Parse(bgf[3])));
                //var bgm = ((string)theme["MenuBackground"]).Split(",");
                //rbc.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.Parse(bgm[0]), byte.Parse(bgm[1]), byte.Parse(bgm[2]), byte.Parse(bgm[3])));
            }
            else { blackIconTheme(); }
            new ClickEventAdder(goBackBTN).Click += goBack;
            new ClickEventAdder(goForwardBTN).Click += goForward;
            new ClickEventAdder(goUrlTBBTN).Click += goToUrlTB;
            new ClickEventAdder(refreshBTN).Click += refreshOrStop;
            new ClickEventAdder(newtabBTN).Click += (sender,e) => AddTabCommandAction();
            new ClickEventAdder(newbookmarkBTN).Click += (sender,e) =>
            {
                bookmarkadd bookmarkadd = new();
                bookmarkadd.conf.Click += (sender, e) =>
                {
                    JArray item = (JArray)bookmarkList["BookmarksBar"];
                    item.Add(new JObject(
                        new JProperty("Type", "Item"),
                        new JProperty("Name", bookmarkadd.nameTB.Text),
                        new JProperty("Icon", bookmarkadd.iconTB.Text),
                        new JProperty("Url", bookmarkadd.urlTB.Text)));
                    initBookmarksBar();
                    bookmarkadd.Close();
                };
                bookmarkadd.ShowDialog();
            };
        }

        void calcprogtb()
        {
            double avr = 0;
            double cont = 0;
            foreach (var kv in dowprog)
            {
                avr += kv.Value;
                cont += 1.0;
            }
            if (cont != 0)
            {
                tbnInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                tbnInfo.ProgressValue = (double)(avr/cont)/100;
            }else
            {
                tbnInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                tbnInfo.ProgressValue = 0;
            }
            //MessageBox.Show((avr / cont).ToString());
        }


        void blackIconTheme()
        {
            goBackBTN.Source = new BitmapImage(new Uri("arrow_back.png",UriKind.Relative));
            BitmapImage af = new BitmapImage(new Uri("arrow_forward.png", UriKind.Relative));
            goForwardBTN.Source = af;
            goUrlTBBTN.Source = af;
            refreshBTN.Source = new BitmapImage(new Uri("refresh.png", UriKind.Relative));
            BitmapImage plusbtn = new BitmapImage(new Uri("add.png", UriKind.Relative));
            newtabBTN.Source = plusbtn;
            newbookmarkBTN.Source = plusbtn;
            appmenu.SmallImageSource = new BitmapImage(new Uri("menu.png", UriKind.Relative));
            dowsmenu.SmallImageSource = new BitmapImage(new Uri("download.png", UriKind.Relative));
            stoploadBM = new(new Uri("close40.png", UriKind.Relative));
            reloadBM = new(new Uri("refresh.png", UriKind.Relative));
            loadBM = new(new Uri("cached.png", UriKind.Relative));
            pgBM = new(new Uri("description.png", UriKind.Relative));
        }

        void whiteIconTheme()
        {
            goBackBTN.Source = new BitmapImage(new Uri("arrow_back-White.png", UriKind.Relative));
            BitmapImage af = new BitmapImage(new Uri("arrow_forward-White.png", UriKind.Relative));
            goForwardBTN.Source = af;
            goUrlTBBTN.Source = af;
            refreshBTN.Source = new BitmapImage(new Uri("refresh-White.png", UriKind.Relative));
            BitmapImage plusbtn = new BitmapImage(new Uri("add-White.png", UriKind.Relative));
            newtabBTN.Source = plusbtn;
            newbookmarkBTN.Source = plusbtn;
            appmenu.SmallImageSource = new BitmapImage(new Uri("menu-White.png", UriKind.Relative));
            dowsmenu.SmallImageSource = new BitmapImage(new Uri("download-White.png", UriKind.Relative));
            stoploadBM = new(new Uri("close40-White.png", UriKind.Relative));
            reloadBM = new(new Uri("refresh-White.png", UriKind.Relative));
            loadBM = new(new Uri("cached.png", UriKind.Relative));
            pgBM = new(new Uri("description.png", UriKind.Relative));
        }

        void CloseTabCommandAction(TabItem tab)
        {
            Dispatcher.Invoke(() =>
            {
                BrowserTab.Items.Remove(tab);
                (tab.Content as ChromiumWebBrowser).Dispose();
                if (BrowserTab.Items.Count == 0)
                {
                    Close();
                    Environment.Exit(1);
                }
            });
        }
        void AddTabCommandAction()
        {
            newTab((string)settings["HomePageUrl"]);
        }

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!(e.Source is TabItem tabItem))
            {
                return;
            }

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
            }
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Source is TabItem tabItemTarget &&
                e.Data.GetData(typeof(TabItem)) is TabItem tabItemSource &&
                !tabItemTarget.Equals(tabItemSource) &&
                tabItemTarget.Parent is TabControl tabControl)
            {
                int targetIndex = tabControl.Items.IndexOf(tabItemTarget);

                tabControl.Items.Remove(tabItemSource);
                tabControl.Items.Insert(targetIndex, tabItemSource);
                tabItemSource.IsSelected = true;
            }
        }

        void goToUrlTB(object sender, RoutedEventArgs e) {
            Dispatcher.Invoke(() =>
            {
                TabItem ctab = BrowserTab.SelectedItem as TabItem;
                ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
                cb.LoadUrl(urlTB.Text);
            });
        }

        public void gourl(string url)
        {
            Dispatcher.Invoke(() =>
            {
                TabItem ctab = BrowserTab.SelectedItem as TabItem;
                ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
                cb.LoadUrl(url);
            });
        }

        void refreshOrStop(object sender, RoutedEventArgs e) {
            TabItem ctab = BrowserTab.SelectedItem as TabItem;
            ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
            if (cb.IsLoading) {
                cb.Stop();
            }else
            {
                cb.Reload();
            }
        }

        void goBack(object sender, RoutedEventArgs e)
        {
            TabItem ctab = BrowserTab.SelectedItem as TabItem;
            ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
            cb.Back();
        }
        void goForward(object sender, RoutedEventArgs e)
        {
            TabItem ctab = BrowserTab.SelectedItem as TabItem;
            ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
            cb.Forward();
        }

        void newTab(string url)
        {
            Dispatcher.Invoke(() =>
            {
                ChromiumWebBrowser webBrowser = new ChromiumWebBrowser();
                webBrowser.LoadUrl(url);
                TabItem tab = new TabItem();
                StackPanel stackHeader = new();
                Image tabicon = new() { Height = 20 , Width = 20,VerticalAlignment = VerticalAlignment.Center };
                Label tabtitle = new() { Content = "New Tab", Width = 150 };
                Label tabclose = new() { Content = "✖" };
                stackHeader.Orientation = Orientation.Horizontal;
                stackHeader.Children.Add(tabicon);
                stackHeader.Children.Add(tabtitle);
                stackHeader.Children.Add(tabclose);
                new ClickEventAdder(tabclose).Click += (sender, e) => { CloseTabCommandAction(tab); };
                tab.Header = stackHeader;
                tab.Content = webBrowser;
                webBrowser.KeyDown += (object sender, KeyEventArgs e) => { 
                    if (e.Key == Key.F5)
                    {
                        webBrowser.Reload();
                    }
                };
                webBrowser.PreviewMouseWheel += (object sender, MouseWheelEventArgs e) => {
                    if (Keyboard.Modifiers != ModifierKeys.Control)
                        return;

                    if (e.Delta > 0)
                        webBrowser.ZoomInCommand.Execute(null);
                    else
                        webBrowser.ZoomOutCommand.Execute(null);
                    ZoomSlider.Value = webBrowser.ZoomLevel;
                    e.Handled = true;
                };
                BrowserTab.Items.Add(tab);
                BLifeSpanHandler lsh = new BLifeSpanHandler();
                lsh.ti = tab;
                lsh.PopupRequest += newTab;
                lsh.DoCloseEvent += CloseTabCommandAction;
                webBrowser.LifeSpanHandler = lsh;
                webBrowser.DownloadHandler = dh;
                BHandler bHandler = new BHandler();
                bHandler.ti = tab;
                bHandler.win = this;
                webBrowser.TitleChanged += bHandler.TTCHandler;
                webBrowser.LoadingStateChanged += bHandler.LSCHandler;
                webBrowser.AddressChanged += bHandler.ADRHandler;
                DisplayHandler displayH = new();
                displayH.win = this;
                displayH.FaviconChanged += (IList<string> urls) => { if ((bool)settings["LoadFavicons"]) { bHandler.IICHandler(urls); } };
                webBrowser.DisplayHandler = displayH;
                webBrowser.BrowserSettings = bsettings;
                webBrowser.JsDialogHandler = new BJDialogHandler() { mw = this};
                webBrowser.MenuHandler = new CustomMenuHandler();
                BrowserTab.SelectedItem = tab;
                //selectedTab = tab;
            });
        }

        static String getsize(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private void BrowserTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem ctab = BrowserTab.SelectedItem as TabItem;
            if (ctab != null)
            {
                ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
                goBackBTN.IsEnabled = cb.CanGoBack;
                goForwardBTN.IsEnabled = cb.CanGoForward;
                urlTB.Text = cb.Address;
                if (settings["BookmarksBarVisible"] == 2)
                {
                    if (cb.Address == (string)settings["HomePageUrl"])
                    {
                        bbar.Visibility = Visibility.Visible;
                    }else
                    {
                        bbar.Visibility = Visibility.Collapsed;
                    }
                }
                ZoomSlider.Value = cb.ZoomLevel;
            }
        }

        private void opendev(object sender, RoutedEventArgs e)
        {
            TabItem ctab = BrowserTab.SelectedItem as TabItem;
            ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
            cb.ShowDevTools();
        }

        private void opensets(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new();
            settingsWindow.SettingsData = settings;
            settingsWindow.ShowDialog();
            settings = settingsWindow.SettingsData;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F6) { 
                if (urlTB.IsFocused)
                {
                    TabItem ctab = BrowserTab.SelectedItem as TabItem;
                    ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
                    cb.Focus();
                }
                else
                {
                    urlTB.Focus();
                }
            }

            if (e.Key == Key.F11)
            {
                if (WindowStyle == WindowStyle.None)
                {
                    exitFullScreen();
                }else
                {
                    enterFullScreen(false);
                }
            }
            if (e.Key == Key.LeftCtrl)
            {
                ctrlkey = true;
            }
            if (e.Key == Key.LeftShift)
            {
                shiftkey = true;
            }
            if (ctrlkey)
            {
                if (e.Key == Key.T)
                {
                    AddTabCommandAction();
                }
                if (e.Key == Key.Tab)
                {
                    if (shiftkey)
                    {
                        if (BrowserTab.SelectedIndex != 0)
                        {
                            BrowserTab.SelectedIndex -= 1;
                        }
                    }
                    else
                    {
                        if (BrowserTab.SelectedIndex != BrowserTab.Items.Count - 1)
                        {
                            BrowserTab.SelectedIndex += 1;
                        }
                    }
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ctrlkey = false;
            }
            if (e.Key == Key.LeftShift)
            {
                shiftkey = false;
            }
        }

        public void enterFullScreen(bool hidetoolbar)
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Style s = new Style();
            s.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            BrowserTab.ItemContainerStyle = s;
            bookBar.Visibility = Visibility.Collapsed;
            if (hidetoolbar)
            {
                btoolbar.Visibility = Visibility.Collapsed;
            }
        }

        public void exitFullScreen()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Maximized;
            BrowserTab.ItemContainerStyle = null;
            btoolbar.Visibility = Visibility.Visible;
            bookBar.Visibility = Visibility.Visible;
        }

        private void urlTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool isurl = true;
                if (urlTB.Text.Contains(' ')) { isurl = false; }
                if (urlTB.Text.Contains(".") == false && urlTB.Text.Contains(":") == false) { isurl = false; }
                TabItem ctab = BrowserTab.SelectedItem as TabItem;
                ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
                if (isurl)
                {
                    goToUrlTB(sender, new RoutedEventArgs());
                }else
                {
                    searchStr(urlTB.Text);
                }
                cb.Focus();
            }
        }

        public void searchStr(string query)
        {
            Dispatcher.Invoke(() => {
                TabItem ctab = BrowserTab.SelectedItem as TabItem;
                ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
                cb.LoadUrl(((string)settings["SearchUrl"]).Replace("%SEARCH%", query));
            });
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (BrowserTab == null) { return; }
                TabItem ctab = BrowserTab.SelectedItem as TabItem;
                ChromiumWebBrowser cb = ctab.Content as ChromiumWebBrowser;
                cb.ZoomLevel = ZoomSlider.Value;
                ZoomPrct.Content = Math.Floor(cb.ZoomLevel);
            }
            catch
            {

            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(bookmarkList, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Bookmarks.data", output);
            string outputt = Newtonsoft.Json.JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(EnvironmentFolders.GetPath(EnvironmentFolders.SpecialFolder.MyDocuments) + @"\Birdy Browser\Settings.data", outputt);
        }
    }
}

public class BHandler
{
    public TabItem ti { get; set; }
    public Birdy_Browser.MainWindow win { get; set; }

    public void TTCHandler(object sender, DependencyPropertyChangedEventArgs e)
    {
        win.Dispatcher.Invoke(() =>
        {
            ((ti.Header as StackPanel).Children[1] as Label).Content = e.NewValue;
            ((ti.Header as StackPanel).Children[1] as Label).ToolTip = e.NewValue;
        });
    }

    public void IICHandler(IList<string> urls)
    {
        win.Dispatcher.Invoke(() =>
        {
            ((ti.Header as StackPanel).Children[0] as Image).Source = new BitmapImage(new Uri(urls[0]));
        });
    }

    public void LSCHandler(object sender, LoadingStateChangedEventArgs e)
    {
        win.Dispatcher.Invoke(() =>
        {
            ((ti.Header as StackPanel).Children[0] as Image).Source = e.IsLoading ? win.loadBM : win.pgBM;
            if (win.BrowserTab.SelectedItem == ti)
            {
                win.refreshBTN.Source = e.IsLoading ? win.stoploadBM : win.reloadBM;
                win.goBackBTN.IsEnabled = e.CanGoBack;
                win.goForwardBTN.IsEnabled = e.CanGoForward;
            }
        });
    }
    public void ADRHandler(object sender, DependencyPropertyChangedEventArgs e)
    { 
        
        win.Dispatcher.Invoke(() =>
        {
            if (win.BrowserTab.SelectedItem == ti)
            {
                win.urlTB.Text = (string)e.NewValue;
                if (win.settings["BookmarksBarVisible"] == 2)
                {
                    if ((string)e.NewValue == (string)win.settings["HomePageUrl"])
                    {
                        win.bbar.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        win.bbar.Visibility = Visibility.Collapsed;
                    }
                }
            }
        });
    }
}

public class BLifeSpanHandler : ILifeSpanHandler
{
    public event Action<string> PopupRequest;
    public event Action<TabItem> DoCloseEvent;
    public TabItem ti { get; set; }

    public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
    {
        if (PopupRequest != null)
            PopupRequest(targetUrl);
        newBrowser = null;
        return true;
    }

    public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        //throw new NotImplementedException();
    }

    public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        if (browser.IsPopup)
        {
            return false;
        }
        DoCloseEvent(ti);
        chromiumWebBrowser.Dispose();
        return true;
    }

    public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        //throw new NotImplementedException();
    }
}

public class RelayCommand : ICommand
{
    private Action<object> action;
    public RelayCommand(Action<object> action)
    {
        this.action = action;
    }
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        action(parameter);
    }

    public event EventHandler CanExecuteChanged;
}

class DisplayHandler : IDisplayHandler
{
    public event Action<IList<string>> FaviconChanged;
    public Birdy_Browser.MainWindow win { get; set; }
    public void OnAddressChanged(IWebBrowser chromiumWebBrowser, AddressChangedEventArgs addressChangedArgs)
    {
        //throw new NotImplementedException();
    }

    public bool OnAutoResize(IWebBrowser chromiumWebBrowser, IBrowser browser, CefSharp.Structs.Size newSize)
    {
        //throw new NotImplementedException();
        return false;
    }

    public bool OnConsoleMessage(IWebBrowser chromiumWebBrowser, ConsoleMessageEventArgs consoleMessageArgs)
    {
        //throw new NotImplementedException();
        return false;
    }

    public bool OnCursorChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IntPtr cursor, CefSharp.Enums.CursorType type, CursorInfo customCursorInfo)
    {
        //throw new NotImplementedException();
        return false;
    }

    public void OnFaviconUrlChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IList<string> urls)
    {
        FaviconChanged(urls);
    }

    public void OnFullscreenModeChange(IWebBrowser chromiumWebBrowser, IBrowser browser, bool fullscreen)
    {
        win.Dispatcher.Invoke(() =>
        {
            if (fullscreen)
            {
                win.enterFullScreen(true);
            }
            else
            {
                win.exitFullScreen();
            }
        });
    }

    public void OnLoadingProgressChange(IWebBrowser chromiumWebBrowser, IBrowser browser, double progress)
    {
        //throw new NotImplementedException();
    }

    public void OnStatusMessage(IWebBrowser chromiumWebBrowser, StatusMessageEventArgs statusMessageArgs)
    {
        //throw new NotImplementedException();
    }

    public void OnTitleChanged(IWebBrowser chromiumWebBrowser, TitleChangedEventArgs titleChangedArgs)
    {
        //throw new NotImplementedException();
    }

    public bool OnTooltipChanged(IWebBrowser chromiumWebBrowser, ref string text)
    {
        //throw new NotImplementedException();
        return false;
    }
}

public class SchemeHandlerFactory : ISchemeHandlerFactory
{
    Random rndm = new();
    int seccode;
    bool secgen = false;
    public Birdy_Browser.MainWindow mw { get; set; }
    Assembly assembly = Assembly.GetExecutingAssembly();
    string homresourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                    .Single(str => str.EndsWith("BirdyBrowserHomepage.html"));
    public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
    {
        if (!secgen)
        {
            seccode = rndm.Next(100000000, 999999999);
            secgen = true;
        }
        if (schemeName == "birdy")
        {
            if (request.Url.Split("/")[2] == "home")
            {
                Stream streamdat = assembly.GetManifestResourceStream(homresourceName);
                StreamReader readers = new StreamReader(streamdat);
                string hs = "";
                if (mw.bookmarkList["Homepage"] != null)
                {
                    foreach (dynamic bmi in mw.bookmarkList["Homepage"]) {
                        hs += "addShortcut(\"" + ((string)bmi["Icon"]).Replace("\"", "\\\"") + "\",\"" + ((string)bmi["Name"]).Replace("\"", "\\\"") + "\",\"" + ((string)bmi["Url"]).Replace("\"", "\\\"") + "\");";
                    }
                }
                return ResourceHandler.FromString(readers.ReadToEnd().Replace("{SecurityID}", seccode.ToString()) + "<script>" + hs + "</script>");
            }
            if (request.Url.Split("/")[2] == "search")
            {
                mw.searchStr(request.Url.Split("=")[1]);
                return ResourceHandler.FromString("Searching..");
            }
            if (request.Url.Split("/")[2] == "addhomepagebookmark")
            {
                Birdy_Browser.bookmarkadd bma = new();
                if (mw.bookmarkList["Homepage"] == null)
                {
                    mw.bookmarkList.Add(new JProperty("Homepage", new JArray()));
                }
                bma.conf.Click += (sender, e) => {
                    JArray item = (JArray)mw.bookmarkList["Homepage"];
                    item.Add(new JObject(
                        new JProperty("Type", "Item"),
                        new JProperty("Name", bma.nameTB.Text),
                        new JProperty("Icon", bma.iconTB.Text),
                        new JProperty("Url", bma.urlTB.Text)));
                    bma.Close();
                };
                bma.ShowDialog();
                bma.Activate(); 
                mw.gourl("birdy://home");
                return ResourceHandler.FromString("Opening Window..");
            }
            if (request.Url.Split("/")[2] == "removehomepagebookmark")
            {
                if (request.Url.Split("/")[4] == seccode.ToString())
                {
                    mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])].Remove();
                }
                mw.gourl("birdy://home");
                return ResourceHandler.FromString("Removing..");
            }
            if (request.Url.Split("/")[2] == "edithomepagebookmark")
            {
                if (request.Url.Split("/")[4] == seccode.ToString())
                {
                    Birdy_Browser.bookmarkadd bma = new();
                    bma.iconTB.Text = mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])]["Icon"];
                    bma.nameTB.Text = mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])]["Name"];
                    bma.urlTB.Text = mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])]["Url"];
                    bma.conf.Click += (sender, e) => {
                        mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])]["Icon"] = bma.iconTB.Text;
                        mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])]["Name"] = bma.nameTB.Text;
                        mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])]["Url"] = bma.urlTB.Text;
                        bma.Close();
                    };
                    bma.ShowDialog();
                    bma.Activate();
                }

                mw.gourl("birdy://home");
                return ResourceHandler.FromString("Opening Window..");
            }
            if (request.Url.Split("/")[2] == "movehomepagebookmark")
            {
                if (request.Url.Split("/")[5] == seccode.ToString())
                {
                    dynamic olddata = mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])];
                    mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[3])] = mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[4])];
                    mw.bookmarkList["Homepage"][int.Parse(request.Url.Split("/")[4])] = olddata;
                }
                mw.gourl("birdy://home");
                return ResourceHandler.FromString("Moving..");
            }
            return ResourceHandler.FromString("<center><h1>Birdy Browser</h1></center> <center>Version 0.2</center>");
        }

        return new ResourceHandler();
    }
}

public class CustomMenuHandler : IContextMenuHandler
{
    public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
    {
        //model.Clear();

        if (model.Count > 0)
        {
            model.AddSeparator();
        }


        model.AddItem((CefMenuCommand)26501, "Open DevTools");
    }

    public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
    {
        if (commandId == (CefMenuCommand)26501)
        {
            browser.GetHost().ShowDevTools();
            return true;
        }
        return false;
    }

    public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
    {

    }

    public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
    {
        return false;
    }
}

public class BJDialogHandler : IJsDialogHandler
{
    public Birdy_Browser.MainWindow mw { get; set; }
    public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
    {
        return true;
    }

    public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        
    }

    public bool OnJSDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
    {
        int dt;
        if (dialogType == CefJsDialogType.Alert)
            dt = 0;
        else if (dialogType == CefJsDialogType.Confirm)
            dt = 1;
        else
            dt = 2;
        mw.Dispatcher.Invoke(() =>
        {
            Border bdr = new() { Background = Brushes.White, CornerRadius = new CornerRadius(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,Effect = new DropShadowEffect
            {
                Color = new Color { A = 255, R = 0, G = 0, B = 0 },
                ShadowDepth = 0,
                BlurRadius = 5,
                Opacity = 0.2
            }, UseLayoutRounding = false , Padding = new Thickness(5), MinWidth = 300 };
            StackPanel sp = new();
            bdr.Child = sp;
            TextBlock lbl = new() { Text = messageText, TextWrapping = TextWrapping.Wrap,FontSize = 12 };
            ScrollViewer sw = new() { Content = lbl, MaxHeight = 200, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            sp.Children.Add(sw);
            TextBox tb = new() { BorderThickness = new Thickness(0) };
            if (dt == 2)
            {
                tb.Text = defaultPromptText;
                sp.Children.Add(tb);
            }
            StackPanel ctrls = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right};
            if (dt != 0)
            {
                Border cnc = new() { Child = new Label() { Content = "Cancel",Foreground = Brushes.White }, Background = Brushes.Orange, CornerRadius = new CornerRadius(15), Padding = new Thickness(3) };
                ctrls.Children.Add(cnc);
                new Birdy_Browser.ClickEventAdder(cnc).Click += (sender, e) => {
                    callback.Continue(false, "");
                    mw.mainGrid.Children.Remove(bdr);
                };
            }
            Border ok = new() { Child = new Label() { Content = "OK", Foreground = Brushes.White }, Background = Brushes.Orange, CornerRadius = new CornerRadius(15),Padding = new Thickness(3) };
            ctrls.Children.Add(ok);
            new Birdy_Browser.ClickEventAdder(ok).Click += (sender, e) => {
                callback.Continue(true, tb.Text);
                mw.mainGrid.Children.Remove(bdr);
            };
            sp.Children.Add(ctrls);
            mw.mainGrid.Children.Add(bdr);
        });
        return true;
    }

    public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        
    }
}