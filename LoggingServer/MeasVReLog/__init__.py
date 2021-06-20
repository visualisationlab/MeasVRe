#!/usr/bin/env python3

import os

from flask import Flask
from flask_cors import CORS


def create_app():
    app = Flask(__name__,  instance_relative_config=True)
    app.config.from_mapping(
        SECRET_KEY="dev",
        DATABASE=os.path.join(app.instance_path, "measvre.sqlite"),
        UPLOAD_FOLDER="static/snapshots",
        ALLOWED_EXTENSIONS=["png", "jpg"]
    )

    # Ensure the instance folder exists
    try:
        os.makedirs(app.instance_path)
    except OSError:
        pass

    from . import db
    db.init_app(app)

    from . import projects
    app.register_blueprint(projects.bp)

    from MeasVReLog.api import bp as api_bp
    app.register_blueprint(api_bp, url_prefix="/measvre-api")

    CORS(app)
    return app
