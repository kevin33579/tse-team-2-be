-- Buat database
CREATE DATABASE IF NOT EXISTS Otomobil_DB;
USE Otomobil_DB;

-- =====================
-- TABEL: roles
-- =====================
CREATE TABLE roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) NOT NULL
);

INSERT INTO roles (name) VALUES
('Admin'),
('User');

-- =====================
-- TABEL: users
-- =====================
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL,
    `password` VARCHAR(100) NOT NULL,
    roleId INT,
    createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (roleId) REFERENCES roles(id)
);

INSERT INTO users (username, email, `password`, roleId) VALUES
('admin', 'admin@example.com', 'admin123', 1),
('john_doe', 'john@example.com', 'password123', 2);

-- =====================
-- TABEL: productType
-- =====================
CREATE TABLE productType (
    id INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    `description` TEXT
);

INSERT INTO productType (name, description)
VALUES
  ('SUV', 'Sport Utility Vehicle, designed for both on-road and off-road use'),
  ('LCGC', 'Low Cost Green Car, fuel-efficient and affordable vehicle'),
  ('Sedan', 'Passenger car with a three-box configuration'),
  ('Truck', 'Motor vehicle designed to transport cargo');

-- =====================
-- TABEL: product
-- =====================
CREATE TABLE product (
    id INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    stock INT NOT NULL,
    `description` TEXT,
    imageUrl TEXT,
    productTypeId INT,
    FOREIGN KEY (productTypeId) REFERENCES productType(id)
);

INSERT INTO product (`name`, price, stock, `description`,imageUrl ,productTypeId) VALUES
('Kijang Innova', 700000, 100, 'Course SUV Kijang Innova','https://imgcdn.oto.com/large/gallery/exterior/38/1240/toyota-kijang-innova-front-angle-low-view-351782.jpg' ,1),
('Toyota Avanza', 600000, 50, 'LCGC murah dan irit', 'https://medias.auto2000.co.id/sys-master-hybrismedia/h47/h72/8831557730334/avanza-g-puplish_optimized.png', 2);;


-- =====================
-- TABEL: schedule
-- =====================
CREATE TABLE schedule (
    id INT AUTO_INCREMENT PRIMARY KEY,
    time DATETIME NOT NULL,
    productId INT,
    FOREIGN KEY (productId) REFERENCES product(id)
);

INSERT INTO schedule (time, productId) VALUES
('2025-07-01 10:00:00', 1),
('2025-07-02 14:00:00', 2);

-- =====================
-- TABEL: cart
-- =====================
CREATE TABLE cart (
    id INT AUTO_INCREMENT PRIMARY KEY,
    productId INT,
    userId INT,
    scheduleId INT,
    quantity INT,
    FOREIGN KEY (productId) REFERENCES product(id),
    FOREIGN KEY (userId) REFERENCES users(id),
    FOREIGN KEY (scheduleId) REFERENCES schedule(id)
);

INSERT INTO cart (productId, userId, scheduleId, quantity) VALUES
(1, 2, 1, 1),
(2, 2, 2, 2);

-- =====================
-- TABEL: paymentMethod
-- =====================
CREATE TABLE paymentMethod (
    id INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL
);

INSERT INTO paymentMethod (`name`) VALUES
('Bank Transfer'),
('Credit Card'),
('Cash');

-- =====================
-- TABEL: invoice
-- =====================
CREATE TABLE invoice (
    id INT AUTO_INCREMENT PRIMARY KEY,
    invoiceCode VARCHAR(100) NOT NULL,
    `date` DATETIME DEFAULT CURRENT_TIMESTAMP,
    totalPrice DECIMAL(10,2),
    totalCourse INT,
    paymentMethodId INT
);

INSERT INTO invoice (invoiceCode, totalPrice, totalCourse, paymentMethodId) VALUES
('INV-001', 850000, 2, 1);



-- =====================
-- TABEL: detailInvoice
-- =====================
CREATE TABLE detailInvoice (
    id INT AUTO_INCREMENT PRIMARY KEY,
    invoiceId INT,
    productId INT,
    scheduleId INT,
    FOREIGN KEY (invoiceId) REFERENCES invoice(id),
    FOREIGN KEY (productId) REFERENCES product(id),
    FOREIGN KEY (scheduleId) REFERENCES schedule(id)
);

INSERT INTO detailInvoice (invoiceId, productId, scheduleId) VALUES
(1, 1, 1),
(1, 2, 2);
