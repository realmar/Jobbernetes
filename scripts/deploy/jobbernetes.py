#!/usr/bin/env python3

import __init__
from lib.jobbernetes import task


def deploy():
    task("update")


if __name__ == "__main__":
    deploy()
