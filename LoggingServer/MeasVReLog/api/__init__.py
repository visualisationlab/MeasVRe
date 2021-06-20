from flask import Blueprint

bp = Blueprint("measvre-api", __name__)

from MeasVReLog.api import api
