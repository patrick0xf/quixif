using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace Sample.WP8
{
    public partial class App
    {
        public static PhoneApplicationFrame RootFrame { get; private set; }

        public App()
        {
            InitializeComponent();
            InitializePhoneApplication();
            InitializeLanguage();
        }

        #region Phone application initialization

        private bool _phoneApplicationInitialized;

        private void InitializePhoneApplication()
        {
            if (_phoneApplicationInitialized) return;

            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.Navigated += CheckForResetNavigation;
            _phoneApplicationInitialized = true;
        }

        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            RootVisual = RootFrame;
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private static void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Reset) RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private static void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            RootFrame.Navigated -= ClearBackStackAfterReset;
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh) return;
            while (RootFrame.RemoveBackEntry() != null) {}
        }

        #endregion

        private static void InitializeLanguage()
        {
            RootFrame.Language = XmlLanguage.GetLanguage("en-us");
            RootFrame.FlowDirection = FlowDirection.LeftToRight;
        }
    }
}