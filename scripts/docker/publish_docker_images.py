#!/usr/bin/env python3

import __init__
import docker.build_docker_images as build_docker_images
import docker.push_docker_images as push_docker_images


def publish():
    build_docker_images.build()
    push_docker_images.push()


if __name__ == "__main__":
    publish()
