#!/usr/bin/env python3

from lib import run_shell_print, get_registry, run_shell


def does_registry_exists(name):
    result, _ = run_shell("k3d registry list", silent=True)
    return name in result


def start():
    registry = get_registry()

    if not does_registry_exists(registry['registryDns']):
        run_shell_print(f"k3d registry create {registry['registryDns']} --port {registry['registryPort']}")
    else:
        print(f"Registry {registry['registryUrl']} already exists, nothing to do")


if __name__ == "__main__":
    start()
