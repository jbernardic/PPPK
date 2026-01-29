from argparse import ArgumentParser

from db import database

parser = ArgumentParser()
parser.add_argument("--output-file", dest="output_file", required=True)
args = parser.parse_args()

collection = database["classifications"]

results = collection.aggregate([
    {
        "$lookup": {
            "from": "aves",
            "localField": "scientific_name",
            "foreignField": "canonicalName",
            "as": "aves_data"
        },
    },
    {"$unwind": "$aves_data"},
    {
        "$replaceRoot": {
            "newRoot": {"$mergeObjects": ["$$ROOT", "$aves_data"]}
        }
    },
    {"$project": {"aves_data": 0}}
])

counts = {}

for item in results:
    name = item["scientific_name"]
    if counts.get(name) is not None:
        counts[name] += 1
    else:
        counts[name] = 1

with open(args.output_file, "w", encoding="utf-8") as f:
    f.write("scientific_name,count\n")
    for name, count in counts.items():
        f.write(f"{name},{count}\n")
