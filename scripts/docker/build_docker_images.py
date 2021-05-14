#!/usr/bin/env python3

import __init__
import argparse
from lib import get_container_images, run_shell_print


def build(no_cache=False):
    images = get_container_images()

    for image in images:
        run_shell_print(f"docker build " + ("--no-cache " if no_cache else "") + f"-t {image['imageName']} -f job-system/Dockerfile.{image['relativeName']} job-system")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--no-cache', '-n',
                        dest='no_cache', action='store_true',
                        help='--no-cache flag for docker build')

    args = parser.parse_args()

    build(args.no_cache)
