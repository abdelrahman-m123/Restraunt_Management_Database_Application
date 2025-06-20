DROP TABLE IF EXISTS Review;
DROP TABLE IF EXISTS Consists_of;
DROP TABLE IF EXISTS delivery;
DROP TABLE IF EXISTS [order];
DROP TABLE IF EXISTS Deliveryman;
DROP TABLE IF EXISTS payment;
DROP TABLE IF EXISTS menu_item;
DROP TABLE IF EXISTS Menu;
DROP TABLE IF EXISTS Restaurant_Address;
DROP TABLE IF EXISTS Restaurant;
DROP TABLE IF EXISTS Customer;


-- Create Customer table
CREATE TABLE Customer (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    Fname VARCHAR(50) NOT NULL,
    Mname VARCHAR(50),
    Lname VARCHAR(50),
    Email VARCHAR(100) UNIQUE NOT NULL,
    Phone_number VARCHAR(20) UNIQUE,
    Address VARCHAR(255)
);

-- Create Restaurant table
CREATE TABLE Restaurant (
    RestaurantID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100) NOT NULL,
    Phone_Number VARCHAR(20) UNIQUE,
    Opening_Hours VARCHAR(100)
);

-- Create Restaurant_Address table
CREATE TABLE Restaurant_Address (
    RestaurantID INT FOREIGN KEY REFERENCES Restaurant(RestaurantID),
    Address VARCHAR(255) NOT NULL,
    PRIMARY KEY (RestaurantID, Address)
);

-- Create Menu table
CREATE TABLE Menu (
    menuID INT PRIMARY KEY IDENTITY(1,1),
    RestaurantID INT FOREIGN KEY REFERENCES Restaurant(RestaurantID) NOT NULL,
    category VARCHAR(50),
    description VARCHAR(255)
);

-- Create menu_item table
CREATE TABLE menu_item (
    ItemID INT PRIMARY KEY IDENTITY(1,1),
    RestaurantID INT FOREIGN KEY REFERENCES Restaurant(RestaurantID) NOT NULL,
    Description VARCHAR(255),
    Price DECIMAL(10, 2) NOT NULL,
    category VARCHAR(50),
    menuID INT FOREIGN KEY REFERENCES Menu(menuID),
    order_id INT
);

-- Create payment table
CREATE TABLE payment (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    RestaurantID INT FOREIGN KEY REFERENCES Restaurant(RestaurantID),
    PaymentMethod VARCHAR(50) NOT NULL,
    PaymentStatus VARCHAR(20) NOT NULL,
    Total_Price DECIMAL(10, 2) NOT NULL
);

-- Create Deliveryman table
CREATE TABLE Deliveryman (
    DeliverManID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    VehicleType VARCHAR(50)
);

-- Create order table
CREATE TABLE [order] (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    CustomerID INT FOREIGN KEY REFERENCES Customer(CustomerID) NOT NULL,
    RestaurantID INT FOREIGN KEY REFERENCES Restaurant(RestaurantID) NOT NULL,
    PaymentID INT FOREIGN KEY REFERENCES payment(PaymentID),
    OrderDate DATE NOT NULL,
    Status VARCHAR(20) NOT NULL,
    CustomerAddress VARCHAR(255) NOT NULL
);

-- Add foreign key to menu_item table
ALTER TABLE menu_item 
ADD CONSTRAINT FK_MenuItem_Order FOREIGN KEY (order_id) REFERENCES [order](OrderID);

-- Create delivery table
CREATE TABLE delivery (
    DeliveryID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT FOREIGN KEY REFERENCES [order](OrderID) NOT NULL,
    DeliverManID INT FOREIGN KEY REFERENCES Deliveryman(DeliverManID) NOT NULL,
    DeliveryAddress VARCHAR(255) NOT NULL,
    DeliveryStatus VARCHAR(20) NOT NULL
);

-- Create Consists_of table
CREATE TABLE Consists_of (
    OrderID INT FOREIGN KEY REFERENCES [order](OrderID),
    ItemID INT FOREIGN KEY REFERENCES menu_item(ItemID),
    Quantity INT NOT NULL
);

-- Create Review table
CREATE TABLE Review (
    ReviewID INT PRIMARY KEY IDENTITY(1,1),
    CustomerID INT FOREIGN KEY REFERENCES Customer(CustomerID) NOT NULL,
    RestaurantID INT FOREIGN KEY REFERENCES Restaurant(RestaurantID) NOT NULL,
    Rate INT NOT NULL CHECK (Rate BETWEEN 1 AND 5)
);

-- Insert Customers
INSERT INTO Customer (Fname, Mname, Lname, Email, Phone_number, Address)
VALUES 
    ('Nour', NULL, 'Rehab', 'nourrehab2@gmail.com', '0102-161-7181', 'Tanta'),
    ('abdelrahman', 'M', NULL, 'Abdelrahmanmostafa3@gmail.com', '0121-987-6543', 'Madinaty'),
    ('Mohammed', NULL, 'Khallaf', 'mokhallaf34@gmail.com', '0101-456-7890', 'Shobra'),
    ('mostafa', 'L', 'Fouad', 'mostafaFouad@gmail.com', '0111-789-0123', 'Nasr City'),
    ('Anas', 'R', 'Mansour', 'Anasmansour1@gmail.com', '0155-234-5678', 'Nasr city');

-- Insert Restaurants
INSERT INTO Restaurant (Name, Phone_Number, Opening_Hours)
VALUES 
    ('Dominas Pizza', '0100-111-2222', '10:00-22:00'),
    ('Buffulo Burger', '0122-333-4444', '11:00-23:00'),
    ('El-Dahan', '0111-555-6666', '12:00-22:00'),
    ('El-shabrawy', '0155-777-8888', '10:00-21:00'),
    ('Pasta Paradise', '0115-999-0000', '11:00-22:00');

-- Insert Restaurant Addresses
INSERT INTO Restaurant_Address (RestaurantID, Address)
VALUES 
    (1, '100 Mostafa El-Nahas, Nasr City'),
    (2, '200 El-bahr, Tanta'),
    (3, '300 kolyt elbanat, New Cairo'),
    (4, '400 Abdo Basha'),
    (5, 'madinaty');

-- Insert Menus
INSERT INTO Menu (RestaurantID, category, description)
VALUES 
    (1, 'Pizza', 'Various pizzas with different toppings'),
    (1, 'Sides', 'Side dishes to complement your pizza'),
    (2, 'Burgers', 'Gourmet burgers with various toppings'),
    (2, 'Fries', 'Different types of fries'),
    (3, 'Mashawy', 'Traditional Egyptian Mashawy'),
    (3, 'Soup', 'Traditional Soup'),
    (4, 'Flafel', 'Flafel with salad'),
    (4, 'Foul', 'Delicious cooked fava beans'),
    (5, 'Pasta', 'Various pasta dishes with different sauces');

-- Insert Menu Items
INSERT INTO menu_item (RestaurantID, Description, Price, category, menuID, order_id)
VALUES 
    (1, 'Margherita Pizza', 199.99, 'Pizza', 1, NULL),
    (1, 'Pepperoni Pizza', 250.50, 'Pizza', 1, NULL),
    (1, 'Garlic Breadsticks', 69.99, 'Sides', 2, NULL),
    (2, 'Cheeseburger', 100.00, 'Burgers', 3, NULL),
    (2, 'Bacon Burger', 130.50, 'Burgers', 3, NULL),
    (2, 'Sweet Potato Fries', 50.00, 'Fries', 4, NULL),
    (3, 'Tarb', 700.00, 'Mashawy', 5, NULL),
    (3, 'Molokhia', 40.00, 'Soup', 6, NULL),
    (4, 'Flafel mahshya', 10.00, 'Flafel', 7, NULL),
    (4, 'Foul Salta', 9.99, 'Foul', 8, NULL),
    (5, 'Spaghetti Carbonara', 199.99, 'Pasta', 9, NULL),
    (5, 'Fettuccine Alfredo', 250.00, 'Pasta', 9, NULL);

-- Insert Payments
INSERT INTO payment (RestaurantID, PaymentMethod, PaymentStatus, Total_Price)
VALUES 
    (1, 'Credit Card', 'Completed', 469.98),
    (2, 'Cash', 'Completed', 150.00),
    (3, 'Debit Card', 'Completed', 740.00),
    (4, 'Credit Card', 'Pending', 29.99),
    (5, 'Mobile Payment', 'Completed', 449.99);

-- Insert Deliverymen
INSERT INTO Deliveryman (Name, PhoneNumber, VehicleType)
VALUES 
    ('Ahmed Samir', '0100-111-3333', 'Motorcycle'),
    ('Mahmoud Hassan', '0122-222-4444', 'Motorcycle'),
    ('Khaled Adel', '0111-333-5555', 'Bicycle'),
    ('Omar Tarek', '0155-444-6666', 'Car'),
    ('Youssef Reda', '0155-555-7777', 'Motorcycle');

-- Insert Orders
INSERT INTO [order] (CustomerID, RestaurantID, OrderDate, Status, CustomerAddress, PaymentID)
VALUES 
    (1, 1, '2024-04-15', 'Delivered', 'Tanta', 1),
    (2, 2, '2024-04-16', 'Delivered', 'Madinaty', 2),
    (3, 3, '2024-04-17', 'Delivered', 'Shobra', 3),
    (4, 4, '2024-04-18', 'In Transit', 'Nasr City', 4),
    (5, 5, '2024-04-19', 'Delivered', 'Nasr city', 5);

-- Update menu_item with order_id
UPDATE menu_item SET order_id = 1 WHERE ItemID IN (1, 3);
UPDATE menu_item SET order_id = 2 WHERE ItemID IN (4, 6);
UPDATE menu_item SET order_id = 3 WHERE ItemID IN (7, 8);
UPDATE menu_item SET order_id = 4 WHERE ItemID IN (9, 10);
UPDATE menu_item SET order_id = 5 WHERE ItemID IN (11, 12);

-- Insert Deliveries
INSERT INTO delivery (OrderID, DeliverManID, DeliveryAddress, DeliveryStatus)
VALUES 
    (1, 1, 'Tanta', 'Delivered'),
    (2, 2, 'Madinaty', 'Delivered'),
    (3, 3, 'Shobra', 'Delivered'),
    (4, 4, 'Nasr City', 'In Transit'),
    (5, 5, 'Nasr city', 'Delivered');

-- Insert Order Items
INSERT INTO Consists_of (OrderID, ItemID, Quantity)
VALUES 
    (1, 1, 2),
    (1, 3, 1),
    (2, 4, 1),
    (2, 6, 1),
    (3, 7, 1),
    (3, 8, 1),
    (4, 9, 2),
    (4, 10, 1),
    (5, 11, 1),
    (5, 12, 1);

-- Insert Reviews
INSERT INTO Review (CustomerID, RestaurantID, Rate)
VALUES 
    (1, 1, 5),
    (2, 2, 4),
    (3, 3, 5),
    (4, 4, 3),
    (5, 5, 4);

-- Final SELECT statements for all tables
SELECT 'Customer Table' AS TableName, * FROM Customer;
SELECT 'Restaurant Table' AS TableName, * FROM Restaurant;
SELECT 'Restaurant_Address Table' AS TableName, * FROM Restaurant_Address;
SELECT 'Menu Table' AS TableName, * FROM Menu;
SELECT 'Menu_Item Table' AS TableName, * FROM menu_item;
SELECT 'Payment Table' AS TableName, * FROM payment;
SELECT 'Deliveryman Table' AS TableName, * FROM Deliveryman;
SELECT 'Order Table' AS TableName, * FROM [order];
SELECT 'Delivery Table' AS TableName, * FROM delivery;
SELECT 'Consists_of Table' AS TableName, * FROM Consists_of;
SELECT 'Review Table' AS TableName, * FROM Review;


-- 1. List all orders with customer and restaurant names
SELECT o.OrderID, c.Fname + ' ' + ISNULL(c.Lname, '') AS CustomerName,
       r.Name AS RestaurantName, o.OrderDate, o.Status
FROM [order] o
JOIN Customer c ON o.CustomerID = c.CustomerID
JOIN Restaurant r ON o.RestaurantID = r.RestaurantID;

-- 2. Total amount spent by each customer (requires GROUP BY)
SELECT c.CustomerID, c.Fname, c.Lname, SUM(p.Total_Price) AS TotalSpent
FROM Customer c
JOIN [order] o ON c.CustomerID = o.CustomerID
JOIN payment p ON o.PaymentID = p.PaymentID
GROUP BY c.CustomerID, c.Fname, c.Lname;

-- 3. Most ordered item (by quantity)
SELECT mi.Description, SUM(co.Quantity) AS TotalOrdered
FROM menu_item mi
JOIN Consists_of co ON mi.ItemID = co.ItemID
GROUP BY mi.Description
ORDER BY TotalOrdered DESC;

-- 4. Restaurants with average rating
SELECT r.Name AS Restaurant, AVG(rw.Rate) AS AverageRating
FROM Review rw
JOIN Restaurant r ON rw.RestaurantID = r.RestaurantID
GROUP BY r.Name
ORDER BY AverageRating DESC;

-- 5. Delivery status with deliveryman name
SELECT d.DeliveryID, o.OrderID, dm.Name AS Deliveryman, d.DeliveryStatus
FROM delivery d
JOIN [order] o ON d.OrderID = o.OrderID
JOIN Deliveryman dm ON d.DeliverManID = dm.DeliverManID;

-- Add default status and date to orders
ALTER TABLE [order]
ADD CONSTRAINT DF_Order_Status DEFAULT 'Pending' FOR Status;

ALTER TABLE [order]
ADD CONSTRAINT DF_Order_OrderDate DEFAULT GETDATE() FOR OrderDate;

-- Ensure price is positive
ALTER TABLE menu_item
ADD CONSTRAINT CHK_MenuItem_PositivePrice CHECK (Price > 0);

DROP FUNCTION IF EXISTS dbo.GetOrderCountForCustomer;
DROP FUNCTION IF EXISTS dbo.GetTotalSpentByCustomer;
DROP FUNCTION IF EXISTS dbo.GetAverageRatingForRestaurant;


-- ========================================
-- FUNCTION 1: Get number of orders for a customer
-- ========================================
GO
CREATE FUNCTION dbo.GetOrderCountForCustomer (@CustomerID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    SELECT @Count = COUNT(*) 
    FROM [order]
    WHERE CustomerID = @CustomerID;
    RETURN @Count;
END;
GO

-- ========================================
-- FUNCTION 2: Get total amount spent by a customer
-- ========================================
GO
CREATE FUNCTION dbo.GetTotalSpentByCustomer (@CustomerID INT)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Total DECIMAL(10,2);
    SELECT @Total = SUM(p.Total_Price)
    FROM [order] o
    JOIN payment p ON o.PaymentID = p.PaymentID
    WHERE o.CustomerID = @CustomerID;
    RETURN ISNULL(@Total, 0.00);
END;
GO

-- ========================================
-- FUNCTION 3: Get average rating for a restaurant
-- ========================================
GO
CREATE FUNCTION dbo.GetAverageRatingForRestaurant (@RestaurantID INT)
RETURNS FLOAT
AS
BEGIN
    DECLARE @Avg FLOAT;
    SELECT @Avg = AVG(CAST(Rate AS FLOAT))
    FROM Review
    WHERE RestaurantID = @RestaurantID;
    RETURN @Avg;
END;
GO


-- Example: Total orders for customer with ID 1
SELECT dbo.GetOrderCountForCustomer(1) AS OrderCount;

-- Example: Total spent by customer with ID 2
SELECT dbo.GetTotalSpentByCustomer(2) AS TotalSpent;

-- Example: Average rating for restaurant with ID 3
SELECT dbo.GetAverageRatingForRestaurant(3) AS AvgRating;
