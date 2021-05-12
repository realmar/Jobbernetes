#!/usr/bin/env python3

from lib import run_shell_print, get_registry


def delete():
    registry = get_registry()
    run_shell_print(f"k3d registry delete {registry['registryDns']}")


if __name__ == "__main__":
    delete()
