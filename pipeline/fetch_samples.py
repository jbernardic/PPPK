import os
import random
from concurrent.futures import ThreadPoolExecutor
from random import randrange

import requests

os.makedirs("data/audio", exist_ok=True)

API_KEY = "c256158b96c2e4ebad2bfcf48c5d439fd9d724fd"

def download_file(url, output):
    with requests.get(url, stream=True) as r:
        with open(output, 'wb') as f:
            for chunk in r.iter_content(chunk_size=8192):
                if chunk:
                    f.write(chunk)

def download_task(args):
    recording, index = args
    try:
        download_file(recording["file"], f"data/audio/{recording['file-name']}")
        print(f"Finished downloading {index + 1}/30")
        return True
    except Exception as e:
        print(f"Failed to download {index + 1}/30: {e}")
        return False

response = requests.get("https://xeno-canto.org/api/3/recordings", params={
    "query": "grp:birds",
    "key": API_KEY,
    "page": randrange(1, 1000)
})

data = response.json()
recordings = data["recordings"]
random_recordings = random.sample(recordings, 30)
download_tasks = [(recording, i) for i, recording in enumerate(random_recordings)]

with ThreadPoolExecutor(max_workers=10) as executor:
    results = list(executor.map(download_task, download_tasks))
