#!/usr/bin/env python3

import __init__
import argparse
import delete.cluster as cluster
import delete.registry as registry
import delete.data as data


def delete(force=False):
    if force:
        print("Force delete everything is activated")

    cluster.delete()

    if force:
        registry.delete()

    data.delete(force)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--force', '-f',
                        dest='force', action='store_true',
                        help='force delete all volume, even those marked as keep')

    args = parser.parse_args()

    delete(args.force)
