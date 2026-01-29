import sys
from argparse import ArgumentParser
from pathlib import Path
from uuid import uuid4
import requests

from storage import BUCKET, client as storage_client
from db import database

API_URL = "https://aves.regoch.net/api/classify"

parser = ArgumentParser()
parser.add_argument("--input-file", dest="input_file", required=True)
args = parser.parse_args()

file_extension = Path(args.input_file).suffix
object_name = str(uuid4()) + file_extension
storage_client.upload_file(args.input_file, BUCKET, object_name)

try:
    with open(args.input_file, "rb") as f:
        files = {"file": (args.input_file, f, "audio/mpeg")}
        response = requests.post(API_URL, files=files)
    data = response.json()

    collection = database["classifications"]
    for result in data["results"]:
        result["audio_object_name"] = object_name
    collection.insert_many(data["results"])
except:
    print(f"Unable to classify audio file \"{args.input_file}\"!", file=sys.stderr)
    sys.exit(0)
