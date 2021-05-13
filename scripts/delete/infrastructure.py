#!/usr/bin/env python3

import __init__
from lib import infrastructure


def delete():
    infrastructure.task('delete')


if __name__ == "__main__":
    delete()
