#!/usr/bin/env python3

import __init__
from lib.jobbernetes import task


def delete():
    task("delete")


if __name__ == "__main__":
    delete()
