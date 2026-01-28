using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace Loopback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LoopUtil _loop;
        private bool isDirty=false;
        private ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public MainWindow()
        {
            InitializeComponent();
            _loop = new LoopUtil();
            _currentCulture = CultureInfo.CurrentUICulture;
            LoadResources();
            dgLoopback.ItemsSource = _loop.Apps;
            ICollectionView cvApps = CollectionViewSource.GetDefaultView(dgLoopback.ItemsSource);

        }

        private void LoadResources()
        {
            _resourceManager = new ResourceManager("Loopback.Strings", Assembly.GetExecutingAssembly());
            ApplyResources();
        }

        private void ApplyResources()
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("/Strings;component", UriKind.Relative);
            
            var resources = new Dictionary<string, string>();
            resources["WindowTitle"] = _resourceManager.GetString("WindowTitle", _currentCulture);
            resources["SaveButton"] = _resourceManager.GetString("SaveButton", _currentCulture);
            resources["RefreshButton"] = _resourceManager.GetString("RefreshButton", _currentCulture);
            resources["ExemptColumn"] = _resourceManager.GetString("ExemptColumn", _currentCulture);
            resources["AppNameColumn"] = _resourceManager.GetString("AppNameColumn", _currentCulture);
            resources["StatusLabel"] = _resourceManager.GetString("StatusLabel", _currentCulture);
            resources["LanguageButton"] = _resourceManager.GetString("LanguageButton", _currentCulture);
            resources["SelectAllButton"] = _resourceManager.GetString("SelectAllButton", _currentCulture);
            resources["DeselectAllButton"] = _resourceManager.GetString("DeselectAllButton", _currentCulture);

            foreach (var key in resources.Keys)
            {
                this.Resources[key] = resources[key];
            }

            this.Title = resources["WindowTitle"];
            btnLanguage.Content = resources["LanguageButton"];
        }

        private void btnLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentCulture.Name == "zh-CN")
            {
                _currentCulture = new CultureInfo("en");
            }
            else
            {
                _currentCulture = new CultureInfo("zh-CN");
            }
            ApplyResources();
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _loop.Apps)
            {
                app.LoopUtil = true;
            }
            dgLoopback.Items.Refresh();
            isDirty = true;
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _loop.Apps)
            {
                app.LoopUtil = false;
            }
            dgLoopback.Items.Refresh();
            isDirty = true;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isDirty) 
            {
                Log(_resourceManager.GetString("NothingToSave", _currentCulture));
                return; 
            }

            isDirty = false;
            if (_loop.SaveLoopbackState())
            { 
                Log(_resourceManager.GetString("SavedExemptions", _currentCulture));
            }
            else
            { Log(_resourceManager.GetString("ErrorSaving", _currentCulture)); }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _loop.LoadApps();
            dgLoopback.Items.Refresh();
            isDirty = false;
            Log(_resourceManager.GetString("Refreshed", _currentCulture));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (isDirty)
            {
                string title = _resourceManager.GetString("UnsavedChangesTitle", _currentCulture);
                string message = _resourceManager.GetString("UnsavedChangesMessage", _currentCulture);
                MessageBoxResult resp=System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (resp==MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

            }
            _loop.FreeResources();
        }

        private void dgcbLoop_Click(object sender, RoutedEventArgs e)
        {
            isDirty=true;
        }

        private void Log(String logtxt) 
        {
                txtStatus.Text = DateTime.Now.ToString("hh:mm:ss.fff ") + logtxt;
        }

    }
}
