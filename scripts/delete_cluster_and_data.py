#!/usr/bin/env python3

import argparse
import shutil
import os
from lib import get_volumes, run_shell
import delete_cluster


def run(force=False):
    if force:
        print("Force delete everything is activated")

    delete_cluster.run()

    for volume in get_volumes():
        if force or not volume["keep"]:
            uid = os.getuid()
            path = volume["host"]

            if os.path.exists(path):
                run_shell(f"sudo chown {uid}:{uid} {path} -R", silent=True)
                shutil.rmtree(path)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--force', '-f',
                        dest='force', action='store_true',
                        help='force delete all volume, even those marked as keep')

    args = parser.parse_args()

    run(args.force)
