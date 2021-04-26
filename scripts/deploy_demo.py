#!/usr/bin/env python3

import delete_cluster_and_data
import start_cluster
import deploy_all


if __name__ == "__main__":
    delete_cluster_and_data.run()
    print()

    start_cluster.run()
    print()

    deploy_all.deploy()
