CREATE TABLE "Openings" (
	"Id"	INTEGER,
	"Name"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE "Variations" (
	"Id"	INTEGER,
	"Name"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE "OpeningVariations" (
	"Id"	INTEGER,
	"Name"	TEXT,
	"OpeningID"	INTEGER,
	"VariationID"	INTEGER,
	"Moves"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT),
	FOREIGN KEY("VariationID") REFERENCES "Variations"("Id"),
	FOREIGN KEY("OpeningID") REFERENCES "Openings"("Id")
);

CREATE TABLE "OpeningSequences" (
	"Id"	INTEGER,
	"Sequence"	TEXT,
	"OpeningVariationID"	INTEGER,
	FOREIGN KEY("OpeningVariationID") REFERENCES "OpeningVariations"("Id"),
	PRIMARY KEY("Id" AUTOINCREMENT)
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