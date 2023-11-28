CREATE DATABASE PSSC
GO
USE PSSC
GO

CREATE TABLE Users (
    user_id int PRIMARY KEY,
    firstname VARCHAR(255) not null,
    lastname VARCHAR(255) not null
);

CREATE TABLE Card_details (
    card_id int PRIMARY KEY,
    user_id int UNIQUE REFERENCES Users(user_id),
    card_number VARCHAR(255) not null,
    cvv int not null,
    card_expiry_date DATE not null,
    balance FLOAT not null
);

CREATE TABLE Products (
    product_id int PRIMARY KEY,
    product_name VARCHAR(255) not null,
    quantity int not null,
    price FLOAT not null
);

CREATE TABLE Orders (
    order_id int PRIMARY KEY,
    user_id int UNIQUE REFERENCES Users(user_id),
    card_id int UNIQUE REFERENCES Card_details(card_id),
    total_price FLOAT not null,
    delivery_address VARCHAR(255) not null,
    postal_code VARCHAR(255) not null,
    telephone VARCHAR(255) not null,
    order_status VARCHAR(255) not null
);

CREATE TABLE Order_details(
    order_detail_id int PRIMARY KEY,
    order_id int REFERENCES Orders(order_id),
    product_id int REFERENCES Products(product_id),
    quantity INT not null
);


