#!/usr/bin/env python3

from lib import get_container_images, run_shell_print

if __name__ == "__main__":
    images = get_container_images()

    for _,image in images.items():
        run_shell_print(f"docker tag {image['imageName']} {image['fqdn']}")
        run_shell_print(f"docker push {image['fqdn']}")
