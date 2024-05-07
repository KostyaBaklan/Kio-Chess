SELECT Sequence, NextMove, (White+Black+Draw) AS Total, 
Case (length(sequence)/2)%2
	WHEN 0 THEN (10000*(White - Black)/(White+Black+Draw))
	ELSE (10000*(Black - White)/(White+Black+Draw))
END as Difference
from Books
where White+Black+Draw > 10 and length(sequence) < 57


Insert into PositionTotal
SELECT Sequence, NextMove, (White+Black+Draw) AS Total, 
Case (length(sequence)/2)%2
	WHEN 0 THEN (10000*(White - Black)/(White+Black+Draw))
	ELSE (10000*(Black - White)/(White+Black+Draw))
END as Difference
from Books
where White+Black+Draw > 10 and length(sequence) < 57

CREATE TABLE "PositionTotal" (
	"Sequence"	BLOB,
	"NextMove"	INTEGER,
	"Total"	INTEGER NOT NULL,
	"Difference"	INTEGER NOT NULL,
	PRIMARY KEY("Sequence","NextMove")
);

CREATE TABLE "PositionTotalDifference" (
	"Sequence"	TEXT,
	"NextMove"	INTEGER,
	"Total"	INTEGER NOT NULL,
	"Difference"	INTEGER NOT NULL,
	PRIMARY KEY("Sequence","NextMove")
);

CREATE VIEW PTD_View 
AS 
SELECT History, NextMove, (White+Black+Draw) AS Total, 
                        Case (length(History)/2)%2
	                        WHEN 0 THEN (10000*(White - Black)/(White+Black+Draw))
	                        ELSE (10000*(Black - White)/(White+Black+Draw))
                        END as Difference
                        from Books
                        where White+Black+Draw >= 20 and length(History) < 57