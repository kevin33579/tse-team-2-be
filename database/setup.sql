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
CREATE TABLE product_type (
    id INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    `description` TEXT
);

ALTER TABLE productType         
ADD COLUMN imageUrl TEXT ;   

-- SUV
UPDATE productType
SET imageUrl = 'https://carsgallery.co.id/blog/wp-content/uploads/2024/11/contoh-mobil-mpv-dan-suv-3.jpg' where id = 1;


UPDATE productType
SET imageUrl = 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQV5ELO-WDCwibgDdewXKgtrNCN2BdvI3MmzQ&s' where id = 2;


UPDATE productType
SET imageUrl = 'https://s3.ap-southeast-1.amazonaws.com/img.jba.co.id//wysiwyg/ckeditor/20241201221025cover372D4LCX.webp' where id = 3;


UPDATE productType
SET imageUrl = 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQYD5UL1GXpvNSYNEcIHC5--qpwVN6MolRg8A&s' where id = 4;


--select

select * from productType
select * from product


INSERT INTO productType (name, description)
VALUES
  ('SUV', 'Sport Utility Vehicle, designed for both on-road and off-road use'),
  ('LCGC', 'Low Cost Green Car, fuel-efficient and affordable vehicle'),
  ('Sedan', 'Passenger car with a three-box configuration'),
  ('Truck', 'Motor vehicle designed to transport cargo');

  INSERT INTO productType (name, description, imageUrl )
VALUES
  ('Hatchback', 'a car body style that typically features a rear door that opens upwards, combining the passenger and cargo areas into a single compartment','https://hips.hearstapps.com/hmg-prod/images/2023-lightning-lap-volkswagen-golf-gti-mu-105-1675446169.jpg?crop=0.629xw:0.630xh;0.121xw,0.199xh&resize=980:*'),
  ('MPV', 'a car designed to maximize passenger capacity and comfort, making it ideal for families or groups','https://www.gardaoto.com/wp-content/uploads/2023/04/Screenshot-2023-04-27-105141.png'),
  ('Electric', 'a vehicle that is propelled by one or more electric motors, powered by rechargeable batteries','https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRa0lzeM2DGdIdz5kokeBShM9j25l0znUVpiw&s'),
  ('Offroad', 'a motorized vehicle designed to travel over rough, uneven, or unpaved terrain, including dirt, gravel, mud, snow, and other challenging surfaces','https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTPwAPvIkvJqNXBNZhaB9CoNZNZ8Lb0Nehi9Q&s');

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

ALTER TABLE product          
ADD COLUMN imageUrl TEXT ;   


INSERT INTO product (`name`, price, stock, `description`,imageUrl ,productTypeId) VALUES
('Kijang Innova', 700000, 100, 'Course SUV Kijang Innova','https://imgcdn.oto.com/large/gallery/exterior/38/1240/toyota-kijang-innova-front-angle-low-view-351782.jpg' ,1),
('Toyota Avanza', 600000, 50, 'LCGC murah dan irit', 'https://medias.auto2000.co.id/sys-master-hybrismedia/h47/h72/8831557730334/avanza-g-puplish_optimized.png', 2),
('Honda Brio', 300000, 50, 'Course Honda Brio', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRaZwfpwxblQIHqRAerXMqA9Dfi36spg3EOVQ&s', 2),
('Sedan Honda Civic', 400000, 50, 'Course Sedan Civic', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSE16WKNdc2PArY9ZSP8_LMCPRs2BTOIAa19Q&s', 3),
('Dump Truck For Mining ', 1200000, 50, 'Course Dump Truck For Mining', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSzW_itCRPNS_jO451fKKBHbBYZ6_XAv4e1KA&s', 4);


-- =====================
-- TABEL: schedule
-- =====================
CREATE TABLE schedule (
    id INT AUTO_INCREMENT PRIMARY KEY,
    time DATETIME NOT NULL
);

INSERT INTO schedule (time) VALUES
('2025-07-01 10:00:00'),
('2025-07-02 14:00:00');

-- =====================
-- TABEL: cart
-- =====================
CREATE TABLE `carts` (
    `id`          INT NOT NULL AUTO_INCREMENT,
    `user_id`     INT NOT NULL,
    `product_id`  INT NOT NULL,
    `schedule_id` INT NULL,
    `quantity`    INT NOT NULL DEFAULT 1,

    PRIMARY KEY (`id`),

    CONSTRAINT `fk_carts_users`
        FOREIGN KEY (`user_id`)    REFERENCES `users` (`id`)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT `fk_carts_products`
        FOREIGN KEY (`product_id`) REFERENCES `product` (`id`)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    CONSTRAINT `fk_carts_schedules`
        FOREIGN KEY (`schedule_id`) REFERENCES `schedule` (`id`)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

INSERT INTO carts (user_id, product_id, schedule_id, quantity)
VALUES
  (1, 1, 1, 2);

-- =====================
-- TABEL: Orders
-- =====================
CREATE TABLE orders (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT,
    cart_id INT,
    order_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    total_amount DECIMAL(10,2),
    status ENUM('PENDING', 'PAID', 'CANCELLED') DEFAULT 'PENDING',
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (cart_id) REFERENCES carts(id)
);



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
