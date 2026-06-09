-- ============================================================================
-- GamesDbContext Schema - SQLite Database Creation Script
-- Database: games.db
-- Purpose: Store chess game statistics with 128-bit hash support
-- ============================================================================

-- Drop existing table if recreating
-- DROP TABLE IF EXISTS GameEntities;

-- ============================================================================
-- Table: GameEntities
-- Purpose: Store game position statistics indexed by 128-bit hash + next move
-- ============================================================================
CREATE TABLE IF NOT EXISTS GameEntities (
    -- 128-bit hash split into two 64-bit parts
    Low INTEGER NOT NULL,                -- Lower 64 bits of UInt128 hash
    High INTEGER NOT NULL,               -- Upper 64 bits of UInt128 hash

    -- Next move and game statistics
    NextMove INTEGER NOT NULL,           -- Next move key (short)
    White INTEGER NOT NULL DEFAULT 0,    -- Count of white wins
    Draw INTEGER NOT NULL DEFAULT 0,     -- Count of draws
    Black INTEGER NOT NULL DEFAULT 0,    -- Count of black wins
    Length INTEGER NOT NULL DEFAULT 0,   -- Position depth (ply count)

    -- Composite primary key: (Low, High, NextMove)
    -- This ensures uniqueness for each position + move combination
    PRIMARY KEY (Low, High, NextMove)
) WITHOUT ROWID;

-- ============================================================================
-- Index: IX_GameEntities_Length
-- Purpose: Fast filtering by position depth (e.g., opening moves)
-- ============================================================================
CREATE INDEX IF NOT EXISTS IX_GameEntities_Length 
ON GameEntities(Length);

-- ============================================================================
-- Index: IX_GameEntities_Hash
-- Purpose: Fast lookup of all moves for a given position hash
-- ============================================================================
CREATE INDEX IF NOT EXISTS IX_GameEntities_Hash 
ON GameEntities(Low, High);

-- ============================================================================
-- Performance Optimizations
-- ============================================================================

-- Set page size (must be done BEFORE any data is inserted)
-- PRAGMA page_size = 4096;  -- Default, good for most cases
-- PRAGMA page_size = 8192;  -- Better for large databases

-- Enable Write-Ahead Logging for better concurrency
PRAGMA journal_mode = WAL;

-- Set cache size (in pages, negative = KB)
-- PRAGMA cache_size = -64000;  -- 64 MB cache

-- Auto-vacuum to prevent database bloat
PRAGMA auto_vacuum = INCREMENTAL;

-- ============================================================================
-- Verify Schema
-- ============================================================================

-- Show table structure
-- .schema GameEntities

-- Show indexes
-- SELECT name, sql FROM sqlite_master WHERE type='index' AND tbl_name='GameEntities';

-- ============================================================================
-- Sample Queries
-- ============================================================================

-- Get all moves for a specific position (by hash)
-- SELECT * FROM GameEntities 
-- WHERE Low = 12345678901234567890 
--   AND High = 98765432109876543210
-- ORDER BY (White + Draw + Black) DESC;

-- Get opening positions (first 10 plies)
-- SELECT * FROM GameEntities 
-- WHERE Length <= 10
-- ORDER BY Length, (White + Draw + Black) DESC;

-- Get total number of games (starting position, Length = 0)
-- SELECT SUM(White + Draw + Black) AS TotalGames
-- FROM GameEntities
-- WHERE Length = 0;

-- Get statistics
-- SELECT 
--     COUNT(*) AS TotalRecords,
--     MIN(Length) AS MinDepth,
--     MAX(Length) AS MaxDepth,
--     SUM(White + Draw + Black) AS TotalGames
-- FROM GameEntities;

-- ============================================================================
-- Migration Notes
-- ============================================================================

-- Source: chess.db / Books table
--   - History BLOB (move sequence)
--   - NextMove INTEGER
--   - White, Draw, Black INTEGER
--
-- Transformation:
--   1. History (byte[]) ? UInt128 hash using MoveHashSequenceHasher
--   2. Split UInt128 ? (Low: ulong, High: ulong)
--   3. Compute Length = History.Length / 2
--
-- ON CONFLICT behavior:
--   - Accumulate statistics: White = White + excluded.White
--   - Take minimum length: Length = MIN(Length, excluded.Length)

-- ============================================================================
-- Maintenance Commands
-- ============================================================================

-- Rebuild indexes (after bulk insert)
-- REINDEX GameEntities;

-- Update query planner statistics
-- ANALYZE GameEntities;

-- Reclaim unused space
-- VACUUM;

-- Check database integrity
-- PRAGMA integrity_check;

-- ============================================================================
-- Database Size Estimates
-- ============================================================================

-- Expected size for 700M records:
--   - Row size: ~40 bytes (7 INTEGER columns)
--   - Data: 700M × 40 = 28 GB
--   - Indexes: ~14 GB (2 indexes)
--   - Total: ~45-50 GB

-- ============================================================================
-- End of Schema
-- ============================================================================
