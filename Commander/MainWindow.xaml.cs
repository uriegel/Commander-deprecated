using System;
using System.Windows;
using Commander.Properties;

namespace Commander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            webbie.Initialize(this, new Uri("http://localhost:9865"));
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.Width > 0)
            {
                this.Left = Settings.Default.Left;
                this.Top = Settings.Default.Top;
                this.Width = Settings.Default.Width;
                this.Height = Settings.Default.Height;
            }
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Left = this.Left;
            Settings.Default.Top = this.Top;
            Settings.Default.Width = this.Width;
            Settings.Default.Height = this.Height;
            Settings.Default.Save();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
