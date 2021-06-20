from flask import jsonify, request
from flask.globals import current_app
from flask.helpers import make_response
from MeasVReLog.db import get_db, close_db
from MeasVReLog.api import bp
from werkzeug.http import HTTP_STATUS_CODES
from werkzeug.utils import secure_filename

import uuid
import csv
import os
import zipfile
import json
import io

def error_response(status_code, message=None):
    """
    Returns a JSON response with the given status code and message.
    """
    payload = {"error": HTTP_STATUS_CODES.get(status_code, "Unknown error")}

    if message:
        payload["message"] = message

    response = jsonify(payload)
    response.status_code = status_code
    return response


def allowed_file(file_name):
    """
    Check if the extension of the file is allowed.
    """
    return '.' in file_name and \
           file_name.rsplit('.', 1)[1].lower() in current_app.config["ALLOWED_EXTENSIONS"]


def project_exists(cur, key):
    """
    Check whether the project exists in the database.
    """
    project_data = cur.execute(
        "SELECT * FROM projects WHERE \"key\"=?", (key,)
    ).fetchone()

    if not project_data:
        return False, project_data

    return True, project_data


def measurement_exists(cur, project_key, id):
    """
    Check whether the measurement exists in the database in the given project.
    """
    measurement = cur.execute(
        "SELECT * FROM measurements WHERE id=? AND projects_key=?", (id, project_key)
    ).fetchone()

    if not measurement:
        return False

    return True


def update_last_modified(cur, key):
    """
    Update the last modified timestamp of the project in the database.
    """
    cur.execute(
        "UPDATE projects SET timestamp_modified=CURRENT_TIMESTAMP WHERE \"key\"=?", (key,)
    )


def get_image_path(file_name):
    """
    Get the full path to the file with the given image.
    """
    return os.path.join(current_app.root_path, current_app.config["UPLOAD_FOLDER"], file_name)


@bp.route("/projects", methods=["POST"])
def create_project():
    """
    This endpoint should be called to create a project and can contain the following field:
        - name (string, optional): name of the project

    Returned JSON elements
    ----------------------
        - key (string): a unique experiment key
    """
    data = request.get_json() or {}
    key = str(uuid.uuid4())

    con = get_db()
    con.execute(
        "INSERT INTO projects (key, name) VALUES (?, ?)", (key, data.get("name", None))
    )
    con.commit()
    close_db()

    return jsonify(key=key, name=data.get("name", "")), 201


@bp.route("/projects/<project_key>", methods=["DELETE"])
def delete_project(project_key):
    """
    This endpoint should be called to delete a project.

    Returned JSON elements
    ----------------------
        - None
    """
    con = get_db()
    cur = con.cursor()

    project_found, _ = project_exists(cur, project_key)
    if project_found:
        # Deleting the project will delete all related data with foreign key constraints.
        cur.execute(
            "DELETE FROM projects WHERE \"key\"=?", (project_key,)
        )

        con.commit()
        close_db()
        return "", 204
    else:
        close_db()
        return error_response(404, "Project not found")


@bp.route("/projects/<project_key>/measurements", methods=["POST"])
def add_measurements(project_key):
    """
    This endpoint should be called to add measurements to a project and must contain the
    following fields:
        - measurements (list of dicts): a list of measurements data
            - type (string): type of measurement, e.g. distance
            - value (float): value of the measurement
            - markers (list of float lists): a list of 3D points used to take the measurement

    Returned JSON elements
    ----------------------
        - ids (list of int): the ids of the added measurements.
    """
    data = request.get_json() or {}
    if "measurements" not in data:
        return error_response(400, "measurements required")

    ids = []

    con = get_db()
    cur = con.cursor()

    project_found, _ = project_exists(cur, project_key)
    if project_found:
        for measurement in data["measurements"]:
            cur.execute(
                "INSERT INTO measurements (projects_key, type, value) VALUES (?, ?, ?)",
                (project_key, measurement["type"], measurement["value"])
            )

            id = cur.lastrowid
            ids.append(id)
            for m in measurement["markers"]:
                cur.execute(
                    "INSERT INTO markers (measurements_id, x, y, z) VALUES (?, ?, ? ,?)",
                    (id, m[0], m[1], m[2])
                )

            for s in measurement.get("snapshots", []):
                cur.execute(
                    "INSERT INTO snapshots (file_name, measurements_id) VALUES (?, ?)", (s, id)
                )

        update_last_modified(cur, project_key)
        con.commit()
        close_db()

        return jsonify(ids=ids), 201
    else:
        close_db()
        return error_response(404, "Project not found")


def delete_measurements(cur, project_key, measurement_ids):
    """
    Delete a list of measurements from the database.
    """
    print(measurement_ids)
    snapshots = cur.execute(
        "SELECT file_name FROM snapshots " +
        "WHERE measurements_id IN ({})".format(", ".join("?" for _ in measurement_ids)), measurement_ids
    ).fetchall()

    # Deleting the measurement also deletes the markers and snapshots because of the foreign key constraints.
    cur.executemany(
        "DELETE FROM measurements WHERE projects_key=? AND id=?", [(project_key, id) for id in measurement_ids]
    )

    for s in snapshots:
        os.remove(os.path.join(current_app.root_path, current_app.config["UPLOAD_FOLDER"], s["file_name"]))


def update_measurements_value(cur, project_key, measurements):
    """
    Replace the value and markers of a list of measurements.
    """
    print("efhkwg")
    cur.executemany(
        "UPDATE measurements SET value=? WHERE id=? AND projects_key=?",
        [(m["value"], m["id"], project_key) for m in measurements]
    )

    cur.executemany(
        "DELETE FROM markers WHERE measurements_id=?", [(m["id"],) for m in measurements]
    )

    cur.executemany(
        "INSERT INTO markers (measurements_id, x, y, z) VALUES (?, ?, ? ,?)",
        [(m["id"], marker[0], marker[1], marker[2]) for m in measurements for marker in m["markers"]]
    )


@bp.route("/projects/<project_key>/measurements", methods=["PATCH"])
def update_measurements(project_key):
    """
    This endpoint should be called to update measurements of a project and must contain
    the following field:
        - remove (list of int): ids of measurments to delete.
        - replace (list of dict): a list of measurements to update.
            - id (int): id of the modified measurment.
            - value (float): new value of the measurement.
            - markers (list of float lists): new list of 3D points used to take the measurement

    Returned JSON elements
    ----------------------
        - None
    """
    data = request.get_json() or {}
    if "remove" not in data and "replace" not in data:
        return error_response(400, "delete and/or replace required")

    con = get_db()
    cur = con.cursor()
    project_found, _ = project_exists(cur, project_key)

    if project_found:
        if "remove" in data:
            delete_measurements(cur, project_key, data["remove"])

        if "replace" in data:
            update_measurements_value(cur, project_key, data["replace"])

        update_last_modified(cur, project_key)
        con.commit()
        close_db
        return "", 204
    else:
        close_db()
        return error_response(404, "Project not found")


@bp.route("/projects/<project_key>/measurements/<measurement_id>/snapshots", methods=["POST"])
def add_snapshots(project_key, measurement_id):
    """
    This endpoint should be called to add a snapshot to a measurement. The image is received as
    multipart/form-data.

    Returned JSON elements
    ----------------------
        - file_names (list of string): the names of the stored images.
    """
    if not request.files:
        return error_response(404, "missing files")

    con = get_db()
    cur = con.cursor()
    project_found, _ = project_exists(cur, project_key)

    if project_found:
        if not measurement_exists(cur, project_key, measurement_id):
            close_db
            return error_response(404, "Measurement not found")

        file_names = []
        for file in request.files.getlist("file"):
            if file and allowed_file(file.filename):
                file_name = secure_filename(str(uuid.uuid4()) + "-" + file.filename)
                file.save(get_image_path(file_name))
            else:
                close_db
                return error_response(400, "Invalid image, should be PNG or JPG")

            cur.execute(
                "INSERT INTO snapshots (file_name, measurements_id) VALUES (?, ?)", (file_name, measurement_id)
            )

            file_names.append(file_name)

        update_last_modified(cur, project_key)
        con.commit()
        close_db()

        return jsonify({"file_names": file_names}), 201
    else:
        close_db()
        return error_response(404, "Project not found")


@bp.route("/projects/<project_key>/measurements/<measurement_id>/snapshots", methods=["PATCH"])
def delete_snapshots(project_key, measurement_id):
    """
    This endpoint should be called to delete snapshots from a project and must contain
    the following field:
        - remove (list of string): list of snapshot file names

    Returned JSON elements
    ----------------------
        - None
    """
    data = request.get_json() or {}

    if "remove" not in data:
        return error_response(400, "remove required")
    con = get_db()
    cur = con.cursor()
    project_found, _ = project_exists(cur, project_key)

    if project_found:
        for file_name in data["remove"]:
            if cur.execute("SELECT * FROM snapshots WHERE file_name=? AND measurements_id=?",
                           (file_name, measurement_id)).fetchone():
                cur.execute(
                    "DELETE FROM snapshots WHERE file_name=?", (file_name, )
                )

                try:
                    os.remove(get_image_path(file_name))
                except OSError:
                    pass

        update_last_modified(cur, project_key)
        con.commit()
        close_db()

        return "", 204
    else:
        close_db()
        return error_response(404, "Project not found")


def project_data_to_json_csv(project_data, measurements_data, markers_data, snapshots_data):
    csv_file = io.StringIO()
    writer = csv.writer(csv_file)
    writer.writerow(["Type", "Value", "Snapshots"])

    json_dict = {
        "name": project_data["name"],
        "timestamp_created": str(project_data["timestamp_created"]),
        "timestamp_modified": str(project_data["timestamp_modified"]),
        "measurements": {}
    }

    for measurement in measurements_data:
        snapshots = [snapshot["file_name"] for snapshot in snapshots_data if snapshot["measurements_id"] == measurement["id"]]
        json_dict["measurements"][measurement["id"]] = {
            "id": measurement["id"],
            "type": measurement["type"],
            "value": measurement["value"],
            "markers": [[marker["x"], marker["y"], marker["z"]] for marker in markers_data
                        if marker["measurements_id"] == measurement["id"]],
            "snapshots": snapshots
        }

        writer.writerow([measurement["type"], measurement["value"],  *snapshots])

    json_dict["measurements"] = list(json_dict["measurements"].values())
    return json_dict, csv_file


@bp.route("/projects/<project_key>", methods=["GET"])
def get_project_data(project_key):
    """
    This endpoint should be called to get all data of a project in a zip file..

    Returns
    -------
    A zip file with:
        - All the snapshots in the project.
        - The project data in a CSV file containing the type, value, and snaphshot names for each
          measurment.
        - The project data in a JSON file in the format:
            - measurements (list of dict):
                - type (string): type of measurement, e.g. distance
                - value (float): value of the measurement
                - markers (list of float lists): a list of 3D points used to take the measurement
                - snapshots (list of string): a list of image file names
            - name (string): name of the project
            - timestamp_created (datetime): timestamp of when the project was created
            - timestamp_modified (datetime): timestamp of when the project was last modified
    """
    con = get_db()
    cur = con.cursor()
    project_found, project_data = project_exists(cur, project_key)

    if project_found:
        measurements_data = cur.execute(
            "SELECT * FROM measurements WHERE projects_key=?", (project_key,)
        ).fetchall()
        markers_data = cur.execute(
            "SELECT DISTINCT measurements_id, x, y, z FROM measurements " +
            "INNER JOIN markers ON measurements.id=markers.measurements_id " +
            "WHERE projects_key=?", (project_key,)
        ).fetchall()
        snapshots_data = cur.execute(
            "SELECT measurements_id, file_name FROM measurements " +
            "INNER JOIN snapshots ON measurements.id=snapshots.measurements_id " +
            "WHERE projects_key=?", (project_key,)
        ).fetchall()
        close_db()

        json_dict, csv_file = project_data_to_json_csv(project_data, measurements_data, markers_data, snapshots_data)

        zip_file = io.BytesIO()
        with zipfile.ZipFile(zip_file, "w") as zf:
            zf.writestr("data.json", json.dumps(json_dict))
            zf.writestr("data.csv", csv_file.getvalue())

            for snapshot in snapshots_data:
                zf.write(get_image_path(snapshot["file_name"]), "snapshots/" + snapshot["file_name"])

        zip_file.seek(0)

        response = make_response(zip_file.read())
        response.headers.set("Content-Type", "application/zip")
        response.headers.set("Content-Disposition", "attachment",
                             filename="{}.zip".format(project_data["name"] if project_data["name"] else project_key))
        return response
    else:
        close_db()
        return error_response(404, "Project not found")
