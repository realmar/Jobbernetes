#!/usr/bin/env python3

import __init__
from delete import cluster_and_data as delete_cluster_and_data
from start import cluster as start_cluster
from deploy import all as deploy_all
from docker import publish_docker_images


if __name__ == "__main__":
    print()
    print('################################################################')
    print('## Deleting old cluster with data')
    print('################################################################')
    print()

    delete_cluster_and_data.run(force=True)

    print()
    print('################################################################')
    print('## Creating new cluster')
    print('################################################################')
    print()

    start_cluster.run()

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
