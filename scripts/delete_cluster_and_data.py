#!/usr/bin/env python3

import shutil
import os
from lib import get_volumes, run_shell
import delete_cluster

if __name__ == "__main__":
    delete_cluster.run()

    for volume in get_volumes():
        uid = os.getuid()
        path = volume["host"]

        run_shell(f"sudo chown {uid}:{uid} {path} -R")
        shutil.rmtree(path)
