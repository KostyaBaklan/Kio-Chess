-- SQL Script to Reset ResultContext Database
-- Use this in DB Browser for SQLite to reset the database
-- This script deletes all data and resets identity columns

-- Step 1: Disable foreign key constraints temporarily
PRAGMA foreign_keys = OFF;

-- Step 2: Delete all rows from ResultEntity table
DELETE FROM ResultEntity;

-- Step 3: Delete all rows from RunTimeInformation table
DELETE FROM RunTimeInformation;

-- Step 4: Reset the autoincrement sequence for ResultEntity
DELETE FROM sqlite_sequence WHERE name='ResultEntity';

-- Step 5: Reset the autoincrement sequence for RunTimeInformation
DELETE FROM sqlite_sequence WHERE name='RunTimeInformation';

-- Step 6: Re-enable foreign key constraints
PRAGMA foreign_keys = ON;

-- Step 7 (OPTIONAL - RUN SEPARATELY): Compact (vacuum) the database to reclaim space
-- NOTE: VACUUM must be run separately outside of a transaction
-- Execute this in a new SQL Editor tab after running the above script:
-- VACUUM;
