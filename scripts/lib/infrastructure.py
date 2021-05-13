#!/usr/bin/env python3

import __init__
from . import kubecfg


def task(action):
    commands = [
        "kubernetes-specs/storage.jsonnet",
        "kubernetes-specs/pod-reader-rbac.jsonnet",
    ]

    kubecfg.run(action, commands)


if __name__ == "__main__":
    task()
