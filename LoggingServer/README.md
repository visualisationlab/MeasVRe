# Logging Server
MeasVReLog is a Flask REST API server where the Unity client in MeasVRe (the LogManager) can send
measurements data to. The data is then stored in a project in an SQLite database. With the provided
JavaScript client, the data can then be download via the browser as
a zip file containing the measurements data in JSON and CSV format. The zip also contains a folder
with the snapshots of the measuremetns in PNG format.

## Installation
The requirements can be installed with
```
python -m pip install .
```

## Running the server
Run the command below in the `LoggingServer` folder to (re)initialize the database.
This will create an `instance` folder that contains the `.sqlite` file.
```
python -m flask init-db
```
Execute the following commands to start the FLASK application locally:
#### Bash
```Bash
export FLASK_APP=MeasVReLog
export FLASK_ENV=development
python -m flask run
```
#### PowerShell
```PowerShell
$env:FLASK_APP = "MeasVReLog"
$env:FLASK_ENV = "development"
python -m flask run
```
#### CMD
```CMD
set FLASK_APP=MeasVReLog
set FLASK_ENV=development
python -m flask run
```

It is then possible to create/delete projects and download project data via the browser at
<http://localhost:5000/>
