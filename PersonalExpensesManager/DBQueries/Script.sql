   -- Personal Expenses Manager
  --  Database: PersonalExpensesDB
   -- SQL Server / SQL Server Express / LocalDB


/* =========================================================
   1) Create and select the database
   ========================================================= 
*/

--IF DB_ID(N'PersonalExpensesDB') IS NULL
--BEGIN
--    CREATE DATABASE PersonalExpensesDB;
--END;
--GO
/*          
مشروع إدارة المصاريف الشخصية سنستخدم ثلاثة جداول:
                    | الجدول         | فائدته الأساسية                                         |
                    | -------------- | ------------------------------------------------------- |
                    | `Categories`   | تخزين تصنيفات العمليات مثل الطعام والمواصلات والراتب    |
                    | `Transactions` | تخزين كل عمليات الدخل والمصروف                          |
                    | `Budgets`      | تحديد ميزانية شهرية لكل تصنيف ومقارنتها بالمصروف الفعلي |

USE PersonalExpensesDB;
GO

*/
/* =========================================================
   2) Create Categories table
   ========================================================= 
          Categories إنشاء جدول التصنيفات
        هذا الجدول يحفظ أنواع العمليات المالية. بدلًا من كتابة كلمة "طعام" أو "مواصلات" في كل عملية
        ، نضعها مرة واحدة داخل هذا الجدول ونستخدم رقمها في جدول العمليات.

| الحقل          | فائدته                        |
| -------------- | ----------------------------- |
| `CategoryId`   | رقم تلقائي ومميز لكل تصنيف    |
| `CategoryName` | اسم التصنيف، مثل طعام أو راتب |
| `CategoryType` | يحدد هل التصنيف دخل أم مصروف  |
| `IsActive`     | يسمح بإيقاف التصنيف دون حذفه  |
*/

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        CategoryId INT IDENTITY(1,1) NOT NULL,
        CategoryName NVARCHAR(100) NOT NULL,
        CategoryType NVARCHAR(20) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),
        CONSTRAINT CK_Categories_CategoryType
            CHECK (CategoryType IN (N'دخل', N'مصروف')),
        CONSTRAINT UQ_Categories_Name_Type
            UNIQUE (CategoryName, CategoryType)
    );
END;
GO

/* =========================================================
   3) Create Transactions table <<هذا هو الجدول الرئيسي في المشروع.
                                 يسجل كل دخل أو مصروف يقوم المستخدم بإضافته.>>
    | الحقل             | فائدته                                     |
    | ----------------- | ------------------------------------------ |
    | `TransactionId`   | رقم مميز لكل عملية                         |
    | `CategoryId`      | يربط العملية بتصنيفها في جدول `Categories` |
    | `Amount`          | قيمة الدخل أو المصروف                      |
    | `TransactionDate` | تاريخ حدوث العملية                         |
    | `Description`     | ملاحظات اختيارية عن العملية                 |
    | `CreatedAt`       | تاريخ ووقت إضافة العملية إلى النظام        |
   ========================================================= 

   */

IF OBJECT_ID(N'dbo.Transactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions
    (
        TransactionId INT IDENTITY(1,1) NOT NULL,
        CategoryId INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        TransactionDate DATE NOT NULL CONSTRAINT DF_Transactions_Date DEFAULT (CONVERT(DATE, GETDATE())),
        Description NVARCHAR(250) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Transactions_CreatedAt DEFAULT (SYSDATETIME()),
        UpdatedAt DATETIME2(0) NULL,

        CONSTRAINT PK_Transactions PRIMARY KEY (TransactionId),
        CONSTRAINT CK_Transactions_Amount CHECK (Amount > 0),
        CONSTRAINT FK_Transactions_Categories
            FOREIGN KEY (CategoryId)
            REFERENCES dbo.Categories(CategoryId)
    );
END;
GO

/* =========================================================
   4) Create Budgets table <<هذا الجدول اختياري في النسخة الأولى، لكنه مفيد جدًا
                                    . يسمح للمستخدم بتحديد مبلغ أقصى لمصروفات تصنيف معين خلال شهر محدد.>>
    | الحقل         |        فائدته                     |
    | ------------- | --------------------------------- |
    | `BudgetId`    | رقم مميز لكل ميزانية              |
    | `CategoryId`  | التصنيف الذي تنطبق عليه الميزانية |
    | `BudgetYear`  | سنة الميزانية                     |
    | `BudgetMonth` | شهر الميزانية من 1 إلى 12         |
    | `LimitAmount` | الحد الأقصى المسموح به للمصروفات  |
   ========================================================= */

IF OBJECT_ID(N'dbo.Budgets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Budgets
    (
        BudgetId INT IDENTITY(1,1) NOT NULL,
        CategoryId INT NOT NULL,
        BudgetYear INT NOT NULL,
        BudgetMonth INT NOT NULL,
        LimitAmount DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Budgets_CreatedAt DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_Budgets PRIMARY KEY (BudgetId),
        CONSTRAINT FK_Budgets_Categories
            FOREIGN KEY (CategoryId)
            REFERENCES dbo.Categories(CategoryId),
        CONSTRAINT CK_Budgets_Year
            CHECK (BudgetYear BETWEEN 2000 AND 2100),
        CONSTRAINT CK_Budgets_Month
            CHECK (BudgetMonth BETWEEN 1 AND 12),
        CONSTRAINT CK_Budgets_LimitAmount
            CHECK (LimitAmount > 0),
        CONSTRAINT UQ_Budgets_Category_Month
            UNIQUE (CategoryId, BudgetYear, BudgetMonth)
    );
END;
GO

/* =========================================================
   5) Useful indexes
   ========================================================= */

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Transactions_Date'
      AND object_id = OBJECT_ID(N'dbo.Transactions')
)
BEGIN
    CREATE INDEX IX_Transactions_Date
        ON dbo.Transactions(TransactionDate);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Transactions_Category'
      AND object_id = OBJECT_ID(N'dbo.Transactions')
)
BEGIN
    CREATE INDEX IX_Transactions_Category
        ON dbo.Transactions(CategoryId);
END;
GO

/* =========================================================
   6) Initial categories
   ========================================================= */

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

/* =========================================================
   7) Sample transactions
   These rows are inserted only when the table is empty.
   ========================================================= */

IF NOT EXISTS (SELECT 1 FROM dbo.Transactions)
BEGIN
    INSERT INTO dbo.Transactions
        (CategoryId, Amount, TransactionDate, Description)
    SELECT C.CategoryId, V.Amount, V.TransactionDate, V.Description
    FROM
    (
        VALUES
            (N'راتب',      N'دخل',    CAST('2026-08-01' AS DATE), CAST(8000.00 AS DECIMAL(18,2)), N'راتب شهر أغسطس'),
            (N'طعام',      N'مصروف',  CAST('2026-08-02' AS DATE), CAST(250.00 AS DECIMAL(18,2)), N'شراء مواد غذائية'),
            (N'مواصلات',   N'مصروف',  CAST('2026-08-03' AS DATE), CAST(100.00 AS DECIMAL(18,2)), N'مواصلات'),
            (N'فواتير',    N'مصروف',  CAST('2026-08-05' AS DATE), CAST(450.00 AS DECIMAL(18,2)), N'فاتورة الإنترنت'),
            (N'ترفيه',     N'مصروف',  CAST('2026-08-06' AS DATE), CAST(200.00 AS DECIMAL(18,2)), N'خروج مع الأصدقاء'),
            (N'عمل إضافي', N'دخل',    CAST('2026-08-10' AS DATE), CAST(1000.00 AS DECIMAL(18,2)), N'دخل من عمل إضافي')
    ) AS V(CategoryName, CategoryType, TransactionDate, Amount, Description)
    INNER JOIN dbo.Categories C
        ON C.CategoryName = V.CategoryName
       AND C.CategoryType = V.CategoryType;
END;
GO

/* =========================================================
   8) Sample monthly budgets
   ========================================================= */

INSERT INTO dbo.Budgets (CategoryId, BudgetYear, BudgetMonth, LimitAmount)
SELECT C.CategoryId, V.BudgetYear, V.BudgetMonth, V.LimitAmount
FROM
(
    VALUES
        (N'طعام',    N'مصروف', 2026, 8, CAST(1500.00 AS DECIMAL(18,2))),
        (N'مواصلات', N'مصروف', 2026, 8, CAST(700.00  AS DECIMAL(18,2))),
        (N'فواتير',  N'مصروف', 2026, 8, CAST(600.00  AS DECIMAL(18,2))),
        (N'ترفيه',   N'مصروف', 2026, 8, CAST(500.00  AS DECIMAL(18,2)))
) AS V(CategoryName, CategoryType, BudgetYear, BudgetMonth, LimitAmount)
INNER JOIN dbo.Categories C
    ON C.CategoryName = V.CategoryName
   AND C.CategoryType = V.CategoryType
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Budgets B
    WHERE B.CategoryId = C.CategoryId
      AND B.BudgetYear = V.BudgetYear
      AND B.BudgetMonth = V.BudgetMonth
);
GO

/* =========================================================
   9) View: all transactions
   ========================================================= */

CREATE OR ALTER VIEW dbo.vw_Transactions
AS
SELECT
    T.TransactionId,
    T.CategoryId,
    C.CategoryName,
    C.CategoryType,
    T.Amount,
    T.TransactionDate,
    T.Description,
    T.CreatedAt,
    T.UpdatedAt
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId;
GO

/* =========================================================
   10) View: dashboard summary
   ========================================================= */

CREATE OR ALTER VIEW dbo.vw_DashboardSummary
AS
SELECT
    ISNULL(SUM(CASE WHEN C.CategoryType = N'دخل' THEN T.Amount ELSE 0 END), 0) AS TotalIncome,
    ISNULL(SUM(CASE WHEN C.CategoryType = N'مصروف' THEN T.Amount ELSE 0 END), 0) AS TotalExpenses,
    ISNULL(SUM(CASE WHEN C.CategoryType = N'دخل' THEN T.Amount ELSE -T.Amount END), 0) AS CurrentBalance
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId;
GO

/* =========================================================
   11) View: monthly category expenses
   ========================================================= */

CREATE OR ALTER VIEW dbo.vw_MonthlyCategoryExpenses
AS
SELECT
    YEAR(T.TransactionDate) AS ExpenseYear,
    MONTH(T.TransactionDate) AS ExpenseMonth,
    C.CategoryId,
    C.CategoryName,
    SUM(T.Amount) AS TotalExpenses
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId
WHERE C.CategoryType = N'مصروف'
GROUP BY
    YEAR(T.TransactionDate),
    MONTH(T.TransactionDate),
    C.CategoryId,
    C.CategoryName;
GO

/* =========================================================
   12) Stored procedure: add a transaction
   ========================================================= */

CREATE OR ALTER PROCEDURE dbo.AddTransaction
    @CategoryId INT,
    @Amount DECIMAL(18,2),
    @TransactionDate DATE,
    @Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Amount <= 0
        THROW 50001, N'يجب أن يكون المبلغ أكبر من صفر.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Categories
        WHERE CategoryId = @CategoryId
          AND IsActive = 1
    )
        THROW 50002, N'التصنيف غير موجود أو غير فعال.', 1;

    INSERT INTO dbo.Transactions
        (CategoryId, Amount, TransactionDate, Description)
    VALUES
        (@CategoryId, @Amount, @TransactionDate, @Description);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewTransactionId;
END;
GO

/* =========================================================
   13) Stored procedure: update a transaction
   ========================================================= */

CREATE OR ALTER PROCEDURE dbo.UpdateTransaction
    @TransactionId INT,
    @CategoryId INT,
    @Amount DECIMAL(18,2),
    @TransactionDate DATE,
    @Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Amount <= 0
        THROW 50003, N'يجب أن يكون المبلغ أكبر من صفر.', 1;

    UPDATE dbo.Transactions
    SET
        CategoryId = @CategoryId,
        Amount = @Amount,
        TransactionDate = @TransactionDate,
        Description = @Description,
        UpdatedAt = SYSDATETIME()
    WHERE TransactionId = @TransactionId;

    IF @@ROWCOUNT = 0
        THROW 50004, N'العملية غير موجودة.', 1;
END;
GO

/* =========================================================
   14) Stored procedure: delete a transaction
   ========================================================= */

CREATE OR ALTER PROCEDURE dbo.DeleteTransaction
    @TransactionId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Transactions
    WHERE TransactionId = @TransactionId;

    IF @@ROWCOUNT = 0
        THROW 50005, N'العملية غير موجودة.', 1;
END;
GO

/* =========================================================
   15) Stored procedure: search and filter transactions
   ========================================================= */

CREATE OR ALTER PROCEDURE dbo.SearchTransactions
    @SearchText NVARCHAR(250) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CategoryType NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        T.TransactionId,
        T.CategoryId,
        C.CategoryName,
        C.CategoryType,
        T.Amount,
        T.TransactionDate,
        T.Description,
        T.CreatedAt
    FROM dbo.Transactions T
    INNER JOIN dbo.Categories C
        ON C.CategoryId = T.CategoryId
    WHERE
        (
            @SearchText IS NULL
            OR @SearchText = N''
            OR C.CategoryName LIKE N'%' + @SearchText + N'%'
            OR ISNULL(T.Description, N'') LIKE N'%' + @SearchText + N'%'
        )
        AND (@FromDate IS NULL OR T.TransactionDate >= @FromDate)
        AND (@ToDate IS NULL OR T.TransactionDate <= @ToDate)
        AND (@CategoryType IS NULL OR @CategoryType = N'' OR C.CategoryType = @CategoryType)
    ORDER BY T.TransactionDate DESC, T.TransactionId DESC;
END;
GO

/* =========================================================
   16) Basic test queries
   ========================================================= */

-- All categories
SELECT * FROM dbo.Categories ORDER BY CategoryType, CategoryName;

-- All transactions
SELECT * FROM dbo.vw_Transactions
ORDER BY TransactionDate DESC, TransactionId DESC;

-- Dashboard totals
SELECT * FROM dbo.vw_DashboardSummary;

-- Current month totals
SELECT
    ISNULL(SUM(CASE WHEN C.CategoryType = N'دخل' THEN T.Amount ELSE 0 END), 0) AS TotalIncome,
    ISNULL(SUM(CASE WHEN C.CategoryType = N'مصروف' THEN T.Amount ELSE 0 END), 0) AS TotalExpenses,
    ISNULL(SUM(CASE WHEN C.CategoryType = N'دخل' THEN T.Amount ELSE -T.Amount END), 0) AS CurrentBalance
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId
WHERE YEAR(T.TransactionDate) = YEAR(GETDATE())
  AND MONTH(T.TransactionDate) = MONTH(GETDATE());

-- Expenses grouped by category for a selected month
DECLARE @ReportYear INT = 2026;
DECLARE @ReportMonth INT = 8;

SELECT
    C.CategoryName,
    SUM(T.Amount) AS TotalAmount
FROM dbo.Transactions T
INNER JOIN dbo.Categories C
    ON C.CategoryId = T.CategoryId
WHERE C.CategoryType = N'مصروف'
  AND YEAR(T.TransactionDate) = @ReportYear
  AND MONTH(T.TransactionDate) = @ReportMonth
GROUP BY C.CategoryName
ORDER BY TotalAmount DESC;

-- Budget comparison for a selected month
DECLARE @ReportYear INT = 2026;
DECLARE @ReportMonth INT = 8;
SELECT
    B.BudgetId,
    C.CategoryName,
    B.BudgetYear,
    B.BudgetMonth,
    B.LimitAmount,
    ISNULL(SUM(T.Amount), 0) AS ActualExpenses,
    B.LimitAmount - ISNULL(SUM(T.Amount), 0) AS RemainingAmount,
    CASE
        WHEN ISNULL(SUM(T.Amount), 0) > B.LimitAmount THEN N'تجاوز الميزانية'
        ELSE N'ضمن الميزانية'
    END AS BudgetStatus
FROM dbo.Budgets B
INNER JOIN dbo.Categories C
    ON C.CategoryId = B.CategoryId
LEFT JOIN dbo.Transactions T
    ON T.CategoryId = B.CategoryId
   AND YEAR(T.TransactionDate) = B.BudgetYear
   AND MONTH(T.TransactionDate) = B.BudgetMonth
WHERE B.BudgetYear = @ReportYear
  AND B.BudgetMonth = @ReportMonth
GROUP BY
    B.BudgetId,
    C.CategoryName,
    B.BudgetYear,
    B.BudgetMonth,
    B.LimitAmount
ORDER BY C.CategoryName;

/* Examples for the stored procedures:

EXEC dbo.AddTransaction
    @CategoryId = 5,
    @Amount = 125.50,
    @TransactionDate = '2026-08-15',
    @Description = N'شراء غداء';

EXEC dbo.UpdateTransaction
    @TransactionId = 1,
    @CategoryId = 1,
    @Amount = 8500.00,
    @TransactionDate = '2026-08-01',
    @Description = N'تعديل الراتب';

EXEC dbo.DeleteTransaction
    @TransactionId = 1;

EXEC dbo.SearchTransactions
    @SearchText = N'فاتورة',
    @FromDate = '2026-08-01',
    @ToDate = '2026-08-31',
    @CategoryType = N'مصروف';
*/

/* =========================================================
   Table purposes:
   Categories    = income and expense categories.
   Transactions  = every income or expense record.
   Budgets       = monthly spending limits per expense category.
   Views         = ready-made dashboard/report queries.
   Procedures    = ready-made add, edit, delete, and search operations.
   ========================================================= */
