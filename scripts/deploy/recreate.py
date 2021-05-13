#!/usr/bin/env python3

import __init__
import start.cluster
import deploy.all
import delete.all


def recreate():
    delete.all.delete()
    start.cluster.start()
    deploy.all.deploy()


if __name__ == "__main__":
    recreate()
