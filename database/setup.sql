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
    createdDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    lastLoginDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    isActive BOOLEAN DEFAULT True,
    FOREIGN KEY (roleId) REFERENCES roles(id)
);

INSERT INTO users (username, email, `password`, roleId) VALUES
('admin', 'admin@example.com', '$2a$11$GQkU36Xltn9I6WRJaBkzEuV/p1sVCX8bTuBeYmK2DbQ60mr9b9o1G', 1), -- password admin123
('john_doe', 'john@example.com', '$2a$11$xiyq54W4oc7bEC9/cWQKLOI1LyRzLJhQ5rDCyZkN6kM9D88M6RAY2', 2); -- password password123


-- =====================
-- TABEL: productType
-- =====================
CREATE TABLE productType (
    id INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    `description` TEXT
);

ALTER TABLE productType         
ADD COLUMN imageUrl TEXT ; 



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

-- SUV
UPDATE productType
SET imageUrl = 'https://carsgallery.co.id/blog/wp-content/uploads/2024/11/contoh-mobil-mpv-dan-suv-3.jpg' where id = 1;


UPDATE productType
SET imageUrl = 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQV5ELO-WDCwibgDdewXKgtrNCN2BdvI3MmzQ&s' where id = 2;


UPDATE productType
SET imageUrl = 'https://s3.ap-southeast-1.amazonaws.com/img.jba.co.id//wysiwyg/ckeditor/20241201221025cover372D4LCX.webp' where id = 3;


UPDATE productType
SET imageUrl = 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQYD5UL1GXpvNSYNEcIHC5--qpwVN6MolRg8A&s' where id = 4;

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
('Toyota Avanza', 600000, 50, 'LCGC murah dan irit', 'https://medias.auto2000.co.id/sys-master-hybrismedia/h47/h72/8831557730334/avanza-g-puplish_optimized.png', 2),
('Honda Brio', 300000, 50, 'Course Honda Brio', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRaZwfpwxblQIHqRAerXMqA9Dfi36spg3EOVQ&s', 2),
('Sedan Honda Civic', 400000, 50, 'Course Sedan Civic', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSE16WKNdc2PArY9ZSP8_LMCPRs2BTOIAa19Q&s', 3),
('Dump Truck For Mining ', 1200000, 50, 'Course Dump Truck For Mining', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSzW_itCRPNS_jO451fKKBHbBYZ6_XAv4e1KA&s', 4),
('Pajero Sport ', 800000, 50, 'Course Pajero Sport', 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAkGBxATEhURExMWFhIWEhgVGRIYGBUZGBUYFRYWFxUWFxgYHCggGBsmHRcZJDEiJikrLi4uFx8zODMsNygwLisBCgoKDQ0NFQ0PFTMdHxk3NzcrKy8rNystKzc4MysrKzcrKy0tKzItKyswMCs3Ky0rKysrNys3KyswKzItNzgyK//AABEIAOEA4QMBIgACEQEDEQH/xAAcAAEAAQUBAQAAAAAAAAAAAAAABgIDBAUHAQj/xABKEAACAgEBBQMHCAQLCAMAAAABAgADEQQFEiExQQYTUQciMmFxgZEUQoKSobHB0SNSYnIVFzNDU1STssLS8AgWc4OU0+HiRFWi/8QAFwEBAAMAAAAAAAAAAAAAAAAAAAECA//EABsRAQABBQEAAAAAAAAAAAAAAAAhAQMREiMC/9oADAMBAAIRAxEAPwDuMREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERAREQEREBERARPGYAZJwPGarU9pdGh3Tcpb9VfOPwWBtokebtnoh85vqN+MoPbjQ9XYfRP4QJJEiz9v8AZ4+ex+gfxlhvKPoB/SH6K/i0CYRIZ/GVouQW0n2J/nmZqO2dKJvshT1WMi/DicwJPE55b5UaRyWv69h/u1ESlfKnT+on1rv+zA6LEgdPlN0x5qPc5/xos2Gn8oGibqR76j9iuT9kCWRNPR2o0TfzoHrZXUfFgBNjptbVZ/J2I/7rK33GBfiIgIiICIiAiIgIiICIiAiJjbQ11VFbW2uErUZLH4ADqSTwAHEk4EDJka1fanfJTSItuDhtQzbunQjmAwBNrDwQEZBBZTIn2h7SHU5Fua9J002cPcPHUEckP9EOnpE53Ro9T2gNmFrPmjHBeCIo+b7cdBAk23bXvwLdSTgcUqXdQnqcNn7SZEtoLp6RxssAPrUZx+7ie3a9jynOO1e1GsvZQ3mp5g9o9I/Hh9GEJHrNsaQdWPtZ/wADNcNpJY25Um8xzgbzDOOJ9IiaOmhFq75hvEo/mnkPOStDw453mJ9izXU6lkIZWKsOTDnAke0LtRWN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', 1);


INSERT INTO product (`name`, price, stock, `description`,imageUrl ,productTypeId) VALUES
('Honda Odyssey', 900000, 100, 'Course SUV Honda Odyssey','https://media.zcreators.id/crop/photo/p2/93/2024/09/15/2021-honda-odyssey-japan-refresh-lead-2254512288.jpg' ,6);

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


ALTER TABLE paymentMethod         
ADD COLUMN imageUrl TEXT ; 


INSERT INTO paymentMethod (`name`,`imageUrl`) VALUES
('Gopay','https://static.vecteezy.com/system/resources/thumbnails/028/766/371/small_2x/gopay-payment-icon-symbol-free-png.png'),
('Ovo','https://play-lh.googleusercontent.com/-kwEfsDenlwTCoWTe2BCAOv9YFPE4m5EReErdU_BsYYcISAtQ16JflXuwU8Okuw3Y6E'),
('Dana','https://1000logos.net/wp-content/uploads/2021/03/Dana-logo.jpg'),
('Mandiri','https://upload.wikimedia.org/wikipedia/commons/thumb/a/ad/Bank_Mandiri_logo_2016.svg/2560px-Bank_Mandiri_logo_2016.svg.png'),
('BCA','https://www.svgrepo.com/show/303676/bca-bank-central-asia-logo.svg'),
('BNI','https://images.seeklogo.com/logo-png/35/1/bank-bni-logo-png_seeklogo-355606.png')
;

-- =====================
-- TABEL: invoice
-- =====================
CREATE TABLE invoice (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    user_id         INT NOT NULL,
    invoiceCode     VARCHAR(100) NOT NULL,
    `date`          DATETIME DEFAULT CURRENT_TIMESTAMP,
    totalPrice      DECIMAL(10,2),
    totalCourse     INT,
    paymentMethodId INT,
    CONSTRAINT fk_invoice_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE
) ENGINE = InnoDB;



INSERT INTO invoice
        (user_id, invoiceCode, `date`, totalPrice, totalCourse, paymentMethodId)
VALUES  (1, 'INV-20250701-0001', NOW(), 1500000.00, 3, 2);




-- =====================
-- TABEL: detailInvoice
-- =====================
CREATE TABLE detail_invoice (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    invoice_id  INT NOT NULL,
    product_id  INT NOT NULL,
    schedule_id INT NULL,

    INDEX idx_invoice  (invoice_id),
    INDEX idx_product  (product_id),
    INDEX idx_schedule (schedule_id)   
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4;


ALTER TABLE detail_invoice
  ADD CONSTRAINT fk_detail_invoice_invoice
      FOREIGN KEY (invoice_id) REFERENCES invoice(id);

ALTER TABLE detail_invoice
  ADD CONSTRAINT fk_detail_invoice_product
      FOREIGN KEY (product_id) REFERENCES product(id);

ALTER TABLE detail_invoice
  ADD CONSTRAINT fk_detail_invoice_schedule
      FOREIGN KEY (schedule_id) REFERENCES schedule(id);




INSERT INTO detail_invoice (invoice_id, product_id,schedule_id)
VALUES (1, 1,1);

CREATE TABLE product_schedule (
    id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    schedule_id INT NOT NULL,
    FOREIGN KEY (product_id) REFERENCES product(id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (schedule_id) REFERENCES schedule(id) ON DELETE CASCADE ON UPDATE CASCADE
);

INSERT INTO product_schedule (product_id, schedule_id) VALUES
(1, 1),
(1, 2),
(3, 1);




