#!/usr/bin/env python3

import __init__
import argparse
import uuid
import requests
import signal
from threading import Thread, Event
from time import sleep
from lib import get_components


def work(event: Event, delay: float):
    print("Request thread started")

    while not event.is_set():
        try:
            req = requests.put(f"http://{dns}/Images?name={uuid.uuid4()}")

            if req.status_code != 200:
                print(f"Request failed {req.reason}")

            sleep(delay)
        except Exception as e:
            print(str(e))


if __name__ == "__main__":
    components = get_components()
    dns = components["input_service"]["ingress"]

    parser = argparse.ArgumentParser()
    parser.add_argument('delay', metavar='delay', type=float)
    parser.add_argument('-p', metavar='num', dest='parallel', type=int, default=1)

    args = parser.parse_args()

    delay = args.delay
    parallel = args.parallel

    event = Event()
    threads = [Thread(target=work, args=(event, delay)) for _ in range(parallel)]

    signal.signal(signal.SIGINT, lambda _, __: event.set())

    [t.start() for t in threads]

    event.wait()

    [t.join() for t in threads]
