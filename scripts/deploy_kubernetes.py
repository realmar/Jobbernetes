#!/usr/bin/env python3

from lib import is_windows, run_shell_print


def deploy():
    commands = [
        "kubecfg update kubernetes-specs/storage.jsonnet",
        "kubecfg update kubernetes-specs/kafka-ui.jsonnet",
    ]
    _ = [run_shell_print(f"wsl -- {c}" if is_windows() else c) for c in commands]


if __name__ == "__main__":
    deploy()
