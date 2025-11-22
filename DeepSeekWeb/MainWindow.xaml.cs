using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;

namespace DeepSeekWeb
{
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
            ApplySystemTheme();
        }

        private void ApplySystemTheme()
        {
            // Ждем полной загрузки окна
            SourceInitialized += (s, e) =>
            {
                SetDarkModeAccordingToSystem();
            };
        }

        private void SetDarkModeAccordingToSystem()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
        
                // Проверяем тему системы
                bool isDarkTheme = IsSystemUsingDarkTheme();
        
                int darkMode = isDarkTheme ? 1 : 0;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
        
                // Устанавливаем единый заголовок
                Title = "DeepSeekWeb";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Theme detection failed: {ex.Message}");
            }
        }

        private bool IsSystemUsingDarkTheme()
        {
            try
            {
                // Проверяем реестр на тему системы
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                
                if (key?.GetValue("AppsUseLightTheme") is int themeValue)
                {
                    return themeValue == 0; // 0 = темная тема, 1 = светлая
                }
            }
            catch
            {
                // Если не получилось - предполагаем светлую тему
            }
            return false;
        }

        private async void InitializeWebView()
        {
            try
            {
                // Ждем инициализации WebView2
                await DeepSeekWebView.EnsureCoreWebView2Async();
                
                // Настройки WebView2
                DeepSeekWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                
                // Опционально: обработчик навигации
                DeepSeekWebView.NavigationStarting += (sender, e) =>
                {
                    Console.WriteLine($"Navigating to: {e.Uri}");
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}