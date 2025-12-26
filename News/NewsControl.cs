using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;
using thecalcify.MarketWatch;

namespace thecalcify.News
{
    public partial class NewsControl : UserControl
    {
        private readonly string _username, _password;
        private string _type { get; set; }
        private static readonly string apiUrl = APIUrl.ApplicationURL;
        private static readonly HttpClient client = new HttpClient();
        private string _token;
        private CancellationTokenSource _cts { get; set; }
        private int pageSize = 30;
        private int pageRefreshDelay = 30000;
        private string PrevCursor = string.Empty;
        private bool buttonClicked = false;
        private string categoryLiteral = string.Empty;
        private string subcategoryLiteral = string.Empty;
        private string categoryCode = string.Empty;
        private string subcategoryCode = string.Empty;
        private string startdateRange = string.Empty;
        private string todateRange = string.Empty;
        private string cursor = string.Empty;
        private int currentPage = 1; // Start from page 1
        private Task _fetchTask; // Store task when you start it

        public NewsControl(string username, string password, string token, string type)
        {
            InitializeComponent();

            _username = username;
            _password = password;
            _token = token;
            _type = type;

            this.Load += NewsControl_Load;

            if (type == "history")
            {
                lblCategory.Visible = true;
                cmbCategory.Visible = true;
                lblSubCategory.Visible = true;
                cmbSubCategory.Visible = true;
                btnSearchNews.Visible = true;
                btnRefresh.Visible = false;
                btnNextPage.Visible = true;
                btnPrevPage.Visible = true;
                fromTextbox.Visible = true;
                //fromcalender.Visible = true;
                //tocalender.Visible = true;
                todateTextbox.Visible = true;


                if (dgvNews.Columns.Contains("DVGCategory"))
                    dgvNews.Columns["DVGCategory"].Visible = false;

                if (dgvNews.Columns.Contains("DVGSubCategory"))
                    dgvNews.Columns["DVGSubCategory"].Visible = false;


            }
            else
            {
                lblPageInfo.Visible = false;
                btnRefresh.Visible = true;

                if (dgvNews.Columns.Contains("DVGCategory"))
                    dgvNews.Columns["DVGCategory"].Visible = false;

                if (dgvNews.Columns.Contains("DVGSubCategory"))
                    dgvNews.Columns["DVGSubCategory"].Visible = false;

                _cts = new CancellationTokenSource();

            }

            dgvNews.EnableHeadersVisualStyles = false;
            dgvNews.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
            dgvNews.GridColor = Color.Gainsboro;
            dgvNews.RowHeadersVisible = false;
            dgvNews.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 2. Header Style (Teal Background, WhiteSmoke Text, Sans Serif Bold 10pt)
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            headerStyle.BackColor = Color.FromArgb(81, 213, 220); // Teal
            headerStyle.ForeColor = Color.WhiteSmoke;
            headerStyle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            headerStyle.SelectionBackColor = Color.FromArgb(81, 213, 220);
            headerStyle.SelectionForeColor = Color.WhiteSmoke;
            headerStyle.WrapMode = DataGridViewTriState.True;

            dgvNews.ColumnHeadersDefaultCellStyle = headerStyle;
            dgvNews.ColumnHeadersHeight = 40; // Match Home Height

            // 3. Default Row Style (White Background, Black Text, Sans Serif 10pt)
            DataGridViewCellStyle rowStyle = new DataGridViewCellStyle();
            rowStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            rowStyle.BackColor = Color.White;
            rowStyle.ForeColor = Color.Black;
            rowStyle.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular);
            rowStyle.SelectionBackColor = Color.FromArgb(81, 213, 220); // Hover/Select Color
            rowStyle.SelectionForeColor = Color.WhiteSmoke;
            rowStyle.WrapMode = DataGridViewTriState.False;

            dgvNews.DefaultCellStyle = rowStyle;
            dgvNews.RowTemplate.Height = 36; // Match Home Row Height

            // 4. Alternating Row Style (Very Light Gray)
            dgvNews.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Loop through all columns and disable sorting
            foreach (DataGridViewColumn column in dgvNews.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvNews.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Explicitly set column FillWeights again
            dgvNews.Columns[0].FillWeight = 20;  // Time
            dgvNews.Columns[1].FillWeight = 80;  // Title


            dgvNews.Columns[0].MinimumWidth = 330;

            // Configure HttpClient once
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            btnSearchNews.Click += BtnSearchNews_Click;
            //UpdatePageInfo(1, pageSize);

            // Smooth scrolling
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                dgvNews,
                new object[] { true }
            );

        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Prepare the API request for categories
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}Client/Reuters/Categories");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    // Send async request
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                        response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        thecalcify thecalcify = thecalcify.CurrentInstance;
                        thecalcify.DisconnectESCToolStripMenuItem_Click(null, null);
                        MessageBox.Show("Session expired. Please log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Read response as string
                    string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    // Deserialize into CategoriesResponse
                    var categoriesResponse = JsonConvert.DeserializeObject<CategoriesResponse>(jsonString);

                    // Access categories from response
                    var categories = categoriesResponse?.Data?.FilterOptions?.Categories;

                    if (categories != null && !this.IsDisposed && this.IsHandleCreated)
                    {
                        // Update ComboBox on UI thread
                        if (cmbCategory.InvokeRequired)
                        {
                            cmbCategory.Invoke(new Action(() =>
                            {
                                try
                                {
                                    cmbCategory.Items.Clear();
                                    cmbCategory.Items.Add("All");
                                    foreach (var cat in categories)
                                        cmbCategory.Items.Add(cat);

                                    cmbCategory.DisplayMember = "Literal";
                                    cmbCategory.ValueMember = "Code";
                                }
                                catch (Exception ex)
                                {
                                    ApplicationLogger.Log($"[LoadCategoriesAsync_Invoke] Error: {ex.Message}");
                                }
                            }));
                        }
                        else
                        {
                            try
                            {
                                cmbCategory.Items.Clear();
                                cmbCategory.Items.Add("All");
                                foreach (var cat in categories)
                                    cmbCategory.Items.Add(cat);

                                cmbCategory.DisplayMember = "Literal";
                                cmbCategory.ValueMember = "Code";
                            }
                            catch (Exception ex)
                            {
                                ApplicationLogger.Log($"[LoadCategoriesAsync_Direct] Error: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[LoadCategoriesAsync] Error: {ex.Message}");
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }

        private async void NewsControl_Load(object sender, EventArgs e)
        {
            try
            {
                // Load categories on form load
                await LoadCategoriesAsync();

                if (string.IsNullOrEmpty(_type))
                {
                    // Start periodic fetch
                    _fetchTask = PeriodicFetchAsync(_cts.Token);
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[NewsControl_Load] Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbCategory != null)
                {
                    ClearSubCategories();
                    var selectedCategory = cmbCategory.SelectedItem as Category;

                    if (cmbCategory.SelectedItem.ToString().Equals("All"))
                    {
                        cmbSubCategory.Items.Clear();
                        cmbSubCategory.Items.Add("All");
                        cmbSubCategory.Enabled = true;
                        cmbSubCategory.SelectedIndex = 0;
                    }
                    else if (selectedCategory != null && selectedCategory.SubCategories != null && selectedCategory.SubCategories.Count > 0)
                    {
                        cmbSubCategory.Items.Clear();
                        foreach (var subCat in selectedCategory.SubCategories)
                        {
                            cmbSubCategory.Items.Add(subCat);
                        }

                        // Show the Literal property in the combobox
                        cmbSubCategory.DisplayMember = "Literal";
                        cmbSubCategory.ValueMember = "Code";
                        cmbSubCategory.Enabled = true;
                    }
                    else
                    {
                        ClearSubCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[CmbCategory_SelectedIndexChanged] Error: {ex.Message}");
            }
        }

        private void ClearSubCategories()
        {
            try
            {
                if (cmbCategory != null)
                {
                    cmbSubCategory.Items.Clear();
                    cmbSubCategory.Items.Add("------Null------");
                    cmbSubCategory.DisplayMember = null; // Reset because it's just a string now
                    cmbSubCategory.ValueMember = null;
                    cmbSubCategory.Enabled = false;
                    cmbSubCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[ClearSubCategories] Error: {ex.Message}");
            }
        }

        private async void BtnSearchNews_Click(object sender, EventArgs e)
        {
            try
            {
                startdateRange = fromTextbox.Text.Replace("From Date", "");
                todateRange = todateTextbox.Text.Replace("To Date", "");
                btnNextPage.Enabled = true;
                btnPrevPage.Enabled = true;
                lblPageInfo.Visible = true;

                currentPage = 1;


                // Example category/subcategory selection
                Category category = cmbCategory.SelectedItem as Category;
                Category subCategory = cmbSubCategory.SelectedItem as Category;

                buttonClicked = true;

                if (category != null)
                {
                    categoryLiteral = category.Literal;
                    categoryCode = category.Code;
                }
                else
                {
                    categoryLiteral = string.Empty;
                    categoryCode = string.Empty;
                    cmbCategory.SelectedIndex = 0;
                }

                if (subCategory != null)
                {
                    subcategoryCode = subCategory.Code ?? string.Empty;
                    subcategoryLiteral = subCategory.Literal ?? string.Empty;
                }
                else
                {
                    subcategoryLiteral = string.Empty;
                    subcategoryCode = string.Empty;
                    //cmbSubCategory.SelectedIndex = 0;
                }


                // Fetch news data and update grid
                await FetchNewsDataAndUpdateGrid(categoryCode ?? string.Empty, subcategoryCode ?? string.Empty, pageSize, string.Empty, $"{startdateRange}-{todateRange}");

                UpdatePageInfo(currentPage, pageSize);
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[BtnSearchNews_Click] Error: {ex.Message}");
            }
        }

        private async Task PeriodicFetchAsync(CancellationToken cancellationToken)
        {
            //string cursor = string.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {

                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(async () =>
                        {
                            // Fetch periodically and update cursor
                            await FetchNewsDataAndUpdateGrid(categoryCode, subcategoryCode, pageSize, string.Empty, string.Empty);

                            //newsUpdateLable.Visible = true;
                            //newsUpdateLable.Text = $"Last News Updated At: {Common.ParseToDate(DateTime.Now.ToString()).ToString()}";
                        }));
                    }
                    else
                    {
                        // Fetch periodically and update cursor
                        await FetchNewsDataAndUpdateGrid(categoryCode, subcategoryCode, pageSize, string.Empty, string.Empty);

                        //newsUpdateLable.Visible = true;
                        //newsUpdateLable.Text = $"Last News Updated At: {Common.ParseToDate(DateTime.Now.ToString()).ToString()}";
                    }

                    //UpdatePageInfo(1, pageSize);
                    //lastPageSize = 1;


                    if (PrevCursor.Equals(cursor))
                    {
                        // Same cursor as before, no changes
                    }

                    PrevCursor = cursor;
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"[PeriodicFetchAsync_Fetch] Error: {ex.Message}");
                }

                try
                {
                    await Task.Delay(pageRefreshDelay, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // Task was canceled, exit loop
                    break;
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"[PeriodicFetchAsync_Delay] Error: {ex.Message}");
                }
            }
        }

        private async Task FetchNewsDataAndUpdateGrid(string category, string subcategory, int pageSize, string cursorValue, string dateRange)
        {
            newsSearch.Clear();

            try
            {
                // Build API URL
                string baseUrl = BuildReutersApiUrl($"{apiUrl}Client/Reuters/Items", pageSize, category, subcategory, cursorValue, dateRange);

                // Send request
                HttpResponseMessage response = await client.GetAsync(baseUrl);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                              response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                              response.StatusCode == HttpStatusCode.NotFound ||
                              !response.IsSuccessStatusCode)
                {
                    thecalcify thecalcify = thecalcify.CurrentInstance;
                    thecalcify.DisconnectESCToolStripMenuItem_Click(null, null);
                    MessageBox.Show("Session expired. Please log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid] Session expired. due to {response.StatusCode}");
                }

                // Read and parse response
                string json = await response.Content.ReadAsStringAsync();

                ReutersResponse result = null;
                try
                {
                    // Handle double-encoded JSON
                    string innerJson = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                    result = System.Text.Json.JsonSerializer.Deserialize<ReutersResponse>(innerJson);
                    cursor = result.Data.Search.PageInfo.EndCursor;
                    if (!result.Data.Search.PageInfo.HasNextPage)
                        btnNextPage.Enabled = false;

                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid_Deserialize] Error: {ex.Message}");
                }

                if (result?.Data?.Search?.Items == null || result.Data.Search.Items.Count == 0)
                {
                    MessageBox.Show("No related data found.....", "News Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbCategory.SelectedItem = "All";

                    currentPage = 1;
                    UpdatePageInfo(currentPage, pageSize);

                    // Call search button again with new category
                    BtnSearchNews_Click(this, EventArgs.Empty);
                    ApplicationLogger.Log("[FetchNewsDataAndUpdateGrid] No items found.");
                }

                // Define IST time zone (Windows systems)
                TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var newsItems = result.Data.Search.Items;
                if (string.IsNullOrEmpty(dateRange))
                {
                    // Parse and compare each item's SortTimestamp in IST
                    newsItems = result.Data.Search.Items
                        .OrderBy(x => TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.Parse(x.SortTimestamp, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                            istZone))
                        .ToList();
                }
                else
                {
                    // Parse and compare each item's SortTimestamp in IST
                    newsItems = result.Data.Search.Items
                        .OrderByDescending(x => TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.Parse(x.SortTimestamp, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                            istZone))
                        .ToList();
                }

                // If no items for today, do not display and disable pagination
                if (newsItems.Count == 0)
                {
                    btnNextPage.Enabled = false;
                    // Optional: clear any UI bindings or lists, if needed
                    // listView.Items.Clear(); or similar
                    return;
                }
                // Proceed with displaying `newsItems`...




                if (this.InvokeRequired)
                {
                    // Update DataGridView on UI thread
                    _ = Invoke((Action)(() =>
                    {
                        try
                        {
                            if (this.IsDisposed || !this.IsHandleCreated || dgvNews.IsDisposed)
                            {
                                if (string.IsNullOrEmpty(_type))
                                {
                                    _cts?.Cancel(); // stop previous periodic fetch
                                                    // Restart periodic fetch after reload
                                    _cts = new CancellationTokenSource();
                                    _fetchTask = Task.Run(() => PeriodicFetchAsync(_cts.Token));
                                }
                                //ApplicationLogger.Log("[FetchNewsDataAndUpdateGrid] News Restarted.");
                                return;
                            }


                            dgvNews.Rows.Clear();
                            foreach (var item in newsItems)
                            {
                                // Convert date format (assuming FirstCreated is a valid datetime string)
                                DateTimeOffset dto = DateTimeOffset.Parse(item.SortTimestamp);
                                DateTimeOffset istTime = dto.ToOffset(TimeSpan.FromHours(5.5)); // Convert to IST time
                                string formattedTime = Common.ParseToDate(istTime.ToString()).ToString();

                                // Get the selected category and subcategory names
                                var selectedCategory = cmbCategory.SelectedItem as Category;
                                string categoryName = "N/A";
                                if (selectedCategory?.Literal != null && buttonClicked && categoryLiteral != null)
                                    categoryName = categoryLiteral;

                                var selectedSubCategory = cmbSubCategory.SelectedItem as Category;
                                string subcategoryName = "N/A";
                                if (selectedSubCategory?.Literal != null && buttonClicked && subcategoryLiteral != null)
                                    subcategoryName = subcategoryLiteral;

                                if (dgvNews.Columns.Count == 0)
                                {
                                    dgvNews.Columns.Add("Time", "Time");
                                    dgvNews.Columns.Add("Title", "Title");
                                    dgvNews.Columns.Add("Category", "Category");
                                    dgvNews.Columns.Add("SubCategory", "SubCategory");
                                }

                                dgvNews.Rows.Insert(0,
                                formattedTime,    // Time
                                item.HeadLine,    // Title
                                categoryName,     // Category
                                subcategoryName   // SubCategory
                            );

                                // Tag the row with the news item for future reference
                                dgvNews.Rows[0].Tag = item;
                            }
                        }
                        catch (Exception ex)
                        {
                            ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid_Invoke] Error: {ex.Message}");
                            ApplicationLogger.LogException(ex);
                        }
                    }));
                }
                else
                {
                    try
                    {

                        if (this.IsDisposed || !this.IsHandleCreated || dgvNews.IsDisposed)
                        {
                            if (string.IsNullOrEmpty(_type))
                            {
                                _cts?.Cancel(); // stop previous periodic fetch
                                                // Restart periodic fetch after reload
                                _cts = new CancellationTokenSource();
                                _fetchTask = Task.Run(() => PeriodicFetchAsync(_cts.Token));
                                //ApplicationLogger.Log("[FetchNewsDataAndUpdateGrid] News Restarted."); 
                            }
                            return;
                        }

                        dgvNews.Rows.Clear();
                        foreach (var item in newsItems)
                        {
                            // Convert date format (assuming FirstCreated is a valid datetime string)
                            DateTimeOffset dto = DateTimeOffset.Parse(item.SortTimestamp);
                            DateTimeOffset istTime = dto.ToOffset(TimeSpan.FromHours(5.5)); // Convert to IST time
                            string formattedTime = Common.ParseToDate(istTime.ToString()).ToString();

                            // Get the selected category and subcategory names
                            var selectedCategory = cmbCategory.SelectedItem as Category;
                            string categoryName = "N/A";
                            if (selectedCategory?.Literal != null && buttonClicked && categoryLiteral != null)
                                categoryName = categoryLiteral;

                            var selectedSubCategory = cmbSubCategory.SelectedItem as Category;
                            string subcategoryName = "N/A";
                            if (selectedSubCategory?.Literal != null && buttonClicked && subcategoryLiteral != null)
                                subcategoryName = subcategoryLiteral;


                            if (dgvNews.Columns.Count == 0)
                            {
                                dgvNews.Columns.Add("Time", "Time");
                                dgvNews.Columns.Add("Title", "Title");
                                dgvNews.Columns.Add("Category", "Category");
                                dgvNews.Columns.Add("SubCategory", "SubCategory");
                            }

                            dgvNews.Rows.Insert(0,
                            formattedTime,    // Time
                            item.HeadLine,    // Title
                            categoryName,     // Category
                            subcategoryName   // SubCategory
                        );

                            // Tag the row with the news item for future reference
                            dgvNews.Rows[0].Tag = item;
                        }
                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid_Invoke] Error: {ex.Message}");
                        ApplicationLogger.LogException(ex);
                    }
                    finally
                    {

                    }
                }

            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[FetchNewsDataAndUpdateGrid] Error: {ex.Message}");
            }
        }

        private async void DgvNews_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Ignore header row clicks
                if (e.RowIndex < 0) return;

                // Get the clicked row and its NewsItem
                var row = dgvNews.Rows[e.RowIndex];
                var selected = row.Tag as NewsItem;
                if (selected == null) return;

                // Fetch full news details by VersionGuid
                NewsCategoryItem fullNews = await GetNewsItemAsync(selected.VersionedGuid);
                //ItemDto fullNews = await GetNewsByVersionGuidAsync(selected.VersionedGuid);

                // Show the details in a new form if available
                if (fullNews != null)
                {
                    using (var frm = new NewsDescription(fullNews)) // NewsDescription constructor should accept NewsCategoryItem
                    {
                        frm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[dgvNews_CellDoubleClick] Error: {ex.Message}");
            }
        }

        public async Task<NewsCategoryItem> GetNewsItemAsync(string id)
        {
            string url = $"{apiUrl}Client/Reuters/NewsDescription?id={Uri.EscapeDataString(id)}";

            // Set up request
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserialize the response to the NewsCategoryResponse object
            var newsResponse = System.Text.Json.JsonSerializer.Deserialize<NewsCategoryResponse>(jsonString, options);

            // Ensure bodyXhtmlRich is checked, and fall back to fragment if null or empty
            var newsItem = newsResponse?.Data?.Item;
            if (newsItem != null && (string.IsNullOrEmpty(newsItem.BodyXhtmlRich) || IsEmptyBodyHtml(newsItem.BodyXhtmlRich)))
            {
                // If BodyXhtmlRich is null, empty, or contains an empty <body/> tag, use the fragment
                newsItem.BodyXhtmlRich = newsItem.Fragment;
            }
            return newsItem;
        }

        // Helper function to check if the bodyXhtmlRich content is empty (i.e., contains only <body/>)
        private static bool IsEmptyBodyHtml(string bodyXhtmlRich)
        {
            // Check if the <body> tag contains only empty content
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(bodyXhtmlRich);

            // Look for the <body> element and check its inner content
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            return bodyNode == null || string.IsNullOrWhiteSpace(bodyNode.InnerHtml);
        }

        private async Task<ItemDto> GetNewsByVersionGuidAsync(string versionGuid)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Prepare request to fetch full news details
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"{apiUrl}Client/Reuters/ItemDescription?id={versionGuid}"
                    );
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    // Send request
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    if (response.StatusCode == HttpStatusCode.Forbidden ||
                             response.StatusCode == HttpStatusCode.Unauthorized ||
                             response.StatusCode == HttpStatusCode.NotFound)
                    {
                        thecalcify thecalcify = thecalcify.CurrentInstance;
                        thecalcify.DisconnectESCToolStripMenuItem_Click(null, null);
                        MessageBox.Show("Session expired. Please log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Parse JSON (double-encoded)
                    string jsonString = await response.Content.ReadAsStringAsync();
                    string innerJson = JsonConvert.DeserializeObject<string>(jsonString);

                    var root = JsonConvert.DeserializeObject<RootDto>(innerJson);
                    ItemDto item = root?.data?.item;

                    return item;
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[GetNewsByVersionGuidAsync] Error: {ex.Message}");
                return null;
            }
        }

        // Event handlers for pagination
        private async void BtnNextPage_Click(object sender, System.EventArgs e)
        {
            btnNextPage.Enabled = false;
            //_cts?.Cancel(); // stop previous periodic fetch

            try
            {
                currentPage++;


                // Fetch next set of news using PrevCursor
                await FetchNewsDataAndUpdateGrid(categoryCode, subcategoryCode, pageSize, cursor, $"{startdateRange}-{todateRange}");

                // Update page info label
                UpdatePageInfo(currentPage, pageSize);
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[btnNextPage_Click] Error: {ex.Message}");
            }
            btnNextPage.Enabled = true;

        }

        private async void BtnPrevPage_Click(object sender, System.EventArgs e)
        {
            try
            {
                btnNextPage.Enabled = true;

                currentPage = 1;
                PrevCursor = string.Empty;

                // Fetch first page of data again
                await FetchNewsDataAndUpdateGrid(categoryCode, subcategoryCode, pageSize, PrevCursor, $"{startdateRange}-{todateRange}");

                // Reset page info starting from 0
                UpdatePageInfo(currentPage, pageSize);
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[btnPrevPage_Click] Error: {ex.Message}");
            }
        }

        private void UpdatePageInfo(int pageNumber, int pageSize)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    int start = ((pageNumber - 1) * pageSize) + 1;
                    int end = start + pageSize - 1;

                    lblPageInfo.Text = $"Showing {start} - {end}";
                }));
            }
            else
            {
                int start = ((pageNumber - 1) * pageSize) + 1;
                int end = start + pageSize - 1;

                lblPageInfo.Text = $"Showing {start} - {end}";
            }
        }


        // Method to update page information
        //public void UpdatePageInfo(int currentPage, int totalPages)
        //{
        //    try
        //    {
        //        // Check if we need to invoke the update to the UI thread
        //        if (this.InvokeRequired)
        //        {
        //            this.Invoke(new Action(() =>
        //            {
        //                try
        //                {
        //                    if (!string.IsNullOrEmpty(_type))
        //                    {

        //                        lblPageInfo.Text = $"Records :- 30";
        //                    }
        //                    else
        //                    {
        //                        lblPageInfo.Text = $"Records :- 30";
        //                    }
        //                }
        //                catch (Exception innerEx)
        //                {
        //                    ApplicationLogger.Log($"[UpdatePageInfo] Error in Invoke: {innerEx.Message}");
        //                }
        //            }));
        //        }
        //        else
        //        {
        //            try
        //            {
        //                if (!string.IsNullOrEmpty(_type))
        //                {

        //                    lblPageInfo.Text = $"Records :- 30";
        //                }
        //                else
        //                {
        //                    lblPageInfo.Text = $"Records :- 30";
        //                }
        //            }
        //            catch (Exception innerEx)
        //            {
        //                ApplicationLogger.Log($"[UpdatePageInfo] Error: {innerEx.Message}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ApplicationLogger.Log($"[UpdatePageInfo] Error: {ex.Message}");
        //    }
        //}

        // Utility method to build Reuters API URL with query parameters
        public static string BuildReutersApiUrl(string baseUrl, int pageSize, string category = null, string subCategory = null, string cursorToken = null, string daterange = null)
        {
            try
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(category))
                    queryParams.Add($"category={Uri.EscapeDataString(category)}");

                if (!string.IsNullOrWhiteSpace(subCategory))
                    queryParams.Add($"subCategory={Uri.EscapeDataString(subCategory)}");

                queryParams.Add($"pageSize={(pageSize > 0 ? pageSize : 20)}");

                if (!string.IsNullOrWhiteSpace(cursorToken))
                    queryParams.Add($"cursorToken={Uri.EscapeDataString(cursorToken)}");

                if (!string.IsNullOrWhiteSpace(daterange))
                    queryParams.Add($"dateRange={Uri.EscapeDataString(NormalizeDateRange(daterange))}");

                return $"{baseUrl}?{string.Join("&", queryParams)}";
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[BuildReutersApiUrl] Error: {ex.Message}");
                return baseUrl;
            }
        }

        public void PeriodicDispose()
        {
            try
            {
                _cts?.Cancel();

                if (_fetchTask != null)
                {
                    _fetchTask.Wait(); // Wait for background task to stop
                }
            }
            catch (AggregateException ex)
            {
                // Optional: log or handle exceptions from the task
                foreach (var inner in ex.InnerExceptions)
                {
                    ApplicationLogger.LogException(inner); // Or log it
                }
            }
            finally
            {
                _fetchTask = null;
                _cts?.Dispose();
                _cts = null;
            }
        }

        public static string NormalizeDateRange(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            DateTime startDate, endDate;
            var parts = input.Trim().Split('-');

            if (parts.Length == 2 &&
                DateTime.TryParseExact(parts[0], "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out startDate) &&
                DateTime.TryParseExact(parts[1], "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out endDate))
            {
                if (startDate == endDate)
                {
                    endDate = endDate.AddDays(1);
                }

                return $"{startDate:yyyy.MM.dd}-{endDate:yyyy.MM.dd}";
            }

            // Invalid input format
            return string.Empty;
        }


        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            MessagePopup.ShowPopup("News Refreshed  "+ DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff"), true);
            _cts?.Cancel(); // stop previous periodic fetch
                            // Restart periodic fetch after reload
            _cts = new CancellationTokenSource();
            _fetchTask = Task.Run(() => PeriodicFetchAsync(_cts.Token));

            Common.ShowWindowsToast("News Refreshed", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff"));

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string filterText = newsSearch.Text.Trim();

            // Split filter by comma, trim each part, and remove empty strings
            var keywords = filterText.Split(',')
                                     .Select(k => k.Trim())
                                     .Where(k => !string.IsNullOrEmpty(k))
                                     .ToList();

            if (keywords.Count == 0)
            {
                // Reset all rows visible in defaultGrid
                if (dgvNews != null)
                {
                    foreach (DataGridViewRow row in dgvNews.Rows)
                    {
                        if (!row.IsNewRow)
                            row.Visible = true;
                    }
                }
            }
            else
            {
                // Filter rows in defaultGrid based on "Name" column
                if (dgvNews != null)
                {
                    foreach (DataGridViewRow row in dgvNews.Rows)
                    {
                        if (!row.IsNewRow && row.Cells["DGVTitle"].Value != null)
                        {
                            string name = row.Cells["DGVTitle"].Value.ToString();
                            bool match = keywords.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                            row.Visible = match;
                        }
                    }
                }
            }

        }

        private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Check if Ctrl + Backspace is pressed
            if (e.Control && e.KeyCode == Keys.Back)
            {
                newsSearch.Clear();  // Clear all text 
                e.SuppressKeyPress = true; // Prevent default backspace behavior 
            }
        }

        private void fromTextbox_Click(object sender, EventArgs e)
        {
            tocalender.Visible = false; // hide other calendar
            fromcalender.Visible = true;
            startDate = null;
            fromcalender.BringToFront();
            fromcalender.MaxDate = DateTime.Today;
            fromcalender.MinDate = DateTime.Today.AddMonths(-1); // optional, limit range
        }

        private void todateTextbox_Click(object sender, EventArgs e)
        {
            fromcalender.Visible = false; // hide other calendar
            tocalender.Visible = true;
            endDate = null;
            tocalender.BringToFront();
            tocalender.MaxDate = DateTime.Today;
            tocalender.MinDate = DateTime.Today.AddMonths(-1);
        }

        private void fromcalender_DateSelected(object sender, DateRangeEventArgs e)
        {
            startDate = e.Start.Date;
            fromTextbox.Text = startDate?.ToString("yyyy.MM.dd");
            fromcalender.Visible = false;

            // Auto adjust To Date if before From Date
            if (endDate.HasValue && endDate < startDate)
            {
                endDate = startDate;
                todateTextbox.Text = endDate?.ToString("yyyy.MM.dd");
            }

            UpdateRangeTextbox();
        }

        private void tocalender_DateSelected(object sender, DateRangeEventArgs e)
        {
            endDate = e.Start.Date;
            if (endDate > DateTime.Today)
                endDate = DateTime.Today; // never greater than today

            // Auto fix if To < From
            if (startDate.HasValue && endDate < startDate)
                endDate = startDate;

            todateTextbox.Text = endDate?.ToString("yyyy.MM.dd");
            tocalender.Visible = false;
            UpdateRangeTextbox();
        }

        private void fromTextbox_Leave(object sender, EventArgs e)
        {
            DateTime parsedDate;
            if (DateTime.TryParse(fromTextbox.Text, out parsedDate))
            {
                if (parsedDate > DateTime.Today)
                    parsedDate = DateTime.Today;

                startDate = parsedDate;
                fromTextbox.Text = startDate?.ToString("yyyy.MM.dd");
            }
            else
            {
                fromTextbox.Text = "yyyy.MM.dd";
                startDate = null;
            }
            UpdateRangeTextbox();
        }

        private void todateTextbox_Leave(object sender, EventArgs e)
        {
            DateTime parsedDate;
            if (DateTime.TryParse(todateTextbox.Text, out parsedDate))
            {
                if (parsedDate > DateTime.Today)
                    parsedDate = DateTime.Today;

                if (startDate.HasValue && parsedDate < startDate)
                    parsedDate = startDate.Value;

                endDate = parsedDate;
                todateTextbox.Text = endDate?.ToString("yyyy.MM.dd");
            }
            else
            {
                todateTextbox.Text = "yyyy.MM.dd";
                endDate = null;
            }
            UpdateRangeTextbox();
        }

        private void UpdateRangeTextbox()
        {
            string from = startDate?.ToString("yyyy.MM.dd") ?? "yyyy.MM.dd";
            string to = endDate?.ToString("yyyy.MM.dd") ?? "yyyy.MM.dd";

            if (from == "yyyy.MM.dd")
            {
                fromcalender.Visible = true;
            }
            else if (to == "yyyy.MM.dd")
            {
                tocalender.Visible = true;
            }
        }
    }
}