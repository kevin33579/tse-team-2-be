CREATE TABLE ProductType (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL ,
    description TEXT NULL,
    
    createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) 

INSERT INTO ProductType (name, description)
VALUES
  ('SUV', 'Sport Utility Vehicle, designed for both on-road and off-road use'),
  ('LCGC', 'Low Cost Green Car, fuel-efficient and affordable vehicle'),
  ('Sedan', 'Passenger car with a three-box configuration'),
  ('Truck', 'Motor vehicle designed to transport cargo');
