DROP TABLE IF EXISTS projects;
DROP TABLE IF EXISTS measurements;
DROP TABLE IF EXISTS markers;
DROP TABLE IF EXISTS snapshots;

CREATE TABLE projects (
    "key" TEXT PRIMARY KEY NOT NULL,
    name TEXT DEFAULT NULL,
    timestamp_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    timestamp_modified TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE TABLE measurements (
    id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    projects_key TEXT NOT NULL,
    type TEXT NOT NULL,
    value REAL NOT NULL,
    FOREIGN KEY (projects_key) REFERENCES projects("key") ON DELETE CASCADE
);

CREATE TABLE markers (
    id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    measurements_id INTEGER NOT NULL,
    x REAL NOT NULL,
    y REAL NOT NULL,
    z REAL NOT NULL,
    FOREIGN KEY (measurements_id) REFERENCES measurements(id) ON DELETE CASCADE
);

CREATE TABLE snapshots (
    file_name TEXT PRIMARY KEY NOT NULL,
    measurements_id INTEGER NOT NULL,
    FOREIGN KEY (measurements_id) REFERENCES measurements(id) ON DELETE CASCADE
);

