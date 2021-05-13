#!/usr/bin/env python3

import __init__
from lib import infrastructure


def deploy():
    infrastructure.task('update')


if __name__ == "__main__":
    deploy()
