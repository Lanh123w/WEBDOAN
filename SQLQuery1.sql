create database WebDoAn

CREATE LOGIN WebDoAn WITH PASSWORD = 'StrongPassword123!';
CREATE USER WebDoAn FOR LOGIN WebDoAn;
ALTER ROLE db_owner ADD MEMBER WebDoAn;

-- Bảng người dùng
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(256),
    NormalizedUserName NVARCHAR(256),
    Email NVARCHAR(256),
    NormalizedEmail NVARCHAR(256),
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(MAX),
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL
);

-- Bảng vai trò
CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256),
    NormalizedName NVARCHAR(256),
    ConcurrencyStamp NVARCHAR(MAX)
);

-- Bảng người dùng - vai trò
CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
);

-- Bảng đăng nhập ngoài (Google, Facebook, v.v.)
CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(128) NOT NULL,
    ProviderKey NVARCHAR(128) NOT NULL,
    ProviderDisplayName NVARCHAR(128),
    UserId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

-- Bảng xác nhận token (email, mật khẩu, v.v.)
CREATE TABLE AspNetUserTokens (
    UserId NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL,
    Name NVARCHAR(128) NOT NULL,
    Value NVARCHAR(MAX),
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

-- Bảng xác nhận quyền truy cập
CREATE TABLE AspNetRoleClaims (
    Id INT NOT NULL IDENTITY PRIMARY KEY,
    RoleId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX),
    ClaimValue NVARCHAR(MAX),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
);

-- Bảng xác nhận quyền người dùng
CREATE TABLE AspNetUserClaims (
    Id INT NOT NULL IDENTITY PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX),
    ClaimValue NVARCHAR(MAX),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

-- Tạo bảng Category
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

-- Tạo bảng FoodItems có khóa ngoại đến Category
CREATE TABLE FoodItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL CHECK (Price >= 0 AND Price <= 100000),
    ImageUrl NVARCHAR(255),
    CategoryId INT NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
-- Bảng Orders
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderDate DATETIME NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255)
);

-- Bảng OrderDetails
CREATE TABLE OrderDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    FoodItemId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),

    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (FoodItemId) REFERENCES FoodItems(Id)
);



INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('aaf01832-63ff-42cf-b183-98e5040b714d', '<RoleId của Admin>');

INSERT INTO Categories (Name) VALUES
('Món chính'),
('Món phụ'),
('Đồ uống'),
('Tráng miệng');

INSERT INTO FoodItems( Name, Description, Price, CategoryId, ImageUrl) VALUES
('Cơm gà xối mỡ', 'Cơm gà giòn rụm, thơm ngon', 45000, 1, '/images/com-ga.jpg'),
('Bún bò Huế', 'Đậm đà hương vị miền Trung', 50000, 1, '/images/bun-bo.jpg'),
('Trà đào cam sả', 'Thức uống mát lạnh', 25000, 3, '/images/tra-dao.jpg'),
('Bánh flan', 'Món tráng miệng mềm mịn', 20000, 4, '/images/flan.jpg');


CREATE TABLE CartItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    FoodItemId INT NOT NULL,
    Quantity INT NOT NULL,

    CONSTRAINT FK_CartItems_FoodItems FOREIGN KEY (FoodItemId)
        REFERENCES FoodItems(Id),

    CONSTRAINT FK_CartItems_Users FOREIGN KEY (UserId)
        REFERENCES AspNetUsers(Id)
);
INSERT INTO FoodItems (Name, Description, Price, ImageUrl, CategoryId)
VALUES
-- Đồ uống
('7Up', 'Nước giải khát có gas vị chanh', 12000, '/images/7up.jpg', 1),
('Coca Cola', 'Nước ngọt có gas phổ biến', 12000, '/images/cocacola.jpg', 1),
('Pepsi', 'Nước ngọt có gas vị cola', 12000, '/images/pepsi.jpeg', 1),
('Revive', 'Nước bù khoáng cho người vận động', 15000, '/images/revive.jpg', 1),
('Sting Dâu', 'Nước tăng lực vị dâu', 12000, '/images/stingdau.jpg', 1),
('TH Milk', 'Sữa tươi tiệt trùng', 10000, '/images/thmilk.jpg', 1),
('Trà Sữa Trân Châu', 'Trà sữa thơm béo kèm trân châu', 25000, '/images/imagestrasuatranchau.jpeg', 1),
('Nước Ép Cam', 'Nước ép cam nguyên chất', 20000, '/images/nuocepcam.jpg', 1),
('Trà Chanh Mật Ong', 'Trà chanh kết hợp mật ong', 18000, '/images/trachanhmatong.jpg', 1),
('Trà Đào', 'Trà đào thơm mát', 18000, '/images/tradao.jpg', 1),

-- Món ăn chính
('Bánh Bèo', 'Bánh bèo Huế mềm mịn, topping tôm cháy', 25000, '/images/banhbeo.jpg', 2),
('Bánh Cuốn', 'Bánh cuốn nhân thịt, ăn kèm chả', 30000, '/images/banhcuon.jpg', 2),
('Bánh Khọt', 'Bánh khọt giòn rụm, ăn kèm rau sống', 35000, '/images/banhkhot.jpg', 2),
('Bánh Mì', 'Bánh mì Việt Nam nhân thịt, chả, rau', 20000, '/images/banhmi.jpg', 2),
('Bánh Xèo', 'Bánh xèo vàng giòn, nhân tôm thịt', 35000, '/images/banhxeo.jpg', 2),
('Bún Bò Sốt Xoạt', 'Bún bò cay đậm đà kiểu Huế', 40000, '/images/bunbosoaxoat.jpg', 2),
('Bún Đậu Mắm Tôm', 'Bún đậu giòn, chả cốm, mắm tôm', 35000, '/images/bundaumamtom.jpg', 2),
('Chả Giò', 'Chả giò chiên giòn, nhân thịt rau củ', 30000, '/images/chagio.jpg', 2),
('Cơm Chiên Hải Sản', 'Cơm chiên thơm ngon với tôm, mực', 45000, '/images/comchienhaisan.jpg', 2),
('Cơm Tấm', 'Cơm tấm sườn bì chả đặc trưng Sài Gòn', 40000, '/images/comtam.jpeg', 2),
('Cơm Thập Cẩm', 'Cơm thập cẩm đủ món: thịt, rau, trứng', 45000, '/images/comthapcam.jpeg', 2),
('Gỏi Cuốn', 'Gỏi cuốn tôm thịt, chấm nước mắm chua ngọt', 25000, '/images/goicuon.jpg', 2),
('Hủ Tiếu', 'Hủ tiếu Nam Vang thơm ngon', 40000, '/images/hutieu.jpg', 2),
('Phở', 'Phở bò truyền thống', 45000, '/images/pho.jpeg', 2),
('Phở Gà', 'Phở gà thơm mềm', 40000, '/images/phoga.jpeg', 2),
('Phở Tái', 'Phở bò tái mềm ngọt', 45000, '/images/photai.jpeg', 2),
('Sườn Bì', 'Cơm sườn bì chả', 40000, '/images/suonbi.jpeg', 2);
INSERT INTO Categories (Name)
VALUES ('Đồ uống'), ('Món ăn chính');

ALTER TABLE Orders
ADD Status NVARCHAR(50),
    TotalAmount DECIMAL(18, 2);
	ALTER TABLE Orders ADD Phone NVARCHAR(50);
ALTER TABLE Orders ADD UserId NVARCHAR(450);
INSERT INTO AspNetRoles (Id, Name, NormalizedName)
VALUES ('admin-role-id', 'Admin', 'ADMIN');
SELECT Id FROM AspNetUsers WHERE Email = 'lanhlanhmai@gmail.com';

SELECT Id FROM AspNetRoles WHERE Name = 'Admin';
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('aaf01832-63ff-42cf-b183-98e5040b714d', 'admin-role-id');
SELECT * FROM AspNetUserRoles WHERE UserId = 'f3a1b2c3-d4e5-6789-abcd-1234567890ab';
CREATE TABLE DiscountCode (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Amount DECIMAL(18,2) NOT NULL, -- Số tiền giảm
    IsActive BIT NOT NULL DEFAULT 1,
    ExpiryDate DATETIME NOT NULL
);

ALTER TABLE Orders
ADD DiscountCode NVARCHAR(50) NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
	INSERT INTO DiscountCode (Code, Amount, IsActive, ExpiryDate)
VALUES 
('FOODLOVE15', 15000, 1, '2025-12-31'),
('EATFRESH5', 5000, 1, '2025-12-31'),
('YUMMY25K', 25000, 1, '2025-12-31'),
('SHIPFREE', 10000, 1, '2025-12-31'),
('HELLOFOOD', 12000, 1, '2025-12-31');
ALTER TABLE Orders
ADD 
    OriginalAmount DECIMAL(18, 2) NOT NULL DEFAULT 0