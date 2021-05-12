#!/usr/bin/env python3

from lib import get_container_images, run_shell_print


def build():
    images = get_container_images()

    for _, image in images.items():
        run_shell_print(f"docker build -t {image['imageName']} -f job-system/Dockerfile.{image['relativeName']} job-system")


if __name__ == "__main__":
    build()
