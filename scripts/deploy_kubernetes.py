#!/usr/bin/env python3

from lib import run_shell_print


def deploy():
    commands = [
        "kubecfg update kubernetes-specs/storage.jsonnet",
    ]
    _ = [run_shell_print(c) for c in commands]


if __name__ == "__main__":
    deploy()
