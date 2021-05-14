#!/usr/bin/env python3

import __init__
import argparse
from delete import cluster_and_data as delete_cluster_and_data
from start import cluster as start_cluster
from deploy import all as deploy_all
from docker import publish_docker_images


def deploy(force):
    print()
    print('################################################################')
    print('## Deleting old cluster with data')
    print('################################################################')
    print()

    delete_cluster_and_data.delete(force=force)

    print()
    print('################################################################')
    print('## Creating new cluster')
    print('################################################################')
    print()

    start_cluster.start()

    print()
    print('################################################################')
    print('## Publishing container images')
    print('################################################################')
    print()

    publish_docker_images.publish()

    print()
    print('################################################################')
    print('## Deploying kubernetes objects and helm charts')
    print('################################################################')
    print()

    deploy_all.deploy()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--force', '-f',
                        dest='force', action='store_true',
                        help='force delete all volume, even those marked as keep')

    args = parser.parse_args()

    deploy(args.force)
