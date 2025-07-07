create DATABASE ServisMobil;


-- Tabel Pelanggan
CREATE TABLE Pelanggan (
    ID_Pelanggan INT IDENTITY(1,1) PRIMARY KEY,
    Nama VARCHAR(100) NOT NULL,
    Telepon VARCHAR(13) NOT NULL UNIQUE CHECK (Telepon LIKE '08%' AND LEN(Telepon) BETWEEN 12 AND 13),
    Alamat VARCHAR(255) NOT NULL,
    Email VARCHAR(30) NOT NULL UNIQUE CHECK (Email LIKE '_%@_%.com')
);

-- Tabel Mekanik
CREATE TABLE Mekanik (
    ID_Mekanik INT IDENTITY(1,1) PRIMARY KEY,
    Nama VARCHAR(100) NOT NULL,
    Telepon VARCHAR(13) NOT NULL UNIQUE CHECK (Telepon LIKE '08%' AND LEN(Telepon) BETWEEN 12 AND 13),
    Spesialisasi VARCHAR(20) NOT NULL CHECK (Spesialisasi IN ('Tune Up', 'Servis Berkala', 'Mesin'))
);


-- Tabel Kendaraan
CREATE TABLE Kendaraan (
    ID_Kendaraan INT IDENTITY(1,1) PRIMARY KEY,
    ID_Pelanggan INT NOT NULL,
    Merek VARCHAR(50) NOT NULL,
    Model VARCHAR(50) NOT NULL,
    Tahun CHAR(4) NOT NULL CHECK (Tahun BETWEEN '2000' AND CAST(YEAR(GETDATE()) AS CHAR(4))),
    NomorPlat VARCHAR(11) NOT NULL UNIQUE,
    FOREIGN KEY (ID_Pelanggan) REFERENCES Pelanggan(ID_Pelanggan) ON DELETE CASCADE
);

-- Tabel LayananServis
CREATE TABLE LayananServis (
    ID_Layanan INT IDENTITY(1,1) PRIMARY KEY,
    NamaLayanan VARCHAR(100) NOT NULL,
    Deskripsi VARCHAR(255) NOT NULL,
    Harga DECIMAL(11,2) NOT NULL CHECK (Harga >= 0)
);

-- Tabel PemesananServis
CREATE TABLE PemesananServis (
    ID_Pemesanan INT IDENTITY(1,1) PRIMARY KEY,
    ID_Pelanggan INT NOT NULL,
    ID_Kendaraan INT NOT NULL,
    ID_Layanan INT NOT NULL,
    ID_Mekanik INT NULL,
    TanggalPesan DATETIME NOT NULL DEFAULT GETDATE(),
    TanggalServis DATETIME NOT NULL,
    Status VARCHAR(10) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Selesai')),
    FOREIGN KEY (ID_Pelanggan) REFERENCES Pelanggan(ID_Pelanggan) ON DELETE CASCADE,
    FOREIGN KEY (ID_Kendaraan) REFERENCES Kendaraan(ID_Kendaraan) ON DELETE NO ACTION,
    FOREIGN KEY (ID_Layanan) REFERENCES LayananServis(ID_Layanan) ON DELETE CASCADE,
    FOREIGN KEY (ID_Mekanik) REFERENCES Mekanik(ID_Mekanik) ON DELETE SET NULL,
    CONSTRAINT chk_TanggalPesanServis CHECK (TanggalPesan <= TanggalServis AND TanggalServis >= GETDATE())
);

CREATE TABLE LaporanServis (
    ID_Laporan INT IDENTITY(1,1) PRIMARY KEY,
    ID_Pemesanan INT NOT NULL,
    Deskripsi VARCHAR(255) NOT NULL,
    BiayaTambahan DECIMAL(11,2) NOT NULL DEFAULT 0 CHECK (BiayaTambahan >= 0),
    TanggalSelesai DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ID_Pemesanan) REFERENCES PemesananServis(ID_Pemesanan) ON DELETE CASCADE
);

CREATE TRIGGER trg_CekStatusDanTanggalSelesai
ON LaporanServis
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Cek status 'Selesai' dan validasi tanggal
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN PemesananServis ps ON ps.ID_Pemesanan = i.ID_Pemesanan
        WHERE ps.Status != 'Selesai'
           OR i.TanggalSelesai < ps.TanggalServis
    )
    BEGIN
        RAISERROR('Laporan hanya bisa dibuat jika status pemesanan sudah Selesai dan TanggalSelesai >= TanggalServis.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Jika valid, masukkan data
    INSERT INTO LaporanServis (ID_Pemesanan, Deskripsi, BiayaTambahan, TanggalSelesai)
    SELECT ID_Pemesanan, Deskripsi, BiayaTambahan, TanggalSelesai
    FROM inserted;
END

CREATE PROCEDURE InsertPelanggan
    @Nama VARCHAR(100),
    @Telepon VARCHAR(13),
    @Alamat VARCHAR(255),
    @Email VARCHAR(30)
AS
BEGIN
    INSERT INTO Pelanggan (Nama, Telepon, Alamat, Email)
    VALUES (@Nama, @Telepon, @Alamat, @Email);
END
GO

CREATE PROCEDURE UpdatePelanggan
    @ID_Pelanggan INT,
    @Nama VARCHAR(100),
    @Telepon VARCHAR(13),
    @Alamat VARCHAR(255),
    @Email VARCHAR(30)
AS
BEGIN
    UPDATE Pelanggan
    SET Nama = COALESCE(NULLIF(@Nama, ''), Nama),
        Telepon = COALESCE(NULLIF(@Telepon, ''), Telepon),
        Alamat = COALESCE(NULLIF(@Alamat, ''), Alamat),
        Email = COALESCE(NULLIF(@Email, ''), Email)
    WHERE ID_Pelanggan = @ID_Pelanggan;
END
GO

CREATE PROCEDURE DeletePelanggan
    @ID_Pelanggan INT
AS
BEGIN
    DELETE FROM Pelanggan
    WHERE ID_Pelanggan = @ID_Pelanggan;
END
GO
-- Add
CREATE PROCEDURE InsertMekanik
    @Nama VARCHAR(100),
    @Telepon VARCHAR(13),
    @Spesialisasi VARCHAR(20)
AS
BEGIN
    INSERT INTO Mekanik (Nama, Telepon, Spesialisasi)
    VALUES (@Nama, @Telepon, @Spesialisasi);
END
GO

-- Update
CREATE PROCEDURE UpdateMekanik
    @ID_Mekanik INT,
    @Nama VARCHAR(100),
    @Telepon VARCHAR(13),
    @Spesialisasi VARCHAR(20)
AS
BEGIN
    UPDATE Mekanik
    SET Nama = COALESCE(NULLIF(@Nama, ''), Nama),
        Telepon = COALESCE(NULLIF(@Telepon, ''), Telepon),
        Spesialisasi = COALESCE(NULLIF(@Spesialisasi, ''), Spesialisasi)
    WHERE ID_Mekanik = @ID_Mekanik;
END
GO

-- Delete
CREATE PROCEDURE DeleteMekanik
    @ID_Mekanik INT
AS
BEGIN
    DELETE FROM Mekanik
    WHERE ID_Mekanik = @ID_Mekanik;
END
GO
-- Add
CREATE PROCEDURE InsertKendaraan
    @ID_Pelanggan INT,
    @Merek VARCHAR(50),
    @Model VARCHAR(50),
    @Tahun CHAR(4),
    @NomorPlat VARCHAR(11)
AS
BEGIN
    INSERT INTO Kendaraan (ID_Pelanggan, Merek, Model, Tahun, NomorPlat)
    VALUES (@ID_Pelanggan, @Merek, @Model, @Tahun, @NomorPlat);
END
GO

-- Update
CREATE PROCEDURE UpdateKendaraan
    @ID_Kendaraan INT,
    @Merek VARCHAR(50),
    @Model VARCHAR(50),
    @Tahun CHAR(4),
    @NomorPlat VARCHAR(11)
AS
BEGIN
    UPDATE Kendaraan
    SET Merek = COALESCE(NULLIF(@Merek, ''), Merek),
        Model = COALESCE(NULLIF(@Model, ''), Model),
        Tahun = COALESCE(NULLIF(@Tahun, ''), Tahun),
        NomorPlat = COALESCE(NULLIF(@NomorPlat, ''), NomorPlat)
    WHERE ID_Kendaraan = @ID_Kendaraan;
END
GO

-- Delete
CREATE PROCEDURE DeleteKendaraan
    @ID_Kendaraan INT
AS
BEGIN
    DELETE FROM Kendaraan
    WHERE ID_Kendaraan = @ID_Kendaraan;
END
GO


-- Add
CREATE PROCEDURE InsertLayanan
    @NamaLayanan VARCHAR(100),
    @Deskripsi VARCHAR(255),
    @Harga DECIMAL(11,2)
AS
BEGIN
    INSERT INTO LayananServis (NamaLayanan, Deskripsi, Harga)
    VALUES (@NamaLayanan, @Deskripsi, @Harga);
END
GO

-- Update
CREATE PROCEDURE UpdateLayanan
    @ID_Layanan INT,
    @NamaLayanan VARCHAR(100),
    @Deskripsi VARCHAR(255),
    @Harga DECIMAL(11,2)
AS
BEGIN
    UPDATE LayananServis
    SET NamaLayanan = COALESCE(NULLIF(@NamaLayanan, ''), NamaLayanan),
        Deskripsi = COALESCE(NULLIF(@Deskripsi, ''), Deskripsi),
        Harga = ISNULL(@Harga, Harga)
    WHERE ID_Layanan = @ID_Layanan;
END
GO

-- Delete
CREATE PROCEDURE DeleteLayanan
    @ID_Layanan INT
AS
BEGIN
    DELETE FROM LayananServis
    WHERE ID_Layanan = @ID_Layanan;
END
GO


-- Add
CREATE PROCEDURE InsertPemesananServis
    @ID_Pelanggan INT,
    @ID_Kendaraan INT,
    @ID_Layanan INT,
    @ID_Mekanik INT,
    @TanggalServis DATETIME
AS
BEGIN
    INSERT INTO PemesananServis (ID_Pelanggan, ID_Kendaraan, ID_Layanan, ID_Mekanik, TanggalServis)
    VALUES (@ID_Pelanggan, @ID_Kendaraan, @ID_Layanan, @ID_Mekanik, @TanggalServis);
END
GO

-- Update
CREATE PROCEDURE UpdatePemesananServis
    @ID_Pemesanan INT,
    @ID_Mekanik INT,
    @TanggalServis DATETIME,
    @Status VARCHAR(10)
AS
BEGIN
    UPDATE PemesananServis
    SET ID_Mekanik = ISNULL(@ID_Mekanik, ID_Mekanik),
        TanggalServis = ISNULL(@TanggalServis, TanggalServis),
        Status = COALESCE(NULLIF(@Status, ''), Status)
    WHERE ID_Pemesanan = @ID_Pemesanan;
END
GO

-- Delete
CREATE PROCEDURE DeletePemesananServis
    @ID_Pemesanan INT
AS
BEGIN
    DELETE FROM PemesananServis
    WHERE ID_Pemesanan = @ID_Pemesanan;
END
GO



CREATE PROCEDURE InsertLaporanServis
    @ID_Pemesanan INT,
    @Deskripsi VARCHAR(255),
    @BiayaTambahan DECIMAL(11,2),
    @TanggalSelesai DATETIME
AS
BEGIN
    INSERT INTO LaporanServis (ID_Pemesanan, Deskripsi, BiayaTambahan, TanggalSelesai)
    VALUES (@ID_Pemesanan, @Deskripsi, @BiayaTambahan, @TanggalSelesai);
END
GO



-- Update
CREATE PROCEDURE UpdateLaporanServis
    @ID_Laporan INT,
    @Deskripsi VARCHAR(255),
    @BiayaTambahan DECIMAL(11,2),
    @TanggalSelesai DATETIME
AS
BEGIN
    UPDATE LaporanServis
    SET Deskripsi = COALESCE(NULLIF(@Deskripsi, ''), Deskripsi),
        BiayaTambahan = ISNULL(@BiayaTambahan, BiayaTambahan),
        TanggalSelesai = ISNULL(@TanggalSelesai, TanggalSelesai)
    WHERE ID_Laporan = @ID_Laporan;
END
GO

-- Delete
CREATE PROCEDURE DeleteLaporanServis
    @ID_Laporan INT
AS
BEGIN
    DELETE FROM LaporanServis
    WHERE ID_Laporan = @ID_Laporan;
END
GO


CREATE TABLE Admin (
    ID_Admin INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(10) NOT NULL UNIQUE,
    Password VARCHAR(12) NOT NULL,
    Role VARCHAR(5) NOT NULL DEFAULT 'Admin'
);

INSERT INTO Admin (Username, Password)
VALUES ('admin', 'admin');


CREATE NONCLUSTERED INDEX idx_Pemesanan_Pelanggan ON PemesananServis(ID_Pelanggan);
CREATE NONCLUSTERED INDEX idx_Pemesanan_IDKendaraan ON PemesananServis(ID_Kendaraan);
CREATE NONCLUSTERED INDEX idx_Pemesanan_IDLayanan ON PemesananServis(ID_Layanan);
CREATE NONCLUSTERED INDEX idx_Pemesanan_IDMekanik ON PemesananServis(ID_Mekanik);

CREATE NONCLUSTERED INDEX idx_Laporan_IDPemesanan ON LaporanServis(ID_Pemesanan);

CREATE NONCLUSTERED INDEX idx_Kendaraan_Merek ON Kendaraan(Merek);

CREATE NONCLUSTERED INDEX idx_Pelanggan_NamaPelanggan ON Pelanggan(Nama);

CREATE NONCLUSTERED INDEX idx_Mekanik_NamaMekanik ON Mekanik(Nama);

CREATE NONCLUSTERED INDEX idx_Layanan_NamaLayanan ON LayananServis(NamaLayanan);
SELECT 
    p.Nama AS NamaPelanggan,
    p.Telepon AS TeleponPelanggan,
    k.NomorPlat,
	k.Merek,
	k.Model,
    lsrv.NamaLayanan,
    m.Nama AS NamaMekanik,
    lsrv.Harga AS HargaLayanan,
    ps.TanggalPesan,
    ls.TanggalSelesai,
	ls.BiayaTambahan,
    ps.Status,
    (lsrv.Harga + ls.BiayaTambahan) AS TotalHarga

FROM LaporanServis ls
INNER JOIN PemesananServis ps ON ls.ID_Pemesanan = ps.ID_Pemesanan
INNER JOIN Pelanggan p ON ps.ID_Pelanggan = p.ID_Pelanggan
INNER JOIN Kendaraan k ON ps.ID_Kendaraan = k.ID_Kendaraan
INNER JOIN LayananServis lsrv ON ps.ID_Layanan = lsrv.ID_Layanan
LEFT JOIN Mekanik m ON ps.ID_Mekanik = m.ID_Mekanik

ORDER BY ls.TanggalSelesai DESC, ls.ID_Laporan;

SET STATISTICS IO ON;
SET STATISTICS TIME ON;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;

SELECT Nama, Telepon, Email FROM dbo.Pelanggan WHERE Nama LIKE 'A%';
SELECT Nama, Telepon, Spesialisasi FROM dbo.Mekanik WHERE Nama LIKE 'A%';
SELECT NomorPlat, Merek, Model FROM dbo.Kendaraan WHERE Merek LIKE 'H%';
SELECT * FROM LayananServis WHERE NamaLayanan LIKE 'Servis%';

