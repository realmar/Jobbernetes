#!/usr/bin/env python3

import __init__
from lib import run_shell_print, K3D_CLUSTER_NAME


def stop():
    run_shell_print(f"k3d cluster stop {K3D_CLUSTER_NAME}")


if __name__ == "__main__":
    stop()
