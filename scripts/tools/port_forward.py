#!/usr/bin/env python3

import __init__
import argparse
import signal
import subprocess
from time import sleep
from threading import Thread, Event
from typing import List

SERVICES = [
    ("svc/mongodb", "27017:27017"),
    ("svc/prometheus-server", "8080:80"),
    ("svc/prometheus-aggregation-gateway", "9091:9091"),
    ("svc/grafana", "9092:80"),
    ("svc/rabbitmq", "5672:5672"),
    ("svc/rabbitmq", "15672:15672"),
    ("svc/loki", "3100:3100"),
]


def main(event, silent, name, ports):
    while event.is_set() == False:
        command = f"kubectl port-forward --namespace default {name} {ports}"
        print(command)

        args = {}
        if silent:
            args = {'stdout': subprocess.PIPE, 'stderr': subprocess.PIPE}

        process = subprocess.Popen(command.split(" "), **args)

        while True:
            rc = process.poll()
            if rc is not None:
                break
            elif event.is_set():
                process.terminate()
                process.wait()
                return

            sleep(0.5)

        if event.is_set() == False:
            print("\nRestarting Process ...\n")
            sleep(2)


if __name__ == "__main__":
    event = Event()
    threads: List[Thread] = []

    signal.signal(signal.SIGINT, lambda _, __: event.set())

    parser = argparse.ArgumentParser()
    parser.add_argument('--silent', '-s',
                        dest='silent', action='store_true')

    args = parser.parse_args()

    for service in SERVICES:
        thread = Thread(target=main, args=(event, args.silent, *service), daemon=True)
        thread.start()
        threads.append(thread)

    event.wait()

    print("\nTerminating Port-Forwarding, please wait ...\n")

    for thread in threads:
        thread.join()
