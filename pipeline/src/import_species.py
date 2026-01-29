import json
import sys
from argparse import ArgumentParser

from db import database

collection = database["aves"]
if collection.count_documents({}) != 0:
    sys.exit()

parser = ArgumentParser()
parser.add_argument("--input-file", dest="input_file", required=True)
args = parser.parse_args()

with open(args.input_file, "r", encoding="utf-8") as f:
    data = json.load(f)

collection.insert_many(data)
