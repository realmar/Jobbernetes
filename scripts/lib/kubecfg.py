#!/usr/bin/env python3

from . import is_windows, run_shell_print


def run(action, specs):
    commands = [f"kubecfg {action} {spec}" for spec in specs]
    _ = [run_shell_print(f"wsl -- {c}" if is_windows() else c) for c in commands]
