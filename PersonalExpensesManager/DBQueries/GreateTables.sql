-- إنشاء جدول التصنيفات 
CREATE TABLE Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    CategoryType NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_Categories_CategoryType
        CHECK (CategoryType IN (N'دخل', N'مصروف')),

    CONSTRAINT UQ_Categories_Name_Type
        UNIQUE (CategoryName, CategoryType)
);
GO
-- إنشاء جدول المعاملات

CREATE TABLE Transactions
(
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TransactionDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    Description NVARCHAR(250) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT CK_Transactions_Amount
        CHECK (Amount > 0),

    CONSTRAINT FK_Transactions_Categories
        FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId)
);
GO
