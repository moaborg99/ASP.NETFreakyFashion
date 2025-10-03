/*Skapa tabellen Products*/
CREATE TABLE dbo.Products
(
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name         NVARCHAR(100)     NOT NULL,
    Description  NVARCHAR(500)     NOT NULL,
    Price        DECIMAL(10,2)     NOT NULL,        
    ImageUrl     NVARCHAR(255)     NOT NULL,
    CONSTRAINT CK_Products_Price_NonNegative CHECK (Price >= 0) 
);

/*L�gg till en produkt i tabellen Products*/
INSERT INTO dbo.Products (Name, Description, Price, ImageUrl)
VALUES 
(N'Svart T-shirt', N'Beskrivning av svart T-Shirt', 199.00, N'/images/products/svart-t-shirt'),
(N'Vit T-shirt', N'Beskrivning av vit T-Shirt', 199.00, N'/images/products/vit-t-shirt'),
(N'R�d T-shirt', N'Beskrivning av r�d T-Shirt', 199.00, N'/images/products/r�d-t-shirt');

-- 1A) L�gg till kolumnen (tillf�lligt NULL)
ALTER TABLE dbo.Products
ADD UrlSlug NVARCHAR(100) NULL;

-- 1B)  S�tt slug p� ev. befintliga rader
-- Byt WHERE efter dina produkter
UPDATE dbo.Products
SET UrlSlug = N'svart-t-shirt'
WHERE Name = N'Svart T-Shirt';

-- 1C) G�r kolumnen obligatorisk + unik
ALTER TABLE dbo.Products
ALTER COLUMN UrlSlug NVARCHAR(100) NOT NULL;

CREATE UNIQUE INDEX UX_Products_UrlSlug ON dbo.Products(UrlSlug);