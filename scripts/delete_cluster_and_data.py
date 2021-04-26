#!/usr/bin/env python3

import shutil
import os
from lib import get_volumes, run_shell
import delete_cluster


def run():
    delete_cluster.run()

    for volume in get_volumes():
        uid = os.getuid()
        path = volume["host"]

        if os.path.exists(path):
            run_shell(f"sudo chown {uid}:{uid} {path} -R", silent=True)
            shutil.rmtree(path)


if __name__ == "__main__":
    run()
