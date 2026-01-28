using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Loopback
{
    public partial class MainWindow : Window
    {
        private LoopUtil _loop;
        private bool _hasUnsavedChanges;
        private bool _isChinese = true;

        public MainWindow()
        {
            InitializeComponent();
            _loop = new LoopUtil();
            dgLoopback.ItemsSource = _loop.Apps;
        }

        private void btnLanguage_Click(object sender, RoutedEventArgs e)
        {
            _isChinese = !_isChinese;
            UpdateUIText();
        }

        private void UpdateUIText()
        {
            btnLanguage.Content = _isChinese ? "English" : "中文";
            btnSelectAll.Content = _isChinese ? "全选" : "Select All";
            btnDeselectAll.Content = _isChinese ? "全不选" : "Deselect All";
            btnSave.Content = _isChinese ? "保存" : "Save";
            btnRefresh.Content = _isChinese ? "刷新" : "Refresh";
            ((DataGridTemplateColumn)dgLoopback.Columns[0]).Header = _isChinese ? "豁免" : "Exempt";
            ((DataGridTextColumn)dgLoopback.Columns[1]).Header = _isChinese ? "应用名称" : "App Name";
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _loop.Apps)
            {
                app.LoopUtil = true;
            }
            dgLoopback.Items.Refresh();
            _hasUnsavedChanges = true;
            Log(_isChinese ? "已全选" : "All selected");
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _loop.Apps)
            {
                app.LoopUtil = false;
            }
            dgLoopback.Items.Refresh();
            _hasUnsavedChanges = true;
            Log(_isChinese ? "已全不选" : "All deselected");
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_hasUnsavedChanges)
            {
                Log(_isChinese ? "没有需要保存的更改" : "No changes to save");
                return;
            }

            _hasUnsavedChanges = false;
            if (_loop.SaveLoopbackState())
            {
                Log(_isChinese ? "已保存" : "Saved successfully");
            }
            else
            {
                Log(_isChinese ? "保存失败" : "Save failed");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _loop.LoadApps();
            dgLoopback.Items.Refresh();
            _hasUnsavedChanges = false;
            Log(_isChinese ? "已刷新" : "Refreshed");
        }

        private void dgcbLoop_Click(object sender, RoutedEventArgs e)
        {
            _hasUnsavedChanges = true;
        }

        private void dgLoopback_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                string title = _isChinese ? "确认" : "Confirm";
                string message = _isChinese ? "有未保存的更改，确定要退出吗？" : "You have unsaved changes. Are you sure you want to exit?";
                if (MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            _loop.FreeResources();
        }

        private void Log(string message)
        {
            txtStatus.Text = $"{DateTime.Now:HH:mm:ss.fff} {message}";
        }
    }
}
