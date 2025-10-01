using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.MarketWatch
{
    public partial class NewsDescription : Form
    {
        private readonly ItemDto _news;

        public NewsDescription(ItemDto news)
        {
            InitializeComponent();
            _news = news;
            LoadNews();
        }

        private void LoadNews()
        {
            if (_news == null) return;

            lblHeadline.Text = _news.caption;

            // Define IST timezone
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            // Convert the datetime to IST
            DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(_news.versionCreated.ToUniversalTime(), istZone);

            // Format and set the label text
            lblDateSource.Text = $"{istTime:dd-MMM-yyyy HH:mm:ss}"; 
            
            txtDescription.Text = _news.fragment;

        }
    }
}
