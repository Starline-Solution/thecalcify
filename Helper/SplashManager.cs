using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public static class SplashManager
    {
        private static SplashForm _splashForm;
        private static Thread _uiThread;
        private static ManualResetEvent _ready = new ManualResetEvent(false);
        public class SplashToken
        {
            public bool AllowShow = false;
            public DateTime Start;
        }


        public static void Show(Form parentForm = null,string title = "Please Wait", string message = "Loading ...")
        {
            if (_uiThread != null) return;

            _ready.Reset();

            _uiThread = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                _splashForm = new SplashForm(title, message);

                // Signal that the form is ready
                _ready.Set();

                Application.Run(_splashForm);
            });

            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.IsBackground = true;
            _uiThread.Start();

            // Wait until form is fully created
            _ready.WaitOne();
        }

        public static void Update(string title, string message)
        {
            if (_splashForm == null || !_splashForm.IsHandleCreated)
                return;

            try
            {
                _splashForm.BeginInvoke(new Action(() =>
                {
                    _splashForm.UpdateMessage(title, message);
                }));
            }
            catch { }
        }

        public static void Hide()
        {
            if (_splashForm == null) return;

            try
            {
                // Keep a local reference so even if _splashForm becomes null, lambda still works
                var form = _splashForm;

                if (form.IsHandleCreated)
                {
                    form.BeginInvoke(new Action(() =>
                    {
                        form.SafeHide();
                        form.Close();
                    }));
                }
            }
            catch { }

            _splashForm = null;
            _uiThread = null;
        }

        public static SplashToken ShowSmart(Form parent, string title = "Loading", string message = "Please wait...")
        {
            var token = new SplashToken();
            token.Start = DateTime.Now;

            // Show splash only if operation lasts more than 200ms
            Task.Delay(200).ContinueWith(_ =>
            {
                if (token.AllowShow)
                {
                    Show(parent, title, message);
                }
            });

            return token;
        }

        public static async void HideSmart(SplashToken token)
        {
            token.AllowShow = false;

            if (_splashForm != null)
            {
                // Ensure minimum time visible: 300ms
                var elapsed = DateTime.Now - token.Start;
                if (elapsed.TotalMilliseconds < 300)
                {
                    await Task.Delay(300 - (int)elapsed.TotalMilliseconds);
                }

                Hide(); // normal hide
            }
        }


    }
}
