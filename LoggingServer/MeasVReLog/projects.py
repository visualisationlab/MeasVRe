from flask import (
    Blueprint, render_template
)

bp = Blueprint("projects", __name__)

@bp.route("/")
def index():
    return render_template("index.html")

@bp.route("/projects/<project_key>", methods=["GET"])
def projects():
    return render_template("project.html")
