/* =========================================================
   Sample data for PersonalExpensesDB
   The script is safe to run more than once.
   ========================================================= */
--USE PersonalExpensesDB;
--GO

/* ---------------------------------------------------------
   1) Ensure categories exist for the sample data
   --------------------------------------------------------- */

INSERT INTO dbo.Categories (CategoryName, CategoryType)
SELECT V.CategoryName, V.CategoryType
FROM
(
    VALUES
        (N'راتب',       N'دخل'),
        (N'عمل إضافي',  N'دخل'),
        (N'استثمار',     N'دخل'),
        (N'أخرى',        N'دخل'),
        (N'طعام',        N'مصروف'),
        (N'مواصلات',     N'مصروف'),
        (N'فواتير',      N'مصروف'),
        (N'إيجار',       N'مصروف'),
        (N'صحة',         N'مصروف'),
        (N'ترفيه',       N'مصروف'),
        (N'تعليم',       N'مصروف'),
        (N'تسوق',        N'مصروف'),
        (N'أخرى',        N'مصروف')
) AS V(CategoryName, CategoryType)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories C
    WHERE C.CategoryName = V.CategoryName
      AND C.CategoryType = V.CategoryType
);
GO

/* ---------------------------------------------------------
   2) Insert transactions for dashboard, reports and table
   --------------------------------------------------------- */

INSERT INTO dbo.Transactions
    (CategoryId, Amount, TransactionDate, Description)
SELECT
    C.CategoryId,
    V.Amount,
    V.TransactionDate,
    V.Description
FROM
(
    VALUES
        /* January 2026 */
        (N'راتب',       N'دخل',   CAST('2026-01-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر يناير'),
        (N'إيجار',      N'مصروف', CAST('2026-01-02' AS DATE), CAST(1800.00 AS DECIMAL(18,2)), N'إيجار المنزل'),
        (N'طعام',       N'مصروف', CAST('2026-01-05' AS DATE), CAST(650.00  AS DECIMAL(18,2)), N'مشتريات غذائية'),
        (N'مواصلات',    N'مصروف', CAST('2026-01-08' AS DATE), CAST(300.00  AS DECIMAL(18,2)), N'وقود ومواصلات'),
        (N'فواتير',     N'مصروف', CAST('2026-01-15' AS DATE), CAST(420.00  AS DECIMAL(18,2)), N'كهرباء وإنترنت'),

        /* February 2026 */
        (N'راتب',       N'دخل',   CAST('2026-02-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر فبراير'),
        (N'إيجار',      N'مصروف', CAST('2026-02-02' AS DATE), CAST(1800.00 AS DECIMAL(18,2)), N'إيجار المنزل'),
        (N'طعام',       N'مصروف', CAST('2026-02-06' AS DATE), CAST(720.00  AS DECIMAL(18,2)), N'مشتريات غذائية'),
        (N'مواصلات',    N'مصروف', CAST('2026-02-10' AS DATE), CAST(280.00  AS DECIMAL(18,2)), N'وقود ومواصلات'),
        (N'ترفيه',      N'مصروف', CAST('2026-02-20' AS DATE), CAST(350.00  AS DECIMAL(18,2)), N'سينما ومطعم'),

        /* March 2026 */
        (N'راتب',       N'دخل',   CAST('2026-03-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر مارس'),
        (N'عمل إضافي',  N'دخل',   CAST('2026-03-12' AS DATE), CAST(900.00  AS DECIMAL(18,2)), N'مشروع جانبي'),
        (N'إيجار',      N'مصروف', CAST('2026-03-02' AS DATE), CAST(1800.00 AS DECIMAL(18,2)), N'إيجار المنزل'),
        (N'طعام',       N'مصروف', CAST('2026-03-07' AS DATE), CAST(800.00  AS DECIMAL(18,2)), N'مشتريات غذائية'),
        (N'صحة',        N'مصروف', CAST('2026-03-18' AS DATE), CAST(250.00  AS DECIMAL(18,2)), N'صيدلية'),

        /* April 2026 */
        (N'راتب',       N'دخل',   CAST('2026-04-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر أبريل'),
        (N'إيجار',      N'مصروف', CAST('2026-04-02' AS DATE), CAST(1800.00 AS DECIMAL(18,2)), N'إيجار المنزل'),
        (N'فواتير',     N'مصروف', CAST('2026-04-12' AS DATE), CAST(510.00  AS DECIMAL(18,2)), N'فواتير الخدمات'),
        (N'تسوق',       N'مصروف', CAST('2026-04-16' AS DATE), CAST(650.00  AS DECIMAL(18,2)), N'ملابس واحتياجات منزلية'),
        (N'مواصلات',    N'مصروف', CAST('2026-04-22' AS DATE), CAST(320.00  AS DECIMAL(18,2)), N'وقود ومواصلات'),

        /* May 2026 */
        (N'راتب',       N'دخل',   CAST('2026-05-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر مايو'),
        (N'عمل إضافي',  N'دخل',   CAST('2026-05-10' AS DATE), CAST(1000.00 AS DECIMAL(18,2)), N'دخل من عمل إضافي'),
        (N'إيجار',      N'مصروف', CAST('2026-05-02' AS DATE), CAST(1800.00 AS DECIMAL(18,2)), N'إيجار المنزل'),
        (N'طعام',       N'مصروف', CAST('2026-05-08' AS DATE), CAST(950.00  AS DECIMAL(18,2)), N'مشتريات غذائية'),
        (N'فواتير',     N'مصروف', CAST('2026-05-15' AS DATE), CAST(560.00  AS DECIMAL(18,2)), N'فاتورة الكهرباء'),
        (N'ترفيه',      N'مصروف', CAST('2026-05-24' AS DATE), CAST(450.00  AS DECIMAL(18,2)), N'خروج مع الأصدقاء'),

        /* June 2026 */
        (N'راتب',       N'دخل',   CAST('2026-06-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر يونيو'),
        (N'إيجار',      N'مصروف', CAST('2026-06-02' AS DATE), CAST(1800.00 AS DECIMAL(18,2)), N'إيجار المنزل'),
        (N'طعام',       N'مصروف', CAST('2026-06-09' AS DATE), CAST(870.00  AS DECIMAL(18,2)), N'مشتريات غذائية'),
        (N'مواصلات',    N'مصروف', CAST('2026-06-14' AS DATE), CAST(340.00  AS DECIMAL(18,2)), N'وقود ومواصلات'),
        (N'تعليم',      N'مصروف', CAST('2026-06-21' AS DATE), CAST(500.00  AS DECIMAL(18,2)), N'دورة تدريبية'),

        /* July 2026 */
        (N'راتب',       N'دخل',   CAST('2026-07-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر يوليو'),
        (N'استثمار',    N'دخل',   CAST('2026-07-18' AS DATE), CAST(1200.00 AS DECIMAL(18,2)), N'عائد استثمار'),
        (N'إيجار',      N'مصروف', CAST('2026-07-02' AS DATE), CAST(1800.00 AS DECIMAL(18,2)), N'إيجار المنزل'),
        (N'طعام',       N'مصروف', CAST('2026-07-06' AS DATE), CAST(780.00  AS DECIMAL(18,2)), N'مشتريات غذائية'),
        (N'فواتير',     N'مصروف', CAST('2026-07-14' AS DATE), CAST(480.00  AS DECIMAL(18,2)), N'فواتير الخدمات'),
        (N'تسوق',       N'مصروف', CAST('2026-07-25' AS DATE), CAST(800.00  AS DECIMAL(18,2)), N'احتياجات منزلية'),

        /* August 2026 - current demonstration month */
        (N'راتب',       N'دخل',   CAST('2026-08-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر أغسطس'),
        (N'عمل إضافي',  N'دخل',   CAST('2026-08-10' AS DATE), CAST(1000.00 AS DECIMAL(18,2)), N'مشروع جانبي'),
        (N'إيجار',      N'مصروف', CAST('2026-08-02' AS DATE), CAST(1800.00 AS DECIMAL(18,2)), N'إيجار المنزل'),
        (N'طعام',       N'مصروف', CAST('2026-08-04' AS DATE), CAST(920.00  AS DECIMAL(18,2)), N'شراء مواد غذائية'),
        (N'مواصلات',    N'مصروف', CAST('2026-08-06' AS DATE), CAST(310.00  AS DECIMAL(18,2)), N'مواصلات'),
        (N'فواتير',     N'مصروف', CAST('2026-08-12' AS DATE), CAST(520.00  AS DECIMAL(18,2)), N'فاتورة الإنترنت والكهرباء'),
        (N'ترفيه',      N'مصروف', CAST('2026-08-20' AS DATE), CAST(400.00  AS DECIMAL(18,2)), N'خروج مع الأصدقاء'),
        (N'صحة',        N'مصروف', CAST('2026-08-23' AS DATE), CAST(180.00  AS DECIMAL(18,2)), N'أدوية'),
        (N'تسوق',       N'مصروف', CAST('2026-08-25' AS DATE), CAST(600.00  AS DECIMAL(18,2)), N'مشتريات منزلية')
) AS V(CategoryName, CategoryType, TransactionDate, Amount, Description)
INNER JOIN dbo.Categories C
    ON C.CategoryName = V.CategoryName
   AND C.CategoryType = V.CategoryType
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Transactions T
    WHERE T.CategoryId = C.CategoryId
      AND T.TransactionDate = V.TransactionDate
      AND T.Amount = V.Amount
      AND ISNULL(T.Description, N'') = V.Description
);
GO

/* ---------------------------------------------------------
   3) Insert monthly budgets for the reports screen
   --------------------------------------------------------- */

INSERT INTO dbo.Budgets (CategoryId, BudgetYear, BudgetMonth, LimitAmount)
SELECT C.CategoryId, 2026, 8, V.LimitAmount
FROM
(
    VALUES
        (N'طعام',       N'مصروف', CAST(1500.00 AS DECIMAL(18,2))),
        (N'مواصلات',    N'مصروف', CAST(700.00  AS DECIMAL(18,2))),
        (N'فواتير',     N'مصروف', CAST(800.00  AS DECIMAL(18,2))),
        (N'ترفيه',      N'مصروف', CAST(500.00  AS DECIMAL(18,2))),
        (N'صحة',        N'مصروف', CAST(400.00  AS DECIMAL(18,2))),
        (N'تسوق',       N'مصروف', CAST(1000.00 AS DECIMAL(18,2)))
) AS V(CategoryName, CategoryType, LimitAmount)
INNER JOIN dbo.Categories C
    ON C.CategoryName = V.CategoryName
   AND C.CategoryType = V.CategoryType
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Budgets B
    WHERE B.CategoryId = C.CategoryId
      AND B.BudgetYear = 2026
      AND B.BudgetMonth = 8
);
GO

/* =========================================================
   4) Queries for each application screen
   ========================================================= */

/* A) tabAddTransaction: active categories for ComboBox */
SELECT
    CategoryId,
    CategoryName,
    CategoryType
FROM dbo.Categories
WHERE IsActive = 1
ORDER BY CategoryType, CategoryName;

/* B) tabTransactions: all rows for the DataGridView */
SELECT
    T.TransactionId,
    T.TransactionDate,
    C.CategoryName,
    C.CategoryType,
    T.Amount,
    T.Description
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId
ORDER BY T.TransactionDate DESC, T.TransactionId DESC;

/* C) tabDashboard: total income, expenses and balance */
SELECT
    ISNULL(SUM(CASE WHEN C.CategoryType = N'دخل' THEN T.Amount ELSE 0 END), 0) AS TotalIncome,
    ISNULL(SUM(CASE WHEN C.CategoryType = N'مصروف' THEN T.Amount ELSE 0 END), 0) AS TotalExpenses,
    ISNULL(SUM(CASE WHEN C.CategoryType = N'دخل' THEN T.Amount ELSE -T.Amount END), 0) AS CurrentBalance
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId;

/* D) tabDashboard: recent transactions */
SELECT TOP (10)
    T.TransactionId,
    T.TransactionDate,
    C.CategoryName,
    C.CategoryType,
    T.Amount,
    T.Description
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId
ORDER BY T.TransactionDate DESC, T.TransactionId DESC;

/* E) tabDashboard: monthly expenses */
SELECT
    YEAR(T.TransactionDate) AS ExpenseYear,
    MONTH(T.TransactionDate) AS ExpenseMonth,
    SUM(T.Amount) AS TotalExpenses
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId
WHERE C.CategoryType = N'مصروف'
GROUP BY YEAR(T.TransactionDate), MONTH(T.TransactionDate)
ORDER BY ExpenseYear, ExpenseMonth;

/* F) tabReports: expenses by category for August 2026 */
SELECT
    C.CategoryName,
    SUM(T.Amount) AS TotalAmount
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId
WHERE C.CategoryType = N'مصروف'
  AND T.TransactionDate >= '2026-08-01'
  AND T.TransactionDate <  '2026-09-01'
GROUP BY C.CategoryName
ORDER BY TotalAmount DESC;

/* G) tabReports: income and expenses by month */
SELECT
    YEAR(T.TransactionDate) AS ReportYear,
    MONTH(T.TransactionDate) AS ReportMonth,
    SUM(CASE WHEN C.CategoryType = N'دخل' THEN T.Amount ELSE 0 END) AS TotalIncome,
    SUM(CASE WHEN C.CategoryType = N'مصروف' THEN T.Amount ELSE 0 END) AS TotalExpenses
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId
GROUP BY YEAR(T.TransactionDate), MONTH(T.TransactionDate)
ORDER BY ReportYear, ReportMonth;

/* H) tabReports: budget comparison for August 2026 */
SELECT
    B.BudgetId,
    C.CategoryName,
    B.LimitAmount,
    ISNULL(SUM(T.Amount), 0) AS ActualExpenses,
    B.LimitAmount - ISNULL(SUM(T.Amount), 0) AS RemainingAmount,
    CASE
        WHEN ISNULL(SUM(T.Amount), 0) > B.LimitAmount
            THEN N'تجاوز الميزانية'
        ELSE N'ضمن الميزانية'
    END AS BudgetStatus
FROM dbo.Budgets B
INNER JOIN dbo.Categories C
    ON C.CategoryId = B.CategoryId
LEFT JOIN dbo.Transactions T
    ON T.CategoryId = B.CategoryId
   AND T.TransactionDate >= DATEFROMPARTS(B.BudgetYear, B.BudgetMonth, 1)
   AND T.TransactionDate < DATEADD(MONTH, 1, DATEFROMPARTS(B.BudgetYear, B.BudgetMonth, 1))
WHERE B.BudgetYear = 2026
  AND B.BudgetMonth = 8
GROUP BY
    B.BudgetId,
    C.CategoryName,
    B.LimitAmount
ORDER BY C.CategoryName;
GO
