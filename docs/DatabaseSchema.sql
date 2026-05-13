CREATE TABLE Customers (
    Id INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(120) NOT NULL,
    Email NVARCHAR(160) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(30) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Subscriptions (
    Id INT IDENTITY PRIMARY KEY,
    CustomerId INT NOT NULL,
    Type INT NOT NULL,
    ProviderName NVARCHAR(120) NOT NULL,
    SubscriptionNumber NVARCHAR(80) NOT NULL,
    Status INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Subscriptions_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Subscriptions_Customer_Provider_Number UNIQUE (CustomerId, ProviderName, SubscriptionNumber)
);

CREATE TABLE Payments (
    Id INT IDENTITY PRIMARY KEY,
    SubscriptionId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Period NVARCHAR(7) NOT NULL,
    Status INT NOT NULL,
    TransactionReference NVARCHAR(120) NULL,
    FailureReason NVARCHAR(300) NULL,
    CONSTRAINT FK_Payments_Subscriptions FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Payments_Subscription_Period_Status ON Payments(SubscriptionId, Period, Status);
