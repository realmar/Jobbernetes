#!/usr/bin/env python3

import __init__
import deploy.infrastructure as infrastructure
import deploy.helm as helm
import deploy.jobbernetes as jobbernetes


def deploy():
    infrastructure.deploy()
    helm.deploy()
    jobbernetes.deploy()


if __name__ == "__main__":
    deploy()
