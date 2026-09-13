# Personal Expenses Manager

A desktop personal finance management system built with **C#**, **.NET Framework**, **Windows Forms**, **SQL Server**, and **LINQ to SQL**.

The application helps users record income and expenses, organize transactions by category, monitor their current balance, analyze monthly spending, manage budgets, and export reports to Excel and PDF.

## Project Overview

Personal Expenses Manager is a Windows desktop application designed to provide a simple and organized way to manage personal finances. Users can add income and expense transactions, search and filter records, view dashboard summaries, analyze spending through charts, and generate reports.

## Main Features

- Dashboard showing total income, total expenses, and current balance.
- Add income and expense transactions.
- Categorize transactions such as salary, food, transportation, bills, health, and entertainment.
- Search and filter transactions by date, type, category, and description.
- Display recent transactions on the dashboard.
- Monthly expense chart.
- Expense analysis by category.
- Monthly budget management.
- Reports with financial summaries and charts.
- Export reports to Excel and PDF.
- Validation of transaction input before saving.

## Screenshots

### Dashboard

The dashboard provides a quick overview of the user's financial position, including income, expenses, balance, monthly spending, and recent transactions.

![Dashboard](Screenshots/dashboard.png)

### Add Transaction

The add transaction screen allows users to select the transaction type, choose a category, enter the amount, select a date, and add an optional description.

![Add Transaction](Screenshots/add-transaction.png)

### Transactions

The transactions screen displays saved operations in a searchable and filterable table.

![Transactions](Screenshots/transactions.png)

### Reports

The reports screen presents financial summaries and charts for selected periods.

![Reports](Screenshots/reports.png)

## Technologies Used

| Technology | Purpose |
|---|---|
| C# | Application logic and event handling |
| .NET Framework | Desktop application framework |
| Windows Forms | User interface development |
| Guna UI / Guna2 | Modern interface components and styling |
| SQL Server | Relational database storage |
| LINQ to SQL | Database access from C# |
| `PEDBDataContext` | Generated database context |
| DataGridView | Displaying transaction records |
| Chart Control | Financial data visualization |
| ClosedXML | Excel report generation |
| iTextSharp | PDF report generation |

## Database Design

The application uses three main tables:

| Table | Description |
|---|---|
| `Categories` | Stores income and expense categories |
| `Transactions` | Stores all financial operations |
| `Budgets` | Stores monthly spending limits by category |

The main relationships are:

```text
Categories 1 ─────────── ∞ Transactions
Categories 1 ─────────── ∞ Budgets
```

## Project Structure

```text
Personal-Expenses-Manager-WinForms/
│
├── PersonalExpensesManager.sln
├── PersonalExpensesManager/
│   ├── Forms/
│   ├── Models/
│   ├── Data/
│   └── Resources/
│
├── Database/
│   ├── PersonalExpensesDB.sql
│   └── PersonalExpensesDB_SampleData.sql
│
├── Screenshots/
│   ├── dashboard.png
│   ├── add-transaction.png
│   ├── transactions.png
│   └── reports.png
│
├── README.md
└── .gitignore
```

## Requirements

Before running the project, install or configure the following:

- Visual Studio with Windows Forms and .NET Framework support.
- SQL Server or SQL Server Express.
- SQL Server Management Studio, recommended for database setup.
- .NET Framework version compatible with the project.
- NuGet packages used by the project, including ClosedXML and iTextSharp.

## How to Run the Project

1. Clone or download the repository.
2. Open `PersonalExpensesManager.sln` in Visual Studio.
3. Open the database script located in the `Database` folder.
4. Execute `PersonalExpensesDB.sql` in SQL Server Management Studio.
5. Execute `PersonalExpensesDB_SampleData.sql` to insert demonstration data.
6. Check the database connection string in the project configuration.
7. Restore NuGet packages.
8. Build the solution.
9. Run the application.

## Example Connection String

For a local SQL Server instance using Windows Authentication:

```text
Data Source=.;Initial Catalog=PersonalExpensesDB;Integrated Security=True
```

Do not publish passwords or private credentials in the repository.

## How to Add Screenshots

Create a folder named `Screenshots` in the same location as this README file. Save your screenshots using these names:

```text
Screenshots/dashboard.png
Screenshots/add-transaction.png
Screenshots/transactions.png
Screenshots/reports.png
```

The image link in Markdown must match the file path exactly:

```markdown
![Dashboard](Screenshots/dashboard.png)
```

Use PNG images with a clear resolution. A width between 1200 and 1800 pixels is suitable for interface screenshots.

## Git Commands

After adding the README and screenshots to the project folder, run:

```bash
git add README.md Screenshots/
git commit -m "Add professional README and project screenshots"
git push
```

If this is the first upload to GitHub, use:

```bash
git branch -M main
git push -u origin main
```

## Future Improvements

- User authentication and role management.
- Multiple currencies.
- Automatic notifications when a budget is exceeded.
- Database backup and restore.
- Recurring transactions.
- Receipt image attachments.
- Cloud synchronization.
- Mobile or web version of the application.

## Author

**Your Name**  
C# Developer | .NET Developer | Desktop Application Developer

- LinkedIn: [Your LinkedIn Profile](https://www.linkedin.com/in/mazen-al-hajouri-30031839a/)
- GitHub: [Your GitHub Profile](https://github.com/mal221491mazen-star)

## License

This project is created for educational and portfolio purposes. Add an appropriate license if you plan to distribute or reuse the project publicly.
