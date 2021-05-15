#!/usr/bin/env python3

import __init__
import argparse
from lib import print_big_header
from delete import cluster_and_data as delete_cluster_and_data
from start import cluster as start_cluster
from deploy import all as deploy_all
from docker import publish_docker_images


def deploy(force):
    print_big_header('Deleting old cluster with data')
    delete_cluster_and_data.delete(force=force)

    print_big_header('Creating new cluster')
    start_cluster.start()

    print_big_header('Publishing container images')
    publish_docker_images.publish()

    print_big_header('Deploying kubernetes objects and helm charts')
    deploy_all.deploy()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--force', '-f',
                        dest='force', action='store_true',
                        help='force delete all volume, even those marked as keep')

    args = parser.parse_args()

    deploy(args.force)
