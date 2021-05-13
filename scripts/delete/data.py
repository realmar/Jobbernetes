#!/usr/bin/env python3

import __init__
import argparse
import shutil
import os
from lib import is_windows, get_volumes, run_shell


def delete(force=False, shallow=False):
    for volume in get_volumes():
        if force or not volume["keep"]:
            path = volume["host"]

            if os.path.exists(path):
                items = os.listdir(path)

                if not is_windows():
                    uid = os.getuid()

                    def do(x):
                        run_shell(f"sudo chown {uid}:{uid} {x} -R", silent=True)

                    if shallow:
                        for item in items:
                            do(os.path.join(path, item))
                    else:
                        do(path)

                if shallow:
                    for item in items:
                        file = os.path.join(path, item)
                        if os.path.isdir(file):
                            shutil.rmtree(file)
                        else:
                            os.remove(file)
                else:
                    shutil.rmtree(path)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--force', '-f',
                        dest='force', action='store_true',
                        help='force delete all volume, even those marked as keep')

    args = parser.parse_args()

    delete(args.force)
