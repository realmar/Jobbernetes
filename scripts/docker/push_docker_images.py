#!/usr/bin/env python3

import __init__
from lib import get_container_images, run_shell_print


def push():
    images = get_container_images()

    for _, image in images.items():
        run_shell_print(f"docker tag {image['imageName']} {image['fqn']}")
        run_shell_print(f"docker push {image['fqn']}")


if __name__ == "__main__":
    push()
