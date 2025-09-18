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
            lblDateSource.Text = $"{_news.versionCreated:dd-MMM-yyyy HH:mm} · {_news.credit}";
            txtDescription.Text = _news.fragment;
            //lblTags.Text = _news.keyword != null ? $"Tags: {_news.keyword}" : "Tags: -";
            lblCopyright.Text = _news.copyrightNotice;
        }
    }
}
