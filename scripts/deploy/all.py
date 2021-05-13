#!/usr/bin/env python3

import __init__
import deploy.kubernetes as kubernetes
import deploy.helm as helm
import deploy.jobbernetes as jobbernetes


def deploy():
    kubernetes.deploy()
    helm.deploy()
    jobbernetes.deploy()


if __name__ == "__main__":
    deploy()
