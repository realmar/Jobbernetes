#!/usr/bin/env python3

import __init__
from lib import run_shell_print, get_registry, does_registry_exists


def start():
    registry = get_registry()

    if not does_registry_exists(registry['registryDns']):
        run_shell_print(f"k3d registry create {registry['registryDns']} --port {registry['registryPort']}")
    else:
        print(f"Registry {registry['registryUrl']} already exists, nothing to do")


if __name__ == "__main__":
    start()
