#!/usr/bin/env python3

import __init__
from lib import run_shell_print, get_registry, does_registry_exists


def delete():
    registry = get_registry()

    if does_registry_exists(registry['registryDns']):
        run_shell_print(f"k3d registry delete {registry['registryDns']}")
    else:
        print(f"Registry {registry['registryUrl']} does not exist, nothing to do")


if __name__ == "__main__":
    delete()
