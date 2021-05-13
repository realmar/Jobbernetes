#!/usr/bin/env python3

import __init__
import argparse
import shutil
import os
from lib import is_windows, get_volumes, run_shell
import delete.cluster as cluster
import delete.registry as registry
import delete.data as data


def run(force=False):
    if force:
        print("Force delete everything is activated")

    cluster.run()

    if force:
        registry.delete()

    data.run(force)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--force', '-f',
                        dest='force', action='store_true',
                        help='force delete all volume, even those marked as keep')

    args = parser.parse_args()

    run(args.force)
