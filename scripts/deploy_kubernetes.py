#!/usr/bin/env python3

from lib import run_shell_print


def deploy():
    commands = [
        "kubectl apply -f kubernetes-specs/monitor",
    ]
    _ = [run_shell_print(c) for c in commands]


if __name__ == "__main__":
    deploy()
