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
            try
            {
                InitializeComponent();
                _loop = new LoopUtil();
                _currentCulture = CultureInfo.CurrentUICulture;
                LoadResources();
                dgLoopback.ItemsSource = _loop.Apps;
                ICollectionView cvApps = CollectionViewSource.GetDefaultView(dgLoopback.ItemsSource);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadResources()
        {
            try
            {
                _resourceManager = new ResourceManager("Loopback.Strings", Assembly.GetExecutingAssembly());
                ApplyResources();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading resources: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyResources()
        {
            try
            {
                var resources = new Dictionary<string, string>();
                resources["WindowTitle"] = _resourceManager.GetString("WindowTitle", _currentCulture) ?? "Loopback Exemption Manager";
                resources["SaveButton"] = _resourceManager.GetString("SaveButton", _currentCulture) ?? "Save";
                resources["RefreshButton"] = _resourceManager.GetString("RefreshButton", _currentCulture) ?? "Refresh";
                resources["ExemptColumn"] = _resourceManager.GetString("ExemptColumn", _currentCulture) ?? "Exempt";
                resources["AppNameColumn"] = _resourceManager.GetString("AppNameColumn", _currentCulture) ?? "App Name";
                resources["StatusLabel"] = _resourceManager.GetString("StatusLabel", _currentCulture) ?? "Status:";
                resources["LanguageButton"] = _resourceManager.GetString("LanguageButton", _currentCulture) ?? "中文";
                resources["SelectAllButton"] = _resourceManager.GetString("SelectAllButton", _currentCulture) ?? "Select All";
                resources["DeselectAllButton"] = _resourceManager.GetString("DeselectAllButton", _currentCulture) ?? "Deselect All";

                // 更新窗口标题
                this.Title = resources["WindowTitle"];

                // 更新按钮内容
                btnLanguage.Content = resources["LanguageButton"];
                btnSave.Content = resources["SaveButton"];
                btnRefresh.Content = resources["RefreshButton"];
                btnSelectAll.Content = resources["SelectAllButton"];
                btnDeselectAll.Content = resources["DeselectAllButton"];

                // 更新DataGrid列标题
                ((DataGridTemplateColumn)dgLoopback.Columns[0]).Header = resources["ExemptColumn"];
                ((DataGridTextColumn)dgLoopback.Columns[1]).Header = resources["AppNameColumn"];

                // 更新状态栏文本
                ((TextBlock)((StatusBarItem)SBar.Items[0]).Content).Text = resources["StatusLabel"];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying resources: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
