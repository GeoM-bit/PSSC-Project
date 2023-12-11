CREATE DATABASE PSSC
GO
USE PSSC
GO

CREATE TABLE Users (
    userId int IDENTITY(1,1) PRIMARY KEY,
    firstname VARCHAR(255) not null,
    lastname VARCHAR(255) not null,
    cardNumber VARCHAR(255),
    cvv int,
    cardExpiryDate DATE,
    balance FLOAT 
);

CREATE TABLE Products (
    productId int IDENTITY(1,1) PRIMARY KEY,
    productName VARCHAR(255) not null,
    quantity int not null,
    price FLOAT not null
);

CREATE TABLE Orders (
    orderId int IDENTITY(1,1) PRIMARY KEY,
    userId int REFERENCES Users(userId),
    totalPrice FLOAT not null,
    deliveryAddress VARCHAR(255) not null,
    postalCode VARCHAR(255) not null,
    telephone VARCHAR(255) not null,
    orderStatus VARCHAR(255) not null
);

CREATE TABLE OrderDetails(
    orderDetailId int IDENTITY(1,1) PRIMARY KEY,
    orderId int REFERENCES Orders(orderId),
    productId int REFERENCES Products(productId),
    quantity INT not null
);



