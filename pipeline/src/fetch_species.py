from argparse import ArgumentParser
import requests

API_URL = "https://aves.regoch.net/aves.json"

parser = ArgumentParser()
parser.add_argument("--output-file", dest="output_file", required=True)
args = parser.parse_args()

response = requests.get(API_URL)
with open(args.output_file, "wb") as f:
    f.write(response.content)
