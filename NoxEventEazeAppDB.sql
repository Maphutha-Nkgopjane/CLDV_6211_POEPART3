
USE master;
GO

-- Drop the database if it exists
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'NoxEventEazeAppDB')
BEGIN
    PRINT 'Dropping existing database: NoxEventEazeAppDB...';
    ALTER DATABASE NoxEventEazeAppDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE NoxEventEazeAppDB;
    PRINT 'Database NoxEventEazeAppDB dropped.';
END
GO

-- Create the new database
PRINT 'Creating new database: NoxEventEazeAppDB...';
CREATE DATABASE NoxEventEazeAppDB;
GO
PRINT 'Database NoxEventEazeAppDB created.';

-- Switch to the new database
USE NoxEventEazeAppDB;
GO

-- Drop existing tables if they exist, in the correct dependency order
PRINT 'Dropping tables if they exist...';
-- Bookings depends on Events and Venues
IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Bookings;
    PRINT 'Table dbo.Bookings dropped.';
END

-- Events depends on Venues and EventTypes
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Events;
    PRINT 'Table dbo.Events dropped.';
END

-- EventTypes has no dependencies here
IF OBJECT_ID('dbo.EventTypes', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.EventTypes;
    PRINT 'Table dbo.EventTypes dropped.';
END

-- Venues has no dependencies here
IF OBJECT_ID('dbo.Venues', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Venues;
    PRINT 'Table dbo.Venues dropped.';
END
GO
PRINT 'Table drop operations complete.';


-- Create Venues table (Added IsAvailable)
PRINT 'Creating Venues table...';
CREATE TABLE Venues (
    VenueID INT PRIMARY KEY IDENTITY(1,1),
    VenueName VARCHAR(255) NOT NULL,
    Location VARCHAR(255) NOT NULL,
    Capacity INT NOT NULL,
    ImageURL VARCHAR(2048),
    IsAvailable BIT NOT NULL DEFAULT 1 -- <--- ADDED THIS LINE for Venue availability
);
GO
PRINT 'Venues table created.';

-- Create EventTypes table (NEW TABLE)
PRINT 'Creating EventTypes table...';
CREATE TABLE EventTypes (
    EventTypeID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500)
);
GO
PRINT 'EventTypes table created.';

-- Create Events table (CORRECTED: Capacity column added here, EventTypeID and FK added)
PRINT 'Creating Events table...';
CREATE TABLE Events (
    EventID INT PRIMARY KEY IDENTITY(1,1),
    EventName VARCHAR(255) NOT NULL,
    EventDate DATE NOT NULL,
    EventTime TIME NOT NULL,
    Description TEXT,
    ImageURL VARCHAR(2048),
    VenueID INT NOT NULL,
    Capacity INT NOT NULL,
    EventTypeID INT NOT NULL, -- <--- ADDED THIS LINE for EventType
    FOREIGN KEY (VenueID) REFERENCES Venues(VenueID),
    FOREIGN KEY (EventTypeID) REFERENCES EventTypes(EventTypeID) -- <--- ADDED THIS FOREIGN KEY
);
GO
PRINT 'Events table created.';

-- Create Bookings table (Added UserID and NumberOfTickets)
PRINT 'Creating Bookings table...';
CREATE TABLE Bookings (
    BookingID INT PRIMARY KEY IDENTITY(1,1),
    EventID INT NOT NULL, -- Changed order, EventID first based on Booking model foreign key
    UserID NVARCHAR(128) NOT NULL, -- <--- ADDED THIS LINE for User ID (matching ASP.NET Identity default length)
    NumberOfTickets INT NOT NULL, -- <--- ADDED THIS LINE for number of tickets
    BookingDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(50) DEFAULT 'Confirmed', -- Added for booking status
    FOREIGN KEY (EventID) REFERENCES Events(EventID)
    -- FOREIGN KEY (UserID) REFERENCES AspNetUsers(Id) -- This would be added if you implement ASP.NET Identity
);
GO
PRINT 'Bookings table created.';

-- Optional: Insert sample data
PRINT 'Inserting sample data...';

-- Insert Event Types first
INSERT INTO EventTypes (Name, Description) VALUES
('Concert', 'Live music performances'),
('Sporting Event', 'Various sports matches and tournaments'),
('Conference', 'Professional and academic gatherings'),
('Workshop', 'Hands-on learning sessions'),
('Festival', 'Multi-day celebrations with various activities');
GO


INSERT INTO Venues (VenueName, Location, Capacity, ImageURL, IsAvailable) VALUES
('Grand Hall', 'City Center', 500, 'https://via.placeholder.com/300/0000FF/FFFFFF?Text=Grand+Hall', 1),
('The Ballroom', 'Downtown', 300, 'https://via.placeholder.com/300/FF0000/FFFFFF?Text=The+Ballroom', 1),
('Exhibition Centre', 'Industrial Park', 1000, 'https://via.placeholder.com/300/00FFFF/FFFFFF?Text=Exhibition', 0); -- Example of an unavailable venue
GO

INSERT INTO Events (VenueID, EventName, EventDate, EventTime, Description, ImageURL, Capacity, EventTypeID) VALUES
(1, 'Tech Conference 2025', '2025-06-15', '09:00', 'Annual tech conference on AI and Machine Learning.', 'https://via.placeholder.com/300/008000/FFFFFF?Text=Tech+Conf', 200, (SELECT EventTypeID FROM EventTypes WHERE Name = 'Conference')),
(2, 'Summer Gala', '2025-07-20', '19:00', 'Charity gala event supporting local artists.', 'https://via.placeholder.com/300/FFA500/FFFFFF?Text=Summer+Gala', 150, (SELECT EventTypeID FROM EventTypes WHERE Name = 'Festival')),
(1, 'Rock Concert with The Shredders', '2025-08-01', '20:00', 'High-energy rock concert.', 'https://via.placeholder.com/300/800080/FFFFFF?Text=Rock+Concert', 450, (SELECT EventTypeID FROM EventTypes WHERE Name = 'Concert')),
(2, 'Yoga Workshop', '2025-06-28', '10:00', 'Morning yoga session for all levels.', 'https://via.placeholder.com/300/00CED1/FFFFFF?Text=Yoga+Workshop', 50, (SELECT EventTypeID FROM EventTypes WHERE Name = 'Workshop'));
GO

-- UserID is a placeholder here. In a real app, you'd link to AspNetUsers.Id
INSERT INTO Bookings (EventID, UserID, NumberOfTickets, BookingDate, Status) VALUES
((SELECT EventID FROM Events WHERE EventName = 'Tech Conference 2025'), 'TemporaryUserID_1', 2, GETDATE(), 'Confirmed'),
((SELECT EventID FROM Events WHERE EventName = 'Summer Gala'), 'TemporaryUserID_2', 1, GETDATE(), 'Confirmed');
GO
PRINT 'Sample data insertion complete.';

-- Success message for data insertion into Bookings
RAISERROR('Database setup and sample data inserted successfully.', 10, 1);
GO

-- Select all records from Venues to verify the data
SELECT * FROM dbo.Venues;
GO

-- Select all records from EventTypes to verify the data
SELECT * FROM dbo.EventTypes;
GO

-- Select all records from Events to verify the data
SELECT * FROM dbo.Events;
GO

-- Select all records from Bookings to verify the data
SELECT * FROM dbo.Bookings;
GO

