using ClosedXML.Excel;
using DocumentFormat.OpenXml.ExtendedProperties;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Font = System.Drawing.Font;
using PdfFont = iTextSharp.text.Font;


namespace PersonalExpensesManager
{
    public partial class FrmMain : Form
    {
        private readonly PEDBDataContext db;
        //Exell
        //تجهيز بيانات التقرير أولًا 
        private List<ReportTransactionRow> GetReportData()
        {
            DateTime fromDate = dtpReportFrom.Value.Date;
            DateTime toDateExclusive = dtpReportTo.Value.Date.AddDays(1);

            var data =
                (from transaction in db.Transactions
                 join category in db.Categories
                     on transaction.CategoryId equals category.CategoryId
                 where transaction.TransactionDate >= fromDate
                    && transaction.TransactionDate < toDateExclusive
                 orderby transaction.TransactionDate descending,
                          transaction.TransactionId descending
                 select new ReportTransactionRow
                 {
                     TransactionId = transaction.TransactionId,
                     TransactionDate = transaction.TransactionDate,
                     CategoryName = category.CategoryName,
                     CategoryType = category.CategoryType,
                     Amount = transaction.Amount,
                     Description = transaction.Description
                 })
                .ToList();

            return data;
        }
        //<<------------------------------------------------------>>

        // النوع الحالي للعملية: دخل أو مصروف
        private string selectedTransactionType = "مصروف";

        public FrmMain()
        {
            InitializeComponent();
            db = new PEDBDataContext();
        }
        //==================================================================================================

        // < start the tabDashBorad Classes >
        private string FormatMoney(decimal amount)//أضف دالة تنسيق المبالغ
        {
            return amount.ToString("N2") + " ريال";
        }
        private void LoadDashboardSummary()
        {
            try
            {
                // حساب إجمالي الدخل
                decimal totalIncome =
                    (from transaction in db.Transactions
                     join category in db.Categories
                         on transaction.CategoryId equals category.CategoryId
                     where category.CategoryType == "دخل"
                     select (decimal?)transaction.Amount)
                    .Sum() ?? 0m;
                //حساب إجمالي المصروفات 
                decimal totalExpenses =
                    (from transaction in db.Transactions
                     join category in db.Categories
                         on transaction.CategoryId equals category.CategoryId
                     where category.CategoryType == "مصروف"
                     select (decimal?)transaction.Amount)
                    .Sum() ?? 0m;
                // حساب الرصيد الحالي 
                decimal currentBalance = totalIncome - totalExpenses;

                lblTotalIncome.Text = FormatMoney(totalIncome);
                lblTotalExpenses.Text = FormatMoney(totalExpenses);
                lblCurrentBalance.Text = FormatMoney(currentBalance);

                lblLastUpdate.Text = "آخر تحديث: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                // تغيير لون الرصيد حسب قيمته
                if (currentBalance < 0)
                {
                    lblCurrentBalance.ForeColor = System.Drawing.Color.FromArgb(201, 42, 42);
                }
                else
                {
                    lblCurrentBalance.ForeColor = System.Drawing.Color.FromArgb(24, 100, 171);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل بيانات لوحة التحكم:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        //------------------
        private void LoadMonthlyExpensesChart()//مخطط المصروفات الشهرية
        {
            try
            {
                var monthlyExpenses =
                    (from transaction in db.Transactions
                     join category in db.Categories
                         on transaction.CategoryId equals category.CategoryId
                     where category.CategoryType == "مصروف"
                     group transaction by new
                     {
                         Year = transaction.TransactionDate.Year,
                         Month = transaction.TransactionDate.Month
                     }
                     into monthGroup
                     select new
                     {
                         Year = monthGroup.Key.Year,
                         Month = monthGroup.Key.Month,
                         TotalExpenses = monthGroup.Sum(x => x.Amount)
                     })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                chartMonthlyExpenses.Series.Clear();
                chartMonthlyExpenses.ChartAreas.Clear();
                chartMonthlyExpenses.Titles.Clear();
                chartMonthlyExpenses.Legends.Clear();

                chartMonthlyExpenses.BackColor = Color.White;

                ChartArea chartArea = new ChartArea("MonthlyExpensesArea");
                chartArea.BackColor = Color.White;
                chartArea.AxisX.MajorGrid.Enabled = false;
                chartArea.AxisY.MajorGrid.LineColor =
                    Color.FromArgb(237, 242, 247);
                chartArea.AxisX.LabelStyle.Font =
                    new Font("Segoe UI", 9);
                chartArea.AxisY.LabelStyle.Font =
                    new Font("Segoe UI", 9);
                chartArea.AxisY.LabelStyle.Format = "N0";
                chartArea.AxisY.Title = "المبلغ";
                chartArea.AxisX.Title = "الشهر";

                chartMonthlyExpenses.ChartAreas.Add(chartArea);

                Title title = chartMonthlyExpenses.Titles.Add(
                    "المصروفات الشهرية");
                title.Font = new Font(
                    "Segoe UI", 14, FontStyle.Bold);
                title.ForeColor = Color.FromArgb(13, 43, 69);

                Series expenseSeries = new Series("المصروفات");
                expenseSeries.ChartType = SeriesChartType.Column;
                expenseSeries.Color = Color.FromArgb(19, 168, 158);
                expenseSeries.BorderColor = Color.FromArgb(15, 143, 135);
                expenseSeries.BorderWidth = 1;
                expenseSeries.IsValueShownAsLabel = true;
                expenseSeries.LabelFormat = "N0";
                expenseSeries.Font = new Font(
                    "Segoe UI", 8, FontStyle.Bold);

                foreach (var item in monthlyExpenses)
                {
                    string monthName = GetArabicMonthName(item.Month);

                    DataPoint point = new DataPoint();
                    point.SetValueY(item.TotalExpenses);
                    point.AxisLabel = monthName;
                    point.ToolTip = monthName + ": " +
                                    item.TotalExpenses.ToString("N2") +
                                    " ر.س";

                    expenseSeries.Points.Add(point);
                }

                chartMonthlyExpenses.Series.Add(expenseSeries);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل مخطط المصروفات:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private string GetArabicMonthName(int month)//دالة أسماء الأشهر
        {
            string[] months =
            {
                "يناير", "فبراير", "مارس", "أبريل",
                "مايو", "يونيو", "يوليو", "أغسطس",
                "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
            };

            if (month < 1 || month > 12)
            {
                return month.ToString();
            }

            return months[month - 1];
        }
        private void LoadRecentTransactions()// عرض آخر العمليات
        {
            try
            {
                var recentTransactions =
                    (from transaction in db.Transactions
                     join category in db.Categories
                         on transaction.CategoryId equals category.CategoryId
                     orderby transaction.TransactionDate descending,
                              transaction.TransactionId descending
                     select new
                     {
                         رقم = transaction.TransactionId,
                         التاريخ = transaction.TransactionDate,
                         التصنيف = category.CategoryName,
                         النوع = category.CategoryType,
                         المبلغ = transaction.Amount,
                         الوصف = transaction.Description
                     })
                    .Take(5)
                    .ToList();

                dgvRecentTransactions.DataSource = recentTransactions;

                ConfigureRecentTransactionsGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل آخر العمليات:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void ConfigureRecentTransactionsGrid()//تنسيق جدول آخر العمليات
        {
            dgvRecentTransactions.AutoGenerateColumns = true;
            dgvRecentTransactions.ReadOnly = true;
            dgvRecentTransactions.AllowUserToAddRows = false;
            dgvRecentTransactions.AllowUserToDeleteRows = false;
            dgvRecentTransactions.AllowUserToResizeRows = false;
            dgvRecentTransactions.RowHeadersVisible = false;
            dgvRecentTransactions.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvRecentTransactions.MultiSelect = false;
            dgvRecentTransactions.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvRecentTransactions.BackgroundColor = Color.White;
            dgvRecentTransactions.BorderStyle = BorderStyle.None;
            dgvRecentTransactions.RightToLeft = RightToLeft.Yes;

            dgvRecentTransactions.ColumnHeadersHeight = 40;
            dgvRecentTransactions.RowTemplate.Height = 38;

            dgvRecentTransactions.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(234, 242, 247);
            dgvRecentTransactions.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(13, 43, 69);
            dgvRecentTransactions.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);
            dgvRecentTransactions.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dgvRecentTransactions.DefaultCellStyle.Font =
                new Font("Segoe UI", 9);
            dgvRecentTransactions.DefaultCellStyle.ForeColor =
                Color.FromArgb(52, 64, 84);
            dgvRecentTransactions.DefaultCellStyle.BackColor = Color.White;
            dgvRecentTransactions.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(223, 245, 241);
            dgvRecentTransactions.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(13, 43, 69);
            dgvRecentTransactions.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dgvRecentTransactions.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252);

            if (dgvRecentTransactions.Columns["رقم"] != null)
            {
                dgvRecentTransactions.Columns["رقم"].HeaderText = "رقم";
                dgvRecentTransactions.Columns["رقم"].Width = 55;
            }

            if (dgvRecentTransactions.Columns["التاريخ"] != null)
            {
                dgvRecentTransactions.Columns["التاريخ"].HeaderText = "التاريخ";
                dgvRecentTransactions.Columns["التاريخ"]
                    .DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            if (dgvRecentTransactions.Columns["التصنيف"] != null)
            {
                dgvRecentTransactions.Columns["التصنيف"].HeaderText = "التصنيف";
            }

            if (dgvRecentTransactions.Columns["النوع"] != null)
            {
                dgvRecentTransactions.Columns["النوع"].HeaderText = "النوع";
            }

            if (dgvRecentTransactions.Columns["المبلغ"] != null)
            {
                dgvRecentTransactions.Columns["المبلغ"].HeaderText = "المبلغ";
                dgvRecentTransactions.Columns["المبلغ"]
                    .DefaultCellStyle.Format = "N2";
            }

            if (dgvRecentTransactions.Columns["الوصف"] != null)
            {
                dgvRecentTransactions.Columns["الوصف"].HeaderText = "الوصف";
            }
        }
        private void LoadDashboardCharts()// استدعاء المخطط والجدول عند فتح لوحة التحكم
        {
            LoadMonthlyExpensesChart();
            LoadRecentTransactions();
        }
        private void RefreshDashboardAfterSave()
        {
            LoadDashboardSummary();
            LoadDashboardCharts();
        }
        // < End the tabDashBorad Classes >

        //==================================================================================================

        // < start the tabTransactions Classes >

        /*private void LoadTransactionTypes()     //كود تحميل العمليات
        {
            //cmbTransactionType.Items.Clear();

            ////cmbTransactionType.Items.Add("كل العمليات");
            ////cmbTransactionType.Items.Add("دخل");
            ////cmbTransactionType.Items.Add("مصروف");

            //cmbTransactionType.SelectedIndex = 0;
        }*/
        private void LoadTransactions()
        {
            try
            {
                string searchText = txtSearch.Text.Trim(); 
                //Trim() Removes all leading and trailing white-space characters

                DateTime fromDate = dtpFromDate.Value.Date;
                DateTime toDate = dtpToDate.Value.Date;

                string selectedType = null;

                if (cmbTransactionType.SelectedIndex > 0)
                {
                    selectedType = cmbTransactionType.SelectedItem.ToString();
                }

                var query =
                        from transaction in db.Transactions
                        join category in db.Categories
                            on transaction.CategoryId equals category.CategoryId
                        where transaction.TransactionDate >= fromDate
                           && transaction.TransactionDate <= toDate
                           && (
                                searchText == ""
                                || category.CategoryName.Contains(searchText)
                                || transaction.Description.Contains(searchText)
                              )
                           && (
                                selectedType == null
                                || category.CategoryType == selectedType
                              )
                        orderby transaction.TransactionDate descending,
                                transaction.TransactionId descending
                        select new
                        {
                            TransactionId = transaction.TransactionId,
                            TransactionDate = transaction.TransactionDate,
                            CategoryName = category.CategoryName,
                            CategoryType = category.CategoryType,
                            Amount = transaction.Amount,
                            Description = transaction.Description
                        };

                dgvTransactions.DataSource = query.ToList();

                ConfigureTransactionsGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل العمليات:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        private void ConfigureTransactionsGrid()        // تنسيق أعمدة dgvTransactions
        {
            dgvTransactions.AutoGenerateColumns = true;
            dgvTransactions.ReadOnly = true;
            dgvTransactions.AllowUserToAddRows = false;
            dgvTransactions.AllowUserToDeleteRows = false;
            dgvTransactions.AllowUserToResizeRows = false;
            dgvTransactions.RowHeadersVisible = false;
            dgvTransactions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTransactions.MultiSelect = false;
            dgvTransactions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTransactions.BackgroundColor = Color.White;
            dgvTransactions.BorderStyle = BorderStyle.None;
            dgvTransactions.RightToLeft = RightToLeft.Yes;

            dgvTransactions.ColumnHeadersHeight = 45;
            dgvTransactions.RowTemplate.Height = 42;

            dgvTransactions.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(234, 242, 247);
            dgvTransactions.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(13, 43, 69);
            dgvTransactions.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);
            dgvTransactions.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dgvTransactions.DefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Regular);
            dgvTransactions.DefaultCellStyle.ForeColor =
                Color.FromArgb(52, 64, 84);
            dgvTransactions.DefaultCellStyle.BackColor = Color.White;
            dgvTransactions.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(223, 245, 241);
            dgvTransactions.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(13, 43, 69);
            dgvTransactions.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dgvTransactions.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252);

            SetTransactionColumnNames();
        }
        private void SetTransactionColumnNames()        // تغيير أسماء رؤوس الأعمدة
        {
            if (dgvTransactions.Columns["TransactionId"] != null)
            {
                dgvTransactions.Columns["TransactionId"].HeaderText = "رقم";
                dgvTransactions.Columns["TransactionId"].Width = 60;
            }

            if (dgvTransactions.Columns["TransactionDate"] != null)
            {
                dgvTransactions.Columns["TransactionDate"].HeaderText = "التاريخ";
                dgvTransactions.Columns["TransactionDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            if (dgvTransactions.Columns["CategoryName"] != null)
            {
                dgvTransactions.Columns["CategoryName"].HeaderText = "التصنيف";
            }

            if (dgvTransactions.Columns["CategoryType"] != null)
            {
                dgvTransactions.Columns["CategoryType"].HeaderText = "النوع";
            }

            if (dgvTransactions.Columns["Amount"] != null)
            {
                dgvTransactions.Columns["Amount"].HeaderText = "المبلغ";
                dgvTransactions.Columns["Amount"].DefaultCellStyle.Format = "N2";
            }

            if (dgvTransactions.Columns["Description"] != null)
            {
                dgvTransactions.Columns["Description"].HeaderText = "الوصف";
            }
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            //  تشغيل البحث
            if (dtpFromDate.Value.Date > dtpToDate.Value.Date)
            {
                MessageBox.Show(
                    "تاريخ البداية يجب أن يكون قبل تاريخ النهاية.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            LoadTransactions();
        }

        //  تنسيق لون الدخل والمصروف      
        //   يظهر الدخل باللون الأخضر والمصروف باللون الأحمر
        private void DgvTransactions_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dgvTransactions.Columns[e.ColumnIndex].Name == "CategoryType")
            {
                if (e.Value != null && e.Value.ToString() == "دخل")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(8, 127, 91);
                    e.CellStyle.BackColor = Color.FromArgb(232, 247, 241);
                    e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                else if (e.Value != null && e.Value.ToString() == "مصروف")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(201, 42, 42);
                    e.CellStyle.BackColor = Color.FromArgb(253, 236, 236);
                    e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            }

            if (dgvTransactions.Columns[e.ColumnIndex].Name == "Amount")
            {
                DataGridViewRow row = dgvTransactions.Rows[e.RowIndex];
                string type = row.Cells["CategoryType"].Value?.ToString();

                if (type == "دخل")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(8, 127, 91);
                }
                else if (type == "مصروف")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(201, 42, 42);
                }

                e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }

        }

        // < End the tabTransactions Classes >

        //==================================================================================================

        // < start the tabTransactions Classes >

        private void LoadCategories() //     تحميل التصنيفات داخل cmbCategory
        {
            //   {selectedTransactionType = "مصروف"}  is تعرض تصنيفات المصروف فقط لأن القيمة الافتراضية 
            try
            {
                var categories =
                    (from category in db.Categories
                     where category.IsActive == true
                        && category.CategoryType == selectedTransactionType
                     orderby category.CategoryName
                     select category)
                    .ToList();

                cmbCategory.DataSource = null;
                cmbCategory.DataSource = categories;
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryId";
                cmbCategory.SelectedIndex = -1;
                cmbCategory.Text = "اختر التصنيف";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل التصنيفات:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void btnExpense_Click(object sender, EventArgs e) // زر المصروف 
        {
            //عند الضغط على زر المصروف، نغير النوع ثم نعيد تحميل التصنيفات الخاصة بالمصروف. 
            selectedTransactionType = "مصروف";

            LoadCategories();
            SetExpenseButtonStyle();
        }
        private void btnIncome_Click(object sender, EventArgs e) //     زر الدخل     
        {
            selectedTransactionType = "دخل";

            LoadCategories();
            SetIncomeButtonStyle();
        }
        private void SetIncomeButtonStyle() //       تنسيق ازرار الدخل والمصروف
        {
            // زر الدخل محدد
            btnIncome.FillColor = Color.FromArgb(19, 168, 158);
            btnIncome.ForeColor = Color.White;
            btnIncome.BorderColor = Color.FromArgb(19, 168, 158);

            // زر المصروف غير محدد
            btnExpense.FillColor = Color.FromArgb(253, 236, 236);
            btnExpense.ForeColor = Color.FromArgb(201, 42, 42);
            btnExpense.BorderColor = Color.FromArgb(224, 49, 49);
        }
        private void SetExpenseButtonStyle()//      حالة المصروف المحددة
        {
            // زر المصروف محدد
            btnExpense.FillColor = Color.FromArgb(224, 49, 49);
            btnExpense.ForeColor = Color.White;
            btnExpense.BorderColor = Color.FromArgb(224, 49, 49);

            // زر الدخل غير محدد
            btnIncome.FillColor = Color.FromArgb(232, 247, 241);
            btnIncome.ForeColor = Color.FromArgb(8, 127, 91);
            btnIncome.BorderColor = Color.FromArgb(19, 168, 158);
        }
        private bool ValidateTransactionData(out decimal amount)  //    التحقق من بيانات العملية
        {
            amount = 0m;

            if (cmbCategory.SelectedIndex == -1 || cmbCategory.SelectedValue == null)
            {
                MessageBox.Show(
                    "يرجى اختيار تصنيف العملية!",
                    "تنبية",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                cmbCategory.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAmount.Text))
            {
                MessageBox.Show(
                    "يرجى إدخال مبلغ العملية.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtAmount.Focus();
                return false;
            }

            bool isValidAmount = decimal.TryParse(
                txtAmount.Text.Trim(),
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out amount);

            if (!isValidAmount || amount <= 0)
            {
                MessageBox.Show(
                    "يرجى إدخال مبلغ صحيح أكبر من صفر.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtAmount.SelectAll();
                txtAmount.Focus();
                return false;
            }

            return true;
        }
        private void ClearTransactionForm() //  تفريغ الحقول بعد الحفظ
        {
            cmbCategory.SelectedIndex = -1;
            cmbCategory.Text = "اختر التصنيف";

            txtAmount.Clear();
            txtDescription.Clear();

            dtpTransactionDate.Value = DateTime.Today;

            selectedTransactionType = "مصروف";
            LoadCategories();
            SetExpenseButtonStyle();

            txtAmount.Focus();
        }
        private void GoToTransactionsAfterSave() // تحديث شاشة العمليات بعد الحفظ
        {
            //يمكنك الانتقال إلى شاشة العمليات بعد الحفظ
            //بدل تفريغ النموذج
            tabControlMain.SelectedTab = tabTransactions;
            txtAmount.Clear();
            txtDescription.Clear();
            LoadTransactions();
        }
        //------------------
        private void btnSave_Click(object sender, EventArgs e)//    حفظ العملية
        {
            try
            {
                decimal amount;

                if (!ValidateTransactionData(out amount))
                {
                    return;
                }

                int categoryId = Convert.ToInt32(cmbCategory.SelectedValue);

                // التأكد من أن التصنيف ينتمي فعلًا إلى نوع العملية المحدد
                var selectedCategory =
                    (from category in db.Categories
                     where category.CategoryId == categoryId
                        && category.CategoryType == selectedTransactionType
                        && category.IsActive == true
                     select category)
                    .FirstOrDefault();

                if (selectedCategory != null)
                {
                    // إنشاء كائن جديد من كلاس Transaction الذي أنشأه dbml
                    Transaction newTransaction = new Transaction()
                    {
                        CategoryId = selectedCategory.CategoryId,
                        Amount = amount,
                        TransactionDate = dtpTransactionDate.Value.Date,
                        Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                        CreatedAt = DateTime.Now
                    };

                    db.Transactions.InsertOnSubmit(newTransaction);
                    db.SubmitChanges();
                    RefreshDashboardAfterSave();
                    MessageBox.Show("تم حفظ العملية بنجاح.","تم الحفظ",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    // الانتقال إلى شاشة العمليات بعد الحفظ
                    //بدل تفريغ النموذج
                    ClearTransactionForm();
                    GoToTransactionsAfterSave();
                }
                else
                {
                    MessageBox.Show(
                        "التصنيف المحدد غير صحيح.",
                        "تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء حفظ العملية:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)//    ألغاء العملية
        {
            ClearTransactionForm();
        }
        // < End the tabTransactions Classes >
        //==================================================================================================
        // < start the tabReports Classes >
        
        private void SetReportPeriod()
        {
            void Periodview()
            {
                lebPeriod.Text = cmbPeriod.Text;
                lebPeriod0.Text = cmbPeriod.Text;
                lebPeriod1.Text = cmbPeriod.Text;
            }

            //تحديد تاريخ التقرير
            // تتغير التواريخ حسب الفترة المختارة
            DateTime today = DateTime.Today;
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            if (cmbPeriod.SelectedIndex == 0)
            {
                // هذا الشهر
                dtpReportFrom.Value = firstDayOfMonth;
                dtpReportTo.Value = today;
                Periodview();
            }
            else if (cmbPeriod.SelectedIndex == 1)
            {
                // الشهر السابق
                DateTime previousMonth = firstDayOfMonth.AddMonths(-1);
                dtpReportFrom.Value = previousMonth;
                dtpReportTo.Value = firstDayOfMonth.AddDays(-1);
                Periodview();
            }
            else if (cmbPeriod.SelectedIndex == 2)
            {
                // هذه السنة
                dtpReportFrom.Value = new DateTime(today.Year, 1, 1);
                dtpReportTo.Value = today;
                Periodview();

            }
            else if (cmbPeriod.SelectedIndex == 3)
            {
                Periodview();
            }

            // عند اختيار تحديد فترة، لا نغير القيم يدويًا
        }

        // تحميل تقرير الملخص
        // هذه الدالة لحساب الدخل والمصروفات والرصيد في الفترة المحددة
        private void LoadReportSummary(DateTime fromDate, DateTime toDate)
        {
            DateTime exclusiveToDate = toDate.Date.AddDays(1);

            decimal totalIncome =
                (   from transaction in db.Transactions
                     join category in db.Categories
                         on transaction.CategoryId equals category.CategoryId
                     where category.CategoryType == "دخل"
                        && transaction.TransactionDate >= fromDate.Date
                        && transaction.TransactionDate < exclusiveToDate
                     select (decimal?)transaction.Amount)
                .Sum() ?? 0m;

            decimal totalExpenses =
                 (  from transaction in db.Transactions
                      join category in db.Categories
                         on transaction.CategoryId equals category.CategoryId
                     where category.CategoryType == "مصروف"
                        && transaction.TransactionDate >= fromDate.Date
                        && transaction.TransactionDate < exclusiveToDate
                     select (decimal?)transaction.Amount)
                .Sum() ?? 0m;

            decimal balance = totalIncome - totalExpenses;

            lblReportIncome.Text = FormatMoney(totalIncome);
            lblReportExpenses.Text = FormatMoney(totalExpenses);
            lblReportBalance.Text = FormatMoney(balance);

            if (balance < 0)
            {
                lblReportBalance.ForeColor = Color.FromArgb(201, 42, 42);
            }
            else
            {
                lblReportBalance.ForeColor = Color.FromArgb(24, 100, 171);
            }
        }

        //----------------------------------------
        //6. رسم المصروفات حسب التصنيف
        private void LoadCategoryChart(DateTime fromDate, DateTime toDate)
        {
            DateTime exclusiveToDate = toDate.Date.AddDays(1);

            var categoryExpenses =
                (from transaction in db.Transactions
                 join category in db.Categories
                     on transaction.CategoryId equals category.CategoryId
                 where category.CategoryType == "مصروف"
                    && transaction.TransactionDate >= fromDate.Date
                    && transaction.TransactionDate < exclusiveToDate
                 group transaction by category.CategoryName into categoryGroup
                 select new
                 {
                     CategoryName = categoryGroup.Key,
                     TotalAmount = categoryGroup.Sum(x => x.Amount)
                 })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            chartCategories.Series.Clear();
            chartCategories.Titles.Clear();
            chartCategories.Legends.Clear();

            chartCategories.BackColor = Color.White;

            Title title = chartCategories.Titles.Add("المصروفات حسب التصنيف");
            title.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(13, 43, 69);

            Legend legend = new Legend("CategoryLegend");
            legend.Docking = Docking.Bottom;
            legend.Font = new Font("Segoe UI", 9);
            chartCategories.Legends.Add(legend);

            Series series = new Series("المصروفات");
            series.ChartType = SeriesChartType.Doughnut;
            series.Legend = "CategoryLegend";
            series.IsValueShownAsLabel = true;
            series.Label = "#PERCENT{P0}";
            series.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            foreach (var item in categoryExpenses)
            {
                DataPoint point = new DataPoint();
                point.SetValueY(item.TotalAmount);
                point.LegendText = item.CategoryName;
                point.Label = "#PERCENT{P0}";
                point.ToolTip = item.CategoryName + ": " +
                                item.TotalAmount.ToString("N2") + " ر.س";

                point.Color = GetCategoryColor(item.CategoryName);
                series.Points.Add(point);
            }

            chartCategories.Series.Add(series);
            chartCategories.ChartAreas.Clear();

            ChartArea area = new ChartArea("CategoryArea");
            area.BackColor = Color.White;
            chartCategories.ChartAreas.Add(area);
        }
        // دالة ألوان التصنيفات
        private Color GetCategoryColor(string categoryName)
        {
            switch (categoryName)
            {
                case "طعام":
                    return Color.FromArgb(19, 168, 158);
                case "مواصلات":
                    return Color.FromArgb(230, 73, 128);
                case "فواتير":
                    return Color.FromArgb(245, 159, 0);
                case "ترفيه":
                    return Color.FromArgb(132, 94, 247);
                case "صحة":
                    return Color.FromArgb(49, 130, 206);
                case "تسوق":
                    return Color.FromArgb(32, 201, 151);
                case "إيجار":
                    return Color.FromArgb(224, 49, 49);
                default:
                    return Color.FromArgb(148, 163, 184);
            }
        }
        //رسم الدخل والمصروفات حسب الشهر
        private void LoadMonthlyChart(DateTime fromDate, DateTime toDate)
        {
            DateTime exclusiveToDate = toDate.Date.AddDays(1);

            var monthlyData =
                (from transaction in db.Transactions
                 join category in db.Categories
                     on transaction.CategoryId equals category.CategoryId
                 where transaction.TransactionDate >= fromDate.Date
                    && transaction.TransactionDate < exclusiveToDate
                 group new { transaction, category }
                 by new
                 {
                     Year = transaction.TransactionDate.Year,
                     Month = transaction.TransactionDate.Month
                 }
                 into monthGroup
                 select new
                 {
                     Year = monthGroup.Key.Year,
                     Month = monthGroup.Key.Month,
                     TotalIncome = monthGroup
                         .Where(x => x.category.CategoryType == "دخل")
                         .Sum(x => (decimal?)x.transaction.Amount) ?? 0m,
                     TotalExpenses = monthGroup
                         .Where(x => x.category.CategoryType == "مصروف")
                         .Sum(x => (decimal?)x.transaction.Amount) ?? 0m
                 })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            chartMonthly.Series.Clear();
            chartMonthly.Titles.Clear();
            chartMonthly.Legends.Clear();
            chartMonthly.ChartAreas.Clear();

            chartMonthly.BackColor = Color.White;

            Title title = chartMonthly.Titles.Add("الدخل والمصروفات الشهرية");
            title.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(13, 43, 69);

            ChartArea area = new ChartArea("MonthlyArea");
            area.BackColor = Color.White;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(237, 242, 247);
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 9);
            area.AxisY.LabelStyle.Format = "N0";
            chartMonthly.ChartAreas.Add(area);

            Legend legend = new Legend("MonthlyLegend");
            legend.Docking = Docking.Bottom;
            legend.Font = new Font("Segoe UI", 9);
            chartMonthly.Legends.Add(legend);

            Series incomeSeries = new Series("الدخل");
            incomeSeries.ChartType = SeriesChartType.Column;
            incomeSeries.Color = Color.FromArgb(19, 168, 158);
            incomeSeries.Legend = "MonthlyLegend";
            incomeSeries.IsValueShownAsLabel = false;

            Series expensesSeries = new Series("المصروفات");
            expensesSeries.ChartType = SeriesChartType.Column;
            expensesSeries.Color = Color.FromArgb(224, 49, 49);
            expensesSeries.Legend = "MonthlyLegend";
            expensesSeries.IsValueShownAsLabel = false;

            foreach (var item in monthlyData)
            {
                string monthName = GetArabicMonthName(item.Month);

                incomeSeries.Points.AddXY(monthName, item.TotalIncome);
                expensesSeries.Points.AddXY(monthName, item.TotalExpenses);
            }

            chartMonthly.Series.Add(incomeSeries);
            chartMonthly.Series.Add(expensesSeries);
        }
    
        //--------------------------------------------
        //تحميل التقرير بالكامل
        //هذه هي الدالة الرئيسية التي تستدعي جميع أجزاء التقرير
        private void LoadReports()
        {
            try
            {
                DateTime fromDate = dtpReportFrom.Value.Date;
                DateTime toDate = dtpReportTo.Value.Date;

                if (fromDate > toDate)
                {
                    MessageBox.Show(
                        "تاريخ البداية يجب أن يكون قبل تاريخ النهاية.",
                        "تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                LoadReportSummary(fromDate, toDate);
                LoadCategoryChart(fromDate, toDate);
                LoadMonthlyChart(fromDate, toDate);

                // إذا أضفت جدول الميزانية، فعّل السطر التالي
                //LoadBudgetReport(fromDate, toDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل التقرير:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        // < End the tabReports Classes >
        //=========================================================================

        private void FrmMain_Load(object sender, EventArgs e)
        {
            // تحميل لوحة التحكم
            LoadDashboardSummary();
            LoadDashboardCharts();
            //..........viewTransaction......
            LoadTransactions();
                        //  عرض الجدول العمليات من بداية السنة الحالية حتى تاريخ اليوم    
                        dtpFromDate.Value = new DateTime(DateTime.Today.Year, 1, 1);
                            dtpToDate.Value = DateTime.Today;
                //LoadTransactionTypes();

                ConfigureTransactionsGrid();
            //......................
            //  تحميل الشاشة الإضافة addTransaction عند فتح البرنامج
            selectedTransactionType = "مصروف";
            LoadCategories();
            SetExpenseButtonStyle();
            dtpTransactionDate.Value = DateTime.Today;

            //----tabReports---حميل التقرير عند فتح الفورم
            //جعل زر التصدير يظهر بعد عرض التقرير
            //تعطيل الأزرار في بداية التشغيل
            btnExportExcel.Enabled = false;
            btnExportPdf.Enabled = false;

            SetReportPeriod();
            LoadReports();
            //بعد نجاح تحميل البيانات، فعّل الأزرار:
            btnExportExcel.Enabled = true;
            btnExportPdf.Enabled = true;

        }
        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            //   التحديث عند تغيير التبويب مباشرة

            if (tabControlMain.SelectedTab == tabTransactions)
            {
                LoadTransactions();
            }
            if (tabControlMain.SelectedTab == tabDashboard)
            {
                LoadDashboardSummary();
                LoadDashboardCharts();

            }
            if(tabControlMain.SelectedTab == tabAddTransaction)
            {
                LoadCategories();
                txtAmount.Focus();
            }if(tabControlMain.SelectedTab == tabReports) 
            {
                LoadReports();
            }
        }

        private void cmbPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPeriod.SelectedIndex < 4)
            {
                SetReportPeriod();
            }
        }

        private void btnShowReport_Click(object sender, EventArgs e)
        {
            LoadReports();
        }



        //<<-------------------تصدير التقرير إلى Excel----------------------------------->>
        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            // تصدير التقرير إلى Excel   ثانيًا
            try
            {
                List<ReportTransactionRow> reportData = GetReportData();

                if (reportData.Count == 0)
                {
                    MessageBox.Show(
                        "لا توجد بيانات لتصديرها في الفترة المحددة.",
                        "تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Files|*.xlsx";
                    saveDialog.Title = "حفظ تقرير Excel";
                    saveDialog.FileName =
                        "تقرير_المصاريف_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";

                    if (saveDialog.ShowDialog() != DialogResult.OK)
                        return;

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        IXLWorksheet sheet = workbook.Worksheets.Add("التقرير");

                        // عنوان التقرير
                        sheet.Cell(1, 1).Value = "تقرير العمليات المالية";
                        sheet.Range(1, 1, 1, 6).Merge();
                        sheet.Cell(1, 1).Style.Font.Bold = true;
                        sheet.Cell(1, 1).Style.Font.FontSize = 16;
                        sheet.Cell(1, 1).Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Center;

                        // الفترة
                        sheet.Cell(2, 1).Value = "من تاريخ";
                        sheet.Cell(2, 2).Value = dtpReportFrom.Value.ToString("dd/MM/yyyy");
                        sheet.Cell(2, 3).Value = "إلى تاريخ";
                        sheet.Cell(2, 4).Value = dtpReportTo.Value.ToString("dd/MM/yyyy");

                        // رؤوس الأعمدة
                        int headerRow = 4;

                        string[] headers =
                        {
                            "رقم العملية",
                            "التاريخ",
                            "التصنيف",
                            "النوع",
                            "المبلغ",
                            "الوصف"
                        };

                        for (int i = 0; i < headers.Length; i++)
                        {
                            IXLCell cell = sheet.Cell(headerRow, i + 1);
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.FontColor = XLColor.White;
                            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0D2B45");
                            cell.Style.Alignment.Horizontal =
                                XLAlignmentHorizontalValues.Center;
                        }

                        // البيانات
                        int row = headerRow + 1;

                        foreach (ReportTransactionRow item in reportData)
                        {
                            sheet.Cell(row, 1).Value = item.TransactionId;
                            sheet.Cell(row, 2).Value = item.TransactionDate;
                            sheet.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy";
                            sheet.Cell(row, 3).Value = item.CategoryName;
                            sheet.Cell(row, 4).Value = item.CategoryType;
                            sheet.Cell(row, 5).Value = item.Amount;
                            sheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                            sheet.Cell(row, 6).Value = item.Description ?? "";

                            if (item.CategoryType == "دخل")
                            {
                                sheet.Cell(row, 4).Style.Font.FontColor =
                                    XLColor.FromHtml("#087F5B");
                                sheet.Cell(row, 5).Style.Font.FontColor =
                                    XLColor.FromHtml("#087F5B");
                            }
                            else
                            {
                                sheet.Cell(row, 4).Style.Font.FontColor =
                                    XLColor.FromHtml("#C92A2A");
                                sheet.Cell(row, 5).Style.Font.FontColor =
                                    XLColor.FromHtml("#C92A2A");
                            }

                            row++;
                        }

                        // ملخص التقرير
                        int summaryRow = row + 2;

                        decimal totalIncome = reportData
                            .Where(x => x.CategoryType == "دخل")
                            .Sum(x => x.Amount);

                        decimal totalExpenses = reportData
                            .Where(x => x.CategoryType == "مصروف")
                            .Sum(x => x.Amount);

                        decimal balance = totalIncome - totalExpenses;

                        sheet.Cell(summaryRow, 1).Value = "إجمالي الدخل";
                        sheet.Cell(summaryRow, 2).Value = totalIncome;
                        sheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "#,##0.00";

                        sheet.Cell(summaryRow + 1, 1).Value = "إجمالي المصروفات";
                        sheet.Cell(summaryRow + 1, 2).Value = totalExpenses;
                        sheet.Cell(summaryRow + 1, 2).Style.NumberFormat.Format = "#,##0.00";

                        sheet.Cell(summaryRow + 2, 1).Value = "صافي الرصيد";
                        sheet.Cell(summaryRow + 2, 2).Value = balance;
                        sheet.Cell(summaryRow + 2, 2).Style.NumberFormat.Format = "#,##0.00";

                        sheet.Columns().AdjustToContents();
                        sheet.RightToLeft = true;
                        sheet.SheetView.FreezeRows(4);

                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show(
                        "تم تصدير التقرير إلى Excel بنجاح.",
                        "تم التصدير",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تصدير ملف Excel:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        //<<-------------------------تصدير التقرير إلى PDF----------------------------->>
        // دالة إنشاء خلية عربية في PDF
        private PdfPCell CreatePdfCell(string text,PdfFont font,BaseColor backgroundColor = null)
        {
            PdfPCell cell = new PdfPCell(
                new Phrase(text ?? "", font));

            cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 6;
            cell.BorderColor = new BaseColor(220, 226, 232);

            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }

            return cell;
        }
        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime fromDate = dtpReportFrom.Value.Date;
                DateTime toDate = dtpReportTo.Value.Date;

                if (fromDate > toDate)
                {
                    MessageBox.Show(
                        "تاريخ البداية يجب أن يكون قبل تاريخ النهاية.",
                        "تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                List<ReportTransactionRow> reportData = GetReportData();

                if (reportData == null || reportData.Count == 0)
                {
                    MessageBox.Show(
                        "لا توجد عمليات في الفترة المحددة:\n" +
                        fromDate.ToString("dd/MM/yyyy") + " إلى " +
                        toDate.ToString("dd/MM/yyyy"),
                        "لا توجد بيانات",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF Files|*.pdf";
                    saveDialog.Title = "حفظ تقرير PDF";
                    saveDialog.FileName =
                        "تقرير_المصاريف_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmm") +
                        ".pdf";

                    if (saveDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    string fontPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                        "Arial.ttf");

                    if (!File.Exists(fontPath))
                    {
                        MessageBox.Show(
                            "لم يتم العثور على Arial.ttf لإنشاء ملف PDF باللغة العربية.",
                            "خطأ في الخط",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    BaseFont baseFont = BaseFont.CreateFont(
                        fontPath,
                        BaseFont.IDENTITY_H,
                        BaseFont.EMBEDDED);

                    PdfFont normalFont = new PdfFont(baseFont, 9);
                    PdfFont headerFont = new PdfFont(baseFont, 9, PdfFont.BOLD);
                    PdfFont titleFont = new PdfFont(baseFont, 16, PdfFont.BOLD);

                    using (FileStream stream = new FileStream(
                        saveDialog.FileName,
                        FileMode.Create))
                    {
                        Document document = new Document(
                            PageSize.A4.Rotate(),
                            25,
                            25,
                            30,
                            30);

                        PdfWriter.GetInstance(document, stream);
                        document.Open();

                        // =============================================
                        // 1. عنوان التقرير
                        // =============================================

                        PdfPTable titleTable = new PdfPTable(1);
                        titleTable.WidthPercentage = 100;
                        titleTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                        titleTable.AddCell(CreatePdfCell(
                            "تقرير العمليات المالية",
                            titleFont,
                            BaseColor.WHITE));

                        document.Add(titleTable);
                        document.Add(new Paragraph(" "));

                        // =============================================
                        // 2. فترة التقرير
                        // =============================================

                        PdfPTable periodTable = new PdfPTable(1);
                        periodTable.WidthPercentage = 100;
                        periodTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                        string periodText =
                            "الفترة: من " +
                            fromDate.ToString("dd/MM/yyyy") +
                            " إلى " +
                            toDate.ToString("dd/MM/yyyy");

                        periodTable.AddCell(CreatePdfCell(
                            periodText,
                            normalFont,
                            BaseColor.WHITE));

                        document.Add(periodTable);
                        document.Add(new Paragraph(" "));

                        // =============================================
                        // 3. حساب الملخص
                        // =============================================

                        decimal totalIncome = reportData
                            .Where(x => x.CategoryType == "دخل")
                            .Sum(x => x.Amount);

                        decimal totalExpenses = reportData
                            .Where(x => x.CategoryType == "مصروف")
                            .Sum(x => x.Amount);

                        decimal currentBalance = totalIncome - totalExpenses;

                        // =============================================
                        // 4. جدول العمليات
                        // =============================================

                        PdfPTable operationsTable = new PdfPTable(6);
                        operationsTable.WidthPercentage = 100;
                        operationsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                        operationsTable.SpacingBefore = 10;
                        operationsTable.SpacingAfter = 10;

                        operationsTable.SetWidths(new float[]
                        {
                            2.8f, // الوصف
                            1.8f, // المبلغ
                            1.5f, // النوع
                            1.8f, // التصنيف
                            1.8f, // التاريخ
                            1.0f  // الرقم
                        });

                        BaseColor headerColor = new BaseColor(13, 43, 69);

                        // بسبب اتجاه RTL، أضف الأعمدة من اليسار المنطقي إلى اليمين المنطقي
                        string[] headers =
                        {
                            "الوصف",
                            "المبلغ",
                            "النوع",
                            "التصنيف",
                            "التاريخ",
                            "الرقم"
                        };

                        foreach (string header in headers)
                        {
                            PdfPCell headerCell = CreatePdfCell(
                                header,
                                headerFont,
                                headerColor);

                            headerCell.Phrase.Font.Color = BaseColor.WHITE;
                            operationsTable.AddCell(headerCell);
                        }

                        foreach (ReportTransactionRow item in reportData)
                        {
                            BaseColor rowColor;

                            if (item.CategoryType == "دخل")
                            {
                                rowColor = new BaseColor(232, 247, 241);
                            }
                            else
                            {
                                rowColor = new BaseColor(253, 236, 236);
                            }

                            operationsTable.AddCell(CreatePdfCell(
                                item.Description ?? "",
                                normalFont,
                                rowColor));

                            operationsTable.AddCell(CreatePdfCell(
                                item.Amount.ToString("N2") + " ريال",
                                normalFont,
                                rowColor));

                            operationsTable.AddCell(CreatePdfCell(
                                item.CategoryType,
                                normalFont,
                                rowColor));

                            operationsTable.AddCell(CreatePdfCell(
                                item.CategoryName,
                                normalFont,
                                rowColor));

                            operationsTable.AddCell(CreatePdfCell(
                                item.TransactionDate.ToString("dd/MM/yyyy"),
                                normalFont,
                                rowColor));

                            operationsTable.AddCell(CreatePdfCell(
                                item.TransactionId.ToString(),
                                normalFont,
                                rowColor));
                        }

                        // هذا السطر مهم جدًا؛ بدونه لن يظهر جدول العمليات في PDF
                        document.Add(operationsTable);

                        // =============================================
                        // 5. ملخص التقرير
                        // =============================================

                        document.Add(new Paragraph(" "));

                        PdfPTable summaryTable = new PdfPTable(2);
                        summaryTable.WidthPercentage = 45;
                        summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                        summaryTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                        summaryTable.SpacingBefore = 10;

                        summaryTable.AddCell(CreatePdfCell(
                            "إجمالي الدخل",
                            headerFont,
                            new BaseColor(13, 43, 69)));

                        summaryTable.AddCell(CreatePdfCell(
                            totalIncome.ToString("N2") + " ريال",
                            normalFont,
                            new BaseColor(253, 236, 236)));

                        summaryTable.AddCell(CreatePdfCell(
                            "إجمالي المصروفات",
                            headerFont,
                            new BaseColor(13, 43, 69)));

                        summaryTable.AddCell(CreatePdfCell(
                            totalExpenses.ToString("N2") + " ريال",
                            normalFont,
                            new BaseColor(253, 236, 236)));

                        summaryTable.AddCell(CreatePdfCell(
                            "صافي الرصيد",
                            headerFont,
                            new BaseColor(13, 43, 69)));

                        summaryTable.AddCell(CreatePdfCell(
                            currentBalance.ToString("N2") + " ريال",
                            normalFont,
                            new BaseColor(253, 236, 236)));

                        // هذا السطر مهم جدًا؛ بدونه لن يظهر الملخص في PDF
                        document.Add(summaryTable);

                        // =============================================
                        // 6. إغلاق الملف
                        // =============================================
                        document.Close();
                    }

                    MessageBox.Show(
                        "تم تصدير التقرير مع العمليات والملخص بنجاح.",
                        "تم التصدير",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تصدير التقرير:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        //<<------------------------------------------------------>>

    }
}
