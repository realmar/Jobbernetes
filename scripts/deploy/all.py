#!/usr/bin/env python3

import __init__
from lib import print_big_header
import deploy.rancher as rancher
import deploy.infrastructure as infrastructure
import deploy.helm as helm
import deploy.jobbernetes as jobbernetes


def deploy():
    print_big_header('Deploying Rancher')
    rancher.deploy()

    print_big_header('Deploying Infrastructure')
    infrastructure.deploy()

    print_big_header('Deploying Helm Charts')
    helm.deploy()

    print_big_header('Deploying Jobbernetes')
    jobbernetes.deploy()


if __name__ == "__main__":
    deploy()
