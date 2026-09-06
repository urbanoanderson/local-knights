-- Club Seed Data SQL script
-- This script alters an existing club .db to populate a test club with players and games
-- DO NOT RUN THIS ON A PRODUCTION DATABASE!!

DROP TABLE IF EXISTS SeedConfig;
DROP TABLE IF EXISTS RandomNumbers;
DROP TABLE IF EXISTS RandomCompetitions;
DROP TABLE IF EXISTS FirstNames;
DROP TABLE IF EXISTS LastNames;
DROP TABLE IF EXISTS PlayerData;

-- ============================================
-- SEED CONFIGURATION SECTION
-- ============================================

CREATE TEMP TABLE SeedConfig (
  num_players INTEGER,      -- Number of players to create
  num_games INTEGER,        -- Number of games to create
  num_competitions INTEGER, -- Number of competitions to create
  club_display_name TEXT    -- Display name for the club
);

-- INSERT INTO SeedConfig VALUES (100, 1000, 100, 'Small Club');
INSERT INTO SeedConfig VALUES (10000, 200000, 1000, 'Big Club');

-- ============================================
-- AUXILIARY TABLES
-- ============================================

CREATE TEMP TABLE FirstNames(fn, fnid);
INSERT INTO FirstNames VALUES
  ('Ana', 1), ('Alireza', 2), ('Alexander', 3), ('Anish', 4),
  ('Bobby', 5), ('Bianca', 6), ('Boris', 7), ('Bent', 8),
  ('Carlos', 9), ('Carolina', 10), ('Camila', 11), ('Catarina', 12),
  ('David', 13), ('Ding', 14), ('Daniela', 15), ('Diana', 16),
  ('Emanuel', 17), ('Erik', 18), ('Ema', 19), ('Ester', 20),
  ('Fred', 21), ('Fabiano', 22), ('Felicia', 23), ('Fatima', 24),
  ('Gary', 25), ('Gennady', 26), ('Greta', 27), ('Gabriel', 28),
  ('Hikaru', 29), ('Hugo', 30), ('Henry', 31), ('Henrique', 32),
  ('Ilya', 33), ('Iara', 34), ('Igor', 35), ('Ian', 36),
  ('Jan', 37), ('Jose', 38), ('Josh', 39), ('Judit', 40),
  ('Kevin', 41), ('Kaua', 42), ('Keti', 43), ('Katia', 44),
  ('Larissa', 45), ('Levon', 46), ('Leonid', 47), ('Lidia', 48),
  ('Mikhail', 49), ('Mark', 50), ('Maia', 51), ('Magnus', 52),
  ('Norman', 53), ('Nona', 54), ('Noemia', 55), ('Nigel', 56),
  ('Othon', 57), ('Oleg', 58), ('Olivia', 59), ('Olga', 60),
  ('Peter', 61), ('Pal', 62), ('Padmarajan', 63), ('Pia', 64),
  ('Qin', 65), ('Quincy', 66), ('Quintino', 67), ('Qingliang', 68),
  ('Richard', 69), ('Roberto', 70), ('Reuben', 71), ('Ruy', 72),
  ('Samuel', 73), ('Susan', 74), ('Sergey', 75), ('Svetozar', 76),
  ('Tarrasch', 77), ('Tatiana', 78), ('Tal', 79), ('Timur', 80),
  ('Ulrich', 81), ('Umberto', 82), ('Ulysse', 83), ('Ugo', 84),
  ('Vassily', 85), ('Veselin', 86), ('Vladimir', 87), ('Valentin', 88),
  ('Walter', 89), ('Wesley', 90), ('Wilhelm', 91), ('Wladimir', 92),
  ('Xavier', 93), ('Xiangyu', 94), ('Xue', 95), ('Xinyi', 96),
  ('Yasser', 97), ('Yuri', 98), ('Yvonne', 99), ('Yannick', 100),
  ('Zsuzsa', 101), ('Zofia', 102), ('Zozulya', 103), ('Zurab', 104);

CREATE TEMP TABLE LastNames(ln, lnid);
INSERT INTO LastNames VALUES
  ('August', 1), ('Alekhine', 2), ('Aronian', 3), ('Arakhamia', 4),
  ('Botvinnik', 5), ('Bronstein', 6), ('Burn', 7), ('Bird', 8),
  ('Carlsen', 9), ('Caruana', 10), ('Chigorin', 11), ('Capablanca', 12),
  ('Denker', 13), ('Dreev', 14), ('Devi', 15), ('Dantas', 16),
  ('Esperança', 17), ('Euwe', 18), ('Enalto', 19), ('Eliskases', 20),
  ('Fine', 21), ('Firouzja', 22), ('Fischer', 23), ('Flohr', 24),
  ('Giri', 25), ('Glek', 26), ('Gelfand', 27), ('Geller', 28),
  ('Harrison', 29), ('Honduras', 30), ('Hernandez', 31), ('Hort', 32),
  ('Irina', 33), ('Irish', 34), ('Irma', 35), ('Isaac', 36),
  ('Jones', 37), ('James', 38), ('Jackson', 39), ('Jansen', 40),
  ('Korchnoi', 41), ('Karjakin', 42), ('Karpov', 43), ('Kasparov', 44),
  ('Lopez', 45), ('Liren', 46), ('Lasker', 47), ('Larsen', 48),
  ('Moraes', 49), ('Miller', 50), ('Mednis', 51), ('Montenegro', 52),
  ('Najdorf', 53), ('Nolan', 54), ('Naval', 55), ('Nepomniachtchi', 56),
  ('Ostojić', 57), ('Olafsson', 58), ('Onischuk', 59), ('Oliver', 60),
  ('Portisch', 61), ('Petrosian', 62), ('Panno', 63), ('Pfleger', 64),
  ('Quesada', 65), ('Quezada', 66), ('Quinones', 67), ('Quan', 68),
  ('Reti', 69), ('Rubinstein', 70), ('Rossolimo', 71), ('Reshevsky', 72),
  ('Steinitz', 73), ('Spassky', 74), ('Short', 75), ('Smyslov', 76),
  ('Tartakower', 77), ('Timman', 78), ('Topalov', 79), ('Tkachenko', 80),
  ('Urzica', 81), ('Udovcic', 82), ('Unzicker', 83), ('Ulysse', 84),
  ('Voet', 85), ('Vescovi', 86), ('Vidadi', 87), ('Victorin', 88),
  ('Whitaker', 89), ('Wormser', 90), ('Welling', 91), ('Weiner', 92),
  ('Xiong', 93), ('Xu', 94), ('Xinyu', 95), ('Xie', 96),
  ('Yates', 97), ('Yanofsky', 98), ('Yilmazer', 99), ('Yusupov', 100),
  ('Zubarev', 101), ('Znosko-Borovsky', 102), ('Zuckerman', 103), ('Zinger', 104);

CREATE TEMP TABLE RandomNumbers(rnd_n);
INSERT INTO RandomNumbers
  WITH RECURSIVE
    cte(x) AS (
       SELECT ABS(RANDOM())
       UNION ALL
       SELECT ABS(RANDOM())
         FROM cte
        LIMIT (SELECT num_games FROM SeedConfig)
  )
SELECT x FROM cte;

-- Random numbers for competitions
CREATE TEMP TABLE RandomCompetitions(rnd_n);
INSERT INTO RandomCompetitions
  WITH RECURSIVE
    cte(x) AS (
       SELECT ABS(RANDOM())
       UNION ALL
       SELECT ABS(RANDOM())
         FROM cte
        LIMIT (SELECT num_competitions FROM SeedConfig)
  )
SELECT x FROM cte;

CREATE TEMP TABLE PlayerData(id, first_name, last_name, birth_year_diff, base_rating, deviation,
 title, fide_id, cbx_id);
INSERT INTO PlayerData
SELECT
  ROW_NUMBER() OVER () as id,
  (SELECT fn FROM FirstNames WHERE fnid = ((rnd_n % 104) + 1)) AS first_name,
  (SELECT ln FROM LastNames WHERE lnid = (((rnd_n / 104) % 104) + 1)) AS last_name,
  (rnd_n % 60) AS birth_year_diff,
  800 + (rnd_n % 2001) AS base_rating,
  50 + ((rnd_n / 60) % 350) AS deviation,
  (rnd_n % 7) AS title,
  (rnd_n % 10000) AS fide_id,
  (rnd_n % 10000) AS cbx_id
FROM (SELECT * FROM RandomNumbers LIMIT (SELECT num_players FROM SeedConfig));

-- ============================================
-- SEED ACTIONS
-- ============================================

-- Clear existing data
DELETE FROM Players;
DELETE FROM Games;
DELETE FROM Competitions;
DELETE FROM CompetitionParticipants;
DELETE FROM CompetitionPairings;

-- Update the existing club preferences row (always Id = 1)
UPDATE ClubPreferences
SET DisplayName = (SELECT club_display_name FROM SeedConfig),
    UpdatedAt = datetime('now')
WHERE Id = 1;

-- Generate players
INSERT INTO Players (IdentifierName, DisplayName, ChessTitle, Gender, CreatedAt, BirthDate,
    BulletRating_Rating, BulletRating_Deviation, BulletRating_Volatility,
    BlitzRating_Rating, BlitzRating_Deviation, BlitzRating_Volatility,
    RapidRating_Rating, RapidRating_Deviation, RapidRating_Volatility,
    ClassicalRating_Rating, ClassicalRating_Deviation, ClassicalRating_Volatility,
    Email, Phone, FideId, CbxId, LichessUsername, ChessComUsername)
SELECT
    LOWER(first_name || last_name || id) AS IdentifierName,
    first_name || ' ' || last_name AS DisplayName,
    CASE WHEN base_rating > 2400 THEN 5 WHEN base_rating > 2000 THEN 4 WHEN base_rating > 1600 THEN 3 ELSE 0 END AS ChessTitle,
    ABS(RANDOM()) % 3 AS Gender,
    datetime('now', '-' || (ABS(RANDOM()) % 365) || ' days') AS CreatedAt,
    date('now', '-' || birth_year_diff || ' years', '-' || (ABS(RANDOM()) % 365) || ' days') AS BirthDate,
    -- Bullet rating (typically highest)
    MIN(2900, MAX(600, base_rating + (ABS(RANDOM()) % 201) - 100)) AS BulletRating_Rating,
    MIN(400, MAX(50, deviation + (ABS(RANDOM()) % 100) - 50)) AS BulletRating_Deviation,
    0.06 AS BulletRating_Volatility,
    -- Blitz rating (between bullet and rapid)
    MIN(2800, MAX(600, base_rating - (ABS(RANDOM()) % 151) + 50)) AS BlitzRating_Rating,
    MIN(400, MAX(50, deviation + (ABS(RANDOM()) % 120) - 60)) AS BlitzRating_Deviation,
    0.06 AS BlitzRating_Volatility,
    -- Rapid rating (typically lower than blitz)
    MIN(2700, MAX(600, base_rating - (ABS(RANDOM()) % 201))) AS RapidRating_Rating,
    MIN(400, MAX(50, deviation + (ABS(RANDOM()) % 150) - 75)) AS RapidRating_Deviation,
    0.06 AS RapidRating_Volatility,
    -- Classical rating (often similar to rapid for most players)
    MIN(2650, MAX(600, base_rating - (ABS(RANDOM()) % 251) + 50)) AS ClassicalRating_Rating,
    MIN(400, MAX(50, deviation + (ABS(RANDOM()) % 180) - 90)) AS ClassicalRating_Deviation,
    0.06 AS ClassicalRating_Volatility,
    LOWER(first_name || '.' || last_name || '@example.com') AS Email,
    '+55 (81) 99999-9999' AS Phone,
    fide_id AS FideId,
    cbx_id AS CbxId,
    LOWER(first_name || last_name || id) AS LichessUsername,
    LOWER(first_name || last_name || id) AS ChessComUsername
FROM PlayerData;

-- Generate games
INSERT INTO Games (CreatedAt, TimeFormat, Result, IsRated, WhitePlayerId, BlackPlayerId,
    WhitePlayerRating_Rating, WhitePlayerRating_Deviation, WhitePlayerRating_Volatility,
    WhitePlayerRatingChange, BlackPlayerRating_Rating, BlackPlayerRating_Deviation,
    BlackPlayerRating_Volatility, BlackPlayerRatingChange, Pgn)
WITH PlayerPositions AS (
  SELECT IdentifierName, ROW_NUMBER() OVER (ORDER BY IdentifierName) - 1 as position FROM Players
)
SELECT
  datetime('now', '-' || (rnd_n % 365) || ' days') AS CreatedAt,
  rnd_n % 4 AS TimeFormat,
  rnd_n % 3 AS Result,
  rnd_n % 2 AS IsRated,
  (SELECT IdentifierName FROM PlayerPositions WHERE position = (rnd_n % (SELECT COUNT(*) FROM Players))) AS WhitePlayerId,
  (SELECT IdentifierName FROM PlayerPositions WHERE position = ((rnd_n / 100 + rnd_n % 100) % (SELECT COUNT(*) FROM Players))) AS BlackPlayerId,
  800 + (rnd_n % 2001) AS WhitePlayerRating_Rating,
  50 + ((rnd_n / 104) % 350) AS WhitePlayerRating_Deviation,
  0.06 AS WhitePlayerRating_Volatility,
  ((rnd_n % 33) - 16) AS WhitePlayerRatingChange,
  800 + (rnd_n % 2001) AS BlackPlayerRating_Rating,
  50 + ((rnd_n / 60) % 350) AS BlackPlayerRating_Deviation,
  0.06 AS BlackPlayerRating_Volatility,
  (((rnd_n % 33) - 16) * -1) AS BlackPlayerRatingChange,
  NULL AS Pgn
FROM (SELECT * FROM RandomNumbers LIMIT (SELECT num_games FROM SeedConfig));

-- Generate competitions
INSERT INTO Competitions (CreatedAt, State, Title, Description, City, ChiefArbiter, Federation, Type, TimeFormat, IsRated, NumberOfRounds, ByeValue, IsDoubleRoundRobin, InitialSeeding, CurrentRound)
SELECT
  datetime('now', '-' || (rnd_n % 365) || ' days') AS CreatedAt,
  0 AS State, -- Starting
  'Seed Competition ' || ROW_NUMBER() OVER (ORDER BY rnd_n) || ' - ' ||
    (CASE (rnd_n % 4) WHEN 0 THEN 'Swiss' WHEN 1 THEN 'RoundRobin' WHEN 2 THEN 'Knockout' ELSE 'Match' END) AS Title,
  'Auto-generated competition for testing' AS Description,
  'Recife' AS City,
  'Seed Arbiter ' || ((rnd_n % 20) + 1) AS ChiefArbiter,
  'BRA' AS Federation,
  (rnd_n % 4) AS Type, -- 0:Swiss,1:RoundRobin,2:Knockout,3:Match
  (rnd_n % 4) AS TimeFormat, -- 0..3 -> Bullet,Blitz,Rapid,Classical
  (rnd_n % 2) AS IsRated,
  CASE WHEN (rnd_n % 3) IN (0,1) THEN ((rnd_n % 7) + 3) ELSE 3 END AS NumberOfRounds,
  CASE WHEN (rnd_n % 3) IN (0,1) THEN ( (rnd_n % 3) * 0.5 ) ELSE NULL END AS ByeValue,
  CASE WHEN (rnd_n % 3) = 1 THEN (rnd_n % 2) ELSE NULL END AS IsDoubleRoundRobin,
  0 AS InitialSeeding,
  0 AS CurrentRound
FROM (SELECT * FROM RandomCompetitions LIMIT (SELECT num_competitions FROM SeedConfig));

-- Add one fixed Swiss tournament in starting state with the first 200 players as participants.
-- Only create it when the seed generated more than 200 players.
INSERT INTO Competitions (CreatedAt, State, Title, Description, City, ChiefArbiter, Federation, Type, TimeFormat, IsRated, NumberOfRounds, ByeValue, IsDoubleRoundRobin, InitialSeeding, CurrentRound)
SELECT
  datetime('now') AS CreatedAt,
  0 AS State,
  'Seed Competition - Fixed Swiss' AS Title,
  'Fixed Swiss tournament seeded for testing' AS Description,
  'Recife' AS City,
  'Seed Chief Arbiter' AS ChiefArbiter,
  'BRA' AS Federation,
  0 AS Type,
  2 AS TimeFormat,
  1 AS IsRated,
  5 AS NumberOfRounds,
  0.5 AS ByeValue,
  NULL AS IsDoubleRoundRobin,
  0 AS InitialSeeding,
  0 AS CurrentRound
WHERE (SELECT COUNT(*) FROM Players) > 200;

INSERT INTO CompetitionParticipants (
  CompetitionId,
  ParticipantIdentifierName,
  OldRating_Rating,
  OldRating_Deviation,
  OldRating_Volatility
)
SELECT
  (SELECT MAX(Id) FROM Competitions) AS CompetitionId,
  p.IdentifierName AS ParticipantIdentifierName,
  p.RapidRating_Rating AS OldRating_Rating,
  p.RapidRating_Deviation AS OldRating_Deviation,
  p.RapidRating_Volatility AS OldRating_Volatility
FROM Players AS p
WHERE (SELECT COUNT(*) FROM Players) > 200
ORDER BY p.IdentifierName
LIMIT 200;

-- ============================================
-- STATUS OUTPUT AND CLEANUP
-- ============================================

DROP TABLE IF EXISTS SeedConfig;
DROP TABLE IF EXISTS RandomNumbers;
DROP TABLE IF EXISTS RandomCompetitions;
DROP TABLE IF EXISTS FirstNames;
DROP TABLE IF EXISTS LastNames;
DROP TABLE IF EXISTS PlayerData;

-- Final status
SELECT 'Players Created' AS statusKey, COUNT(*) AS statusValue FROM Players
  UNION ALL
SELECT 'Games Created' AS statusKey, COUNT(*) AS statusValue FROM Games
  UNION ALL
SELECT 'Competitions Created' AS statusKey, COUNT(*) AS statusValue FROM Competitions;