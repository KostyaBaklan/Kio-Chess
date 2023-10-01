CREATE TABLE "Openings" (
	"Id"	INTEGER,
	"Name"	TEXT,
	PRIMARY KEY("Id")
);

CREATE TABLE "Variations" (
	"Id"	INTEGER,
	"Name"	TEXT,
	PRIMARY KEY("Id")
);

CREATE TABLE "OpeningVariations" (
	"Id"	INTEGER,
	"Name"	TEXT,
	"OpeningID"	INTEGER,
	"VariationID"	INTEGER,
	"Moves"	TEXT,
	PRIMARY KEY("Id")
);

CREATE TABLE "OpeningSequences" (
	"Id"	INTEGER,
	"Sequence"	TEXT,
	"OpeningVariationID"	INTEGER,
	PRIMARY KEY("Id")
);

CREATE UNIQUE INDEX "Openings_Name" ON "Openings" (
	"Name"	ASC
);

CREATE INDEX "OpeningSequences_Sequence" ON "OpeningSequences" (
	"Sequence"	ASC
);

CREATE UNIQUE INDEX "OpeningVariations_Name" ON "OpeningVariations" (
	"Name"	ASC
);

CREATE UNIQUE INDEX "Variations_name" ON "Variations" (
	"Name"	ASC
);