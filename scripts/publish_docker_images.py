#!/usr/bin/env python3

import build_docker_images
import push_docker_images


def publish():
    build_docker_images.build()
    push_docker_images.push()


if __name__ == "__main__":
    publish()
