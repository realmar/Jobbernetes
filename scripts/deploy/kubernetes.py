#!/usr/bin/env python3

import __init__
from lib import is_windows, run_shell_print


def deploy():
    commands = [
        "kubecfg update kubernetes-specs/storage.jsonnet",
        "kubecfg update kubernetes-specs/pod-reader-rbac.jsonnet",
    ]

    _ = [run_shell_print(f"wsl -- {c}" if is_windows() else c) for c in commands]


if __name__ == "__main__":
    deploy()
