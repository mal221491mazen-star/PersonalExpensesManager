using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalExpensesManager
{
    public class ReportTransactionRow
    {
        //تجهيز بيانات التقرير أولًا 
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }


    }
}

/*
 * حساب إجمالي الدخل

    SELECT ISNULL(SUM(T.Amount), 0) AS TotalIncome
    FROM Transactions AS T
    INNER JOIN Categories AS C
        ON T.CategoryId = C.CategoryId
    WHERE C.CategoryType = N'دخل';
----------------------------
* حساب إجمالي المصروفات
    SELECT ISNULL(SUM(T.Amount), 0) AS TotalExpenses
    FROM Transactions AS T
    INNER JOIN Categories AS C
        ON T.CategoryId = C.CategoryId
    WHERE C.CategoryType = N'مصروف';
--------------------------
* حساب الرصيد الحالي
    SELECT
        ISNULL(SUM(CASE WHEN C.CategoryType = N'دخل' THEN T.Amount ELSE 0 END), 0)
        -
        ISNULL(SUM(CASE WHEN C.CategoryType = N'مصروف' THEN T.Amount ELSE 0 END), 0)
        AS CurrentBalance
    FROM Transactions AS T
    INNER JOIN Categories AS C
        ON T.CategoryId = C.CategoryId;
------------------------
* عرض مصروفات شهر معين
    DECLARE @Year INT = 2026;
    DECLARE @Month INT = 8;

    SELECT
        C.CategoryName,
        SUM(T.Amount) AS TotalAmount
    FROM Transactions AS T
    INNER JOIN Categories AS C
        ON T.CategoryId = C.CategoryId
    WHERE C.CategoryType = N'مصروف'
      AND YEAR(T.TransactionDate) = @Year
      AND MONTH(T.TransactionDate) = @Month
    GROUP BY C.CategoryName
    ORDER BY TotalAmount DESC;
-----------------------------------
* مقارنة الميزانية بالمصروف الفعلي
    DECLARE @Year INT = 2026;
    DECLARE @Month INT = 8;

    SELECT
        B.BudgetId,
        C.CategoryName,
        B.LimitAmount,
        ISNULL(SUM(T.Amount), 0) AS ActualExpenses,
        B.LimitAmount - ISNULL(SUM(T.Amount), 0) AS RemainingAmount
    FROM Budgets AS B
    INNER JOIN Categories AS C
        ON B.CategoryId = C.CategoryId
    LEFT JOIN Transactions AS T
        ON T.CategoryId = B.CategoryId
       AND YEAR(T.TransactionDate) = B.BudgetYear
       AND MONTH(T.TransactionDate) = B.BudgetMonth
    WHERE B.BudgetYear = @Year
      AND B.BudgetMonth = @Month
    GROUP BY
        B.BudgetId,
        C.CategoryName,
        B.LimitAmount
    ORDER BY C.CategoryName;
 */