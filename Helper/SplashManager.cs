using System;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public static class SplashManager
    {
        private static SplashForm _splashForm;

        public static void Show(Form parent, string title = "Please Wait", string message = "Loading ...")
        {
            if (_splashForm == null || _splashForm.IsDisposed)
            {
                _splashForm = new SplashForm(title, message);
                _splashForm.CenterToParent(parent);
                _splashForm.Show(parent);
                _splashForm.TopMost = true;
                Application.DoEvents();
            }
        }

        public static void Update(string title, string message)
        {
            if (_splashForm != null && !_splashForm.IsDisposed)
            {
                _splashForm.UpdateMessage(title, message);
            }
        }

        public static void Hide()
        {
            if (_splashForm != null && !_splashForm.IsDisposed)
            {
                _splashForm.SafeHide();
                _splashForm.Close();
                _splashForm.Dispose();
                _splashForm = null;
            }
        }
    }
}