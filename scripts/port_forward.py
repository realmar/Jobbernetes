#!/usr/bin/env python3

import subprocess
from threading import Thread
from typing import List

SERVICES = [
    ("svc/mongodb", "27017:27017"),
    # ("svc/kafka-0-external", "9094:9094")
]


def run(name, ports):
    while True:
        command = f"kubectl port-forward --namespace default {name} {ports}"
        print(command)
        subprocess.run(command.split(" "), stdout=subprocess.PIPE, stderr=subprocess.PIPE)


if __name__ == "__main__":
    threads: List[Thread] = []

    for service in SERVICES:
        thread = Thread(target=run, args=service, daemon=True)
        thread.start()
        threads.append(thread)

    for thread in threads:
        thread.join()
