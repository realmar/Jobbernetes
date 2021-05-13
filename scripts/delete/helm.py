#!/usr/bin/env python3

import __init__
from lib import run_shell_print
from deploy.helm import charts


def delete():
    for name in (x[0] for x in charts):
        try:
            run_shell_print(f"helm delete {name}")
        except:
            pass


if __name__ == "__main__":
    delete()
